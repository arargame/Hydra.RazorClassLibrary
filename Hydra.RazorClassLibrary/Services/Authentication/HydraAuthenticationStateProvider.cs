using System.Security.Claims;
using System.Text.Json;
using Hydra.RazorClassLibrary.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Hydra.RazorClassLibrary.Services.Authentication
{
    public class HydraAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private const string AuthTokenKey = "authToken";
        private readonly AuthenticationState _anonymous;

        public HydraAuthenticationStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? token;

            try
            {
                token = await _localStorage.GetItemAsync<string>(AuthTokenKey);
            }
            catch
            {
                // Local storage needs JS interop. During prerendering there is no browser yet, so
                // the honest answer is "we don't know who this is" — anonymous. Once the circuit
                // becomes interactive this runs again and returns the real state.
                return _anonymous;
            }

            if (string.IsNullOrWhiteSpace(token))
                return _anonymous;

            if (IsExpired(token))
            {
                // An expired token would otherwise leave the UI looking signed in while every API
                // call comes back 401 — the most confusing possible state. Drop it instead.
                try { await _localStorage.RemoveItemAsync(AuthTokenKey); } catch { /* best effort */ }

                return _anonymous;
            }

            return new AuthenticationState(BuildPrincipal(token));
        }

        public void NotifyUserLogin(string token)
        {
            var authState = Task.FromResult(new AuthenticationState(BuildPrincipal(token)));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
        }

        private static ClaimsPrincipal BuildPrincipal(string token)
        {
            var claims = ParseClaimsFromJwt(token);

            //nameType/roleType AÇIKÇA veriliyor: bunlar verilmezse ClaimsIdentity kendi
            //varsayılanlarını kullanır ve User.Identity.Name ile IsInRole(...) sessizce boş döner.
            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);

            return new ClaimsPrincipal(identity);
        }

        /// <summary>
        /// JWT'de standart claim'ler KISA adlarla taşınır (JwtSecurityTokenHandler dışarı yazarken
        /// ClaimTypes.Role → "role", ClaimTypes.Name → "unique_name", ClaimTypes.NameIdentifier →
        /// "nameid", ClaimTypes.Email → "email" olarak kısaltır).
        ///
        /// Önceki hâl token'da uzun URI'yi (ClaimTypes.Role) arıyordu; token'da öyle bir anahtar
        /// hiç olmadığı için rol claim'i HİÇ bulunamıyordu — yani &lt;AuthorizeView Roles="Admin"&gt;
        /// ve [Authorize(Roles = ...)] sessizce hep başarısız oluyordu. Burada kısa adlar tekrar
        /// uzun ClaimTypes karşılıklarına eşleniyor.
        /// </summary>
        private static readonly Dictionary<string, string> ShortClaimNameMap = new()
        {
            ["role"] = ClaimTypes.Role,
            ["roles"] = ClaimTypes.Role,
            ["nameid"] = ClaimTypes.NameIdentifier,
            ["sub"] = ClaimTypes.NameIdentifier,
            ["unique_name"] = ClaimTypes.Name,
            ["email"] = ClaimTypes.Email,
        };

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();

            var parts = jwt.Split('.');
            if (parts.Length < 2)
                return claims;

            var jsonBytes = ParseBase64WithoutPadding(parts[1]);

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

            if (keyValuePairs is null)
                return claims;

            foreach (var pair in keyValuePairs)
            {
                // Both the short JWT spelling and the long ClaimTypes URI resolve to the same
                // claim type, so a token written either way produces a usable principal.
                var claimType = ShortClaimNameMap.TryGetValue(pair.Key, out var mapped)
                    ? mapped
                    : pair.Key;

                // A claim that appears more than once (roles, permissions) arrives as an array.
                if (pair.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in pair.Value.EnumerateArray())
                    {
                        var value = ElementToString(element);

                        if (!string.IsNullOrEmpty(value))
                            claims.Add(new Claim(claimType, value));
                    }
                }
                else
                {
                    var value = ElementToString(pair.Value);

                    if (!string.IsNullOrEmpty(value))
                        claims.Add(new Claim(claimType, value));
                }
            }

            return claims;
        }

        private static string? ElementToString(JsonElement element)
            => element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => element.ToString()
            };

        /// <summary>Reads the standard "exp" claim (Unix seconds) and says whether it has passed.</summary>
        private static bool IsExpired(string jwt)
        {
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2)
                    return true;

                var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ParseBase64WithoutPadding(parts[1]));

                if (payload is null || !payload.TryGetValue("exp", out var exp))
                    return false; // no expiry claim → let the server be the judge

                var seconds = exp.ValueKind == JsonValueKind.Number
                    ? exp.GetInt64()
                    : long.Parse(exp.ToString());

                return DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow;
            }
            catch
            {
                // Unreadable token is a broken token.
                return true;
            }
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            base64 = base64.Replace('-', '+').Replace('_', '/');

            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            return Convert.FromBase64String(base64);
        }
    }
}

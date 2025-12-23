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
            var token = await _localStorage.GetItemAsync<string>(AuthTokenKey);

            if (string.IsNullOrWhiteSpace(token))
            {
                return _anonymous;
            }

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            
            return new AuthenticationState(user);
        }

        public void NotifyUserLogin(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            
            var authState = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                keyValuePairs.TryGetValue(ClaimTypes.Role, out object? roles);

                if (roles != null)
                {
                    if (roles.ToString().Trim().StartsWith("["))
                    {
                        var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                        if (parsedRoles != null)
                        {
                            foreach (var parsedRole in parsedRoles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                            }
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                    }

                    keyValuePairs.Remove(ClaimTypes.Role);
                }

                claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}

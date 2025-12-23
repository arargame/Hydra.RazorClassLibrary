using Hydra.DTOs.ModelDTOs.SystemUserDTO;
using Hydra.Http;
using Hydra.RazorClassLibrary.Services.Http;
using Hydra.RazorClassLibrary.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Hydra.RazorClassLibrary.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpClientService _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        private const string AuthTokenKey = "authToken";

        public AuthenticationService(
            IHttpClientService httpClient,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<LoginViewDTO?> Login(LoginViewDTO loginModel)
        {
            // Login endpoint expects LoginViewDTO, returns ResponseObject<LoginViewDTO>
            // Note: Adjust URL if base address is not set globally, but HttpClient usually has BaseAddress.
            var response = await _httpClient.PostAsync<LoginViewDTO, ResponseObject<LoginViewDTO>>("api/SystemUser/Login", loginModel);

            if (response != null && response.Success && response.Data != null)
            {
                var resultDto = response.Data;
                
                // Save token to local storage
                if (!string.IsNullOrEmpty(resultDto.Token))
                {
                    await _localStorage.SetItemAsync(AuthTokenKey, resultDto.Token);
                    
                    // Notify auth state provider
                    if (_authStateProvider is HydraAuthenticationStateProvider hydraAuth)
                    {
                        hydraAuth.NotifyUserLogin(resultDto.Token);
                    }
                }

                return resultDto;
            }

            return null;
        }

        public async Task Logout()
        {
            // Call API Logout if necessary (optional depending on API design)
            // await _httpClient.PostAsync("api/SystemUser/Logout", null);

            await _localStorage.RemoveItemAsync(AuthTokenKey);

            if (_authStateProvider is HydraAuthenticationStateProvider hydraAuth)
            {
                hydraAuth.NotifyUserLogout();
            }
        }
    }
}

using Hydra.RazorClassLibrary.Services.Authentication;
using Hydra.RazorClassLibrary.Services.Http;
using Hydra.RazorClassLibrary.Services.Logging;
using Hydra.RazorClassLibrary.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Hydra.RazorClassLibrary.Utils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHydraRazorLibrary(this IServiceCollection services)
        {
            // Authorization Core servisini ekler (Blazor için)
            services.AddAuthorizationCore();

            // HTTP Servisi
            services.AddScoped<IHttpClientService, HttpClientService>();
            
            // Client Log Servisi (Session-scoped CorrelationId için)
            services.AddScoped<IClientLogService, ClientLogService>();

            // Depolama Servisi (LocalStorage)
            services.AddScoped<ILocalStorageService, LocalStorageService>();

            // Kimlik Doğrulama Servisleri
            services.AddScoped<HydraAuthenticationStateProvider>();
            
            // AuthenticationStateProvider olarak da kaydet ki Blazor bulabilsin
            services.AddScoped<AuthenticationStateProvider>(provider => 
                provider.GetRequiredService<HydraAuthenticationStateProvider>());

            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services;
        }
    }
}

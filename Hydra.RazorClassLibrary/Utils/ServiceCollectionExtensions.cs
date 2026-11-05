using Hydra.RazorClassLibrary.Services.Authentication;
using Hydra.RazorClassLibrary.Services.Core;
using Hydra.RazorClassLibrary.Services.Http;
using Hydra.RazorClassLibrary.Services.Logging;
using Hydra.RazorClassLibrary.Services.Status;
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

            // AuthenticationState'i bütün bileşen ağacına cascading parametre olarak yayar.
            // Bu olmadan <AuthorizeView> ve HydraRouteGuard'ın [CascadingParameter] Task<AuthenticationState>
            // alanı null gelir — yani koruma sessizce devre dışı kalır.
            services.AddCascadingAuthenticationState();

            // HTTP durum kodlarının kullanıcıya ulaşmasını sağlayan bildirici.
            // HttpClientService bunu kullandığı için ondan ÖNCE kayıtlı olmalı (DI sırası
            // aslında önemli değil, ama okunurluk için burada duruyor).
            services.AddScoped<HydraStatusNotifier>();

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

            // Generic API client: herhangi bir entity için ApiClient<T> inject edilebilir
            services.AddScoped(typeof(ApiClient<>));

            // CustomFile'a özgü Upload/Download uçlarını taşıyan client — CustomFile için
            // ApiClient<CustomFile> yerine bunu inject edin (bkz. CustomFileClient.cs).
            services.AddScoped<CustomFileClient>();

            // FK / enum / boolean dropdown seçenekleri
            services.AddScoped<LookupService>();

            // Kullanıcı grid/filtre görünüm state ve tercih yöneticisi (Sayfalar arası filtre koruma)
            services.AddScoped<ITableStateManager, TableStateManager>();

            return services;
        }
    }
}

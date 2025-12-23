using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Hydra.RazorClassLibrary.Services.Logging
{
    public class ClientLogService : IClientLogService
    {
        private readonly HttpClient _httpClient;
        private readonly string _correlationId;
        private readonly Guid _platformId;

        public ClientLogService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            
            // Generate session-scoped CorrelationId
            _correlationId = Guid.NewGuid().ToString();
            
            // Read PlatformId from config
            var platformIdString = configuration["Hydra:PlatformId"];
            if (!Guid.TryParse(platformIdString, out _platformId))
            {
                throw new InvalidOperationException("Hydra:PlatformId not configured in appsettings.json");
            }
        }

        public async Task LogErrorAsync(string message, string? stackTrace = null, string? url = null, int? statusCode = null)
        {
            try
            {
                var request = new ClientLogRequest
                {
                    Message = message,
                    StackTrace = stackTrace,
                    Url = url,
                    StatusCode = statusCode,
                    CorrelationId = _correlationId,
                    PlatformId = _platformId,
                    Timestamp = DateTime.UtcNow
                };

                await _httpClient.PostAsJsonAsync("/api/clientlog", request);
            }
            catch
            {
                // Silent fail - logging failure should not crash the app
                Console.WriteLine($"[{_correlationId}] Failed to log error to backend: {message}");
            }
        }
        public async Task LogWarningAsync(string message, string? url = null)
        {
             try
            {
                var request = new ClientLogRequest
                {
                    Message = "[WARNING] " + message,
                    Url = url,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = _correlationId,
                    PlatformId = _platformId
                };

                await _httpClient.PostAsJsonAsync("/api/clientlog", request);
            }
            catch
            {
                // Silent fail
            }
        }
    }

    /// <summary>
    /// DTO for client error log requests
    /// </summary>
    public class ClientLogRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? Url { get; set; }
        public int? StatusCode { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public Guid PlatformId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

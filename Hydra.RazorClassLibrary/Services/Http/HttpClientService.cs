using Hydra.RazorClassLibrary.Services.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Hydra.Http;

namespace Hydra.RazorClassLibrary.Services.Http
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IClientLogService _clientLogService;
        private readonly JsonSerializerOptions _jsonOptions;

        public HttpClientService(HttpClient httpClient, IClientLogService clientLogService)
        {
            _httpClient = httpClient;
            _clientLogService = clientLogService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<TResponse?> GetAsync<TResponse>(string url)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<TResponse>(url, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                await HandleHttpError(ex, url);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                await HandleTimeout(ex, url);
                throw;
            }
            catch (Exception ex)
            {
                await HandleUnexpectedError(ex, url);
                throw;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions);
                response.EnsureSuccessStatusCode();

                if (response.Content.Headers.ContentLength == 0)
                    return default;

                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                await HandleHttpError(ex, url);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                await HandleTimeout(ex, url);
                throw;
            }
            catch (Exception ex)
            {
                await HandleUnexpectedError(ex, url);
                throw;
            }
        }

        public async Task PostAsync<TRequest>(string url, TRequest data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                await HandleHttpError(ex, url);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                await HandleTimeout(ex, url);
                throw;
            }
            catch (Exception ex)
            {
                await HandleUnexpectedError(ex, url);
                throw;
            }
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(url, data, _jsonOptions);
                response.EnsureSuccessStatusCode();

                 if (response.Content.Headers.ContentLength == 0)
                    return default;

                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                await HandleHttpError(ex, url);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                await HandleTimeout(ex, url);
                throw;
            }
            catch (Exception ex)
            {
                await HandleUnexpectedError(ex, url);
                throw;
            }
        }

        public async Task<TResponse?> DeleteAsync<TResponse>(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();

                 if (response.Content.Headers.ContentLength == 0)
                    return default;

                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                await HandleHttpError(ex, url);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                await HandleTimeout(ex, url);
                throw;
            }
            catch (Exception ex)
            {
                await HandleUnexpectedError(ex, url);
                throw;
            }
        }

        // Envelope Pattern Methods
        public async Task<T?> GetEnvelopeAsync<T>(string controller, string action, Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null)
        {
            var url = Utils.UrlHelper.BuildUrl(controller, action, parameters, pathLikeParameter, queryString);
            return await GetEnvelopeAsync<T>(url);
        }

        public async Task<T?> GetEnvelopeAsync<T>(string url)
        {
            try
            {
                // We fetch ResponseObject<T> which matches the standard Hydra envelope
                var response = await _httpClient.GetFromJsonAsync<ResponseObject<T>>(url, _jsonOptions);

                if (response == null)
                {
                    await _clientLogService.LogErrorAsync($"Empty response from {url}", null, url, null);
                    return default;
                }

                if (!response.Success)
                {
                   // Create a robust log message including messages from the response
                   // Extracting messages safely
                   var messages = response.Messages != null 
                        ? string.Join("; ", response.Messages.Select(m => m.Text)) 
                        : "No error messages provided.";

                   // Log as Warning because it's a handled business error, not a crash
                   await _clientLogService.LogWarningAsync(
                        message: $"Business Failure at {url}: {messages}"
                   );
                   
                   return default;
                }

                return response.Data;
            }
            catch (Exception ex)
            {
                await HandleUnexpectedError(ex, url);
                return default;
            }
        }

        public async Task<T?> PostEnvelopeAsync<T>(string controller, string action, object payload, Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null)
        {
            var url = Utils.UrlHelper.BuildUrl(controller, action, parameters, pathLikeParameter, queryString);
            return await PostEnvelopeAsync<T>(url, payload);
        }

        public async Task<T?> PostEnvelopeAsync<T>(string url, object payload)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, payload, _jsonOptions);
                return await HandleResponseEnvelope<T>(response, url);
            }
            catch (Exception ex)
            {
                await HandleUnexpectedError(ex, url);
                return default;
            }
        }

        public async Task<T?> PutEnvelopeAsync<T>(string controller, string action, object payload, Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null)
        {
            var url = Utils.UrlHelper.BuildUrl(controller, action, parameters, pathLikeParameter, queryString);
            return await PutEnvelopeAsync<T>(url, payload);
        }

        public async Task<T?> PutEnvelopeAsync<T>(string url, object payload)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(url, payload, _jsonOptions);
                return await HandleResponseEnvelope<T>(response, url);
            }
            catch (Exception ex)
            {
                await HandleUnexpectedError(ex, url);
                return default;
            }
        }

        public async Task<T?> DeleteEnvelopeAsync<T>(string controller, string action, Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null)
        {
            var url = Utils.UrlHelper.BuildUrl(controller, action, parameters, pathLikeParameter, queryString);
            return await DeleteEnvelopeAsync<T>(url);
        }

        public async Task<T?> DeleteEnvelopeAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                return await HandleResponseEnvelope<T>(response, url);
            }
            catch (Exception ex)
            {
                await HandleUnexpectedError(ex, url);
                return default;
            }
        }

        private async Task<T?> HandleResponseEnvelope<T>(HttpResponseMessage response, string url)
        {
            // If the server returns 4xx/5xx, it might still return a ResponseObject compliant JSON.
            // But typically, a crash (500) might return HTML or pure text.
            // Let's check status code first for critical failures.
            
            if (!response.IsSuccessStatusCode)
            {
                 // If it's a pure HTTP error (e.g. 404, 500) without our envelope
                 // We use the existing error handlers
                 if (response.StatusCode >= HttpStatusCode.InternalServerError)
                 {
                     await _clientLogService.LogErrorAsync($"Server Error: {url}", response.ReasonPhrase, url, (int)response.StatusCode);
                     return default;
                 }
                 else
                 {
                     // 4xx errors - usually client side, log to console
                     Console.WriteLine($"Client Error {response.StatusCode}: {url}");
                     return default;
                 }
            }
            
            if (response.Content.Headers.ContentLength == 0)
            {
                 await _clientLogService.LogErrorAsync($"Empty response from {url}", null, url, null);
                 return default;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[HttpClientService] Raw JSON from {url}: {jsonString}");

            var envelope = JsonSerializer.Deserialize<ResponseObject<T>>(jsonString, _jsonOptions);

            if (envelope == null)
            {
                 await _clientLogService.LogErrorAsync($"Null envelope from {url}", null, url, null);
                 return default;
            }

            if (!envelope.Success)
            {
               var messages = envelope.Messages != null 
                    ? string.Join("; ", envelope.Messages.Select(m => m.Text)) 
                    : "No error messages provided.";

               await _clientLogService.LogWarningAsync(
                    message: $"Business Failure at {url}: {messages}"
               );
               
               return default;
            }

            return envelope.Data;
        }

        // Error handling helper methods
        private async Task HandleHttpError(HttpRequestException ex, string url)
        {
            var statusCode = ex.StatusCode;

            // 5xx = Server error → LOG to backend
            if (statusCode >= HttpStatusCode.InternalServerError)
            {
                await _clientLogService.LogErrorAsync(
                    message: $"Server Error: {url}",
                    stackTrace: ex.ToString(),
                    url: url,
                    statusCode: (int?)statusCode);
            }
            // 4xx = Client error → Console only (no backend log)
            else
            {
                Console.WriteLine($"Client Error {statusCode}: {url} - {ex.Message}");
            }
        }

        private async Task HandleTimeout(TaskCanceledException ex, string url)
        {
            // Timeout = Critical → LOG to backend
            await _clientLogService.LogErrorAsync(
                message: $"Request Timeout: {url}",
                stackTrace: ex.ToString(),
                url: url,
                statusCode: 408); // Request Timeout
        }

        private async Task HandleUnexpectedError(Exception ex, string url)
        {
            // Unknown error = Critical → LOG to backend
            await _clientLogService.LogErrorAsync(
                message: $"Unexpected Error: {url}",
                stackTrace: ex.ToString(),
                url: url);
        }
    }
}

using Hydra.RazorClassLibrary.Services.Logging;
using Hydra.RazorClassLibrary.Services.Status;
using Hydra.RazorClassLibrary.Services.Storage;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Hydra.Http;

namespace Hydra.RazorClassLibrary.Services.Http
{
    public class HttpClientService : IHttpClientService
    {
        private const string AuthTokenKey = "authToken";

        private readonly HttpClient _httpClient;
        private readonly IClientLogService _clientLogService;
        private readonly HydraStatusNotifier _statusNotifier;
        private readonly ILocalStorageService _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public HttpClientService(HttpClient httpClient,
                                 IClientLogService clientLogService,
                                 HydraStatusNotifier statusNotifier,
                                 ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _clientLogService = clientLogService;
            _statusNotifier = statusNotifier;
            _localStorage = localStorage;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public Uri? BaseAddress => _httpClient.BaseAddress;

        /// <summary>
        /// Attaches the bearer token stored at login to every outgoing request.
        ///
        /// Without this the API's deny-by-default filter sees every call as anonymous and answers
        /// 401 — the token was being written to local storage at login and then never used.
        /// The HttpClient is registered Scoped, so in Blazor Server these headers belong to a
        /// single circuit (a single user) and cannot leak across connections.
        /// </summary>
        private async Task EnsureAuthorizationHeaderAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>(AuthTokenKey);

                _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
                    ? null
                    : new AuthenticationHeaderValue("Bearer", token);
            }
            catch
            {
                // Local storage needs JS interop, which is unavailable during prerendering.
                // A missing header is handled properly downstream (401 → status page), so this
                // must not throw.
            }
        }

        public async Task<TResponse?> GetAsync<TResponse>(string url)
        {
            try
            {
                await EnsureAuthorizationHeaderAsync();

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    await ReportFailureAsync(response, url);
                    return default;
                }

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

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                await EnsureAuthorizationHeaderAsync();

                var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions);

                //EnsureSuccessStatusCode() BURADAN KALDIRILDI.
                //Sebep: sunucu artık reddi gerçek bir HTTP koduyla (401/403) döndürüyor ama
                //gövdede yine standart ResponseObject var. Exception fırlatmak, kullanıcıya
                //gösterilecek tek anlamlı bilgiyi (envelope'taki mesajı) çöpe atıyordu.
                //Artık gövde her durumda okunmaya çalışılıyor; başarısızlık çağırana
                //ResponseObject.Success=false olarak ulaşıyor.
                if (!response.IsSuccessStatusCode)
                {
                    await ReportFailureAsync(response, url);

                    return await TryReadBodyAsync<TResponse>(response);
                }

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
                await EnsureAuthorizationHeaderAsync();

                var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                    await ReportFailureAsync(response, url);
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
                await EnsureAuthorizationHeaderAsync();

                var response = await _httpClient.PutAsJsonAsync(url, data, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    await ReportFailureAsync(response, url);

                    return await TryReadBodyAsync<TResponse>(response);
                }

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
                await EnsureAuthorizationHeaderAsync();

                var response = await _httpClient.DeleteAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    await ReportFailureAsync(response, url);

                    return await TryReadBodyAsync<TResponse>(response);
                }

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
                await EnsureAuthorizationHeaderAsync();

                //GetFromJsonAsync yerine GetAsync: önceki hâli 4xx'te exception fırlatıyordu ve
                //gövdedeki envelope hiç okunmuyordu.
                var response = await _httpClient.GetAsync(url);

                return await HandleResponseEnvelope<T>(response, url);
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
                await EnsureAuthorizationHeaderAsync();

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
                await EnsureAuthorizationHeaderAsync();

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
                await EnsureAuthorizationHeaderAsync();

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
            //Hydra sözleşmesi: gövde HER ZAMAN bir ResponseObject. Sunucu artık reddi gerçek
            //HTTP koduyla da bildirdiği için, 4xx/5xx gelse bile gövdeyi okuyup içindeki
            //mesajları kullanıcıya ulaştırıyoruz — eskiden bu durumda gövde okunmadan
            //default dönülüyor, kullanıcı boş bir ekran görüyordu.
            if (!response.IsSuccessStatusCode)
            {
                await ReportFailureAsync(response, url);
                return default;
            }

            if (response.Content.Headers.ContentLength == 0)
            {
                await _clientLogService.LogErrorAsync($"Empty response from {url}", null, url, null);
                return default;
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            var envelope = TryDeserializeEnvelope<T>(jsonString);

            if (envelope == null)
            {
                await _clientLogService.LogErrorAsync($"Null envelope from {url}", null, url, null);
                return default;
            }

            if (!envelope.Success)
            {
                var messages = CollectMessages(envelope);

                await _clientLogService.LogWarningAsync(
                     message: $"Business Failure at {url}: {string.Join("; ", messages)}");

                //İş kuralı hatası da kullanıcıya gösterilmeli. Envelope kendi kodunu taşıyorsa
                //onu kullan, taşımıyorsa 400 (Bad Request) olarak sınıflandır.
                _statusNotifier.Raise(envelope.StatusCode ?? 400, messages, url);

                return default;
            }

            return envelope.Data;
        }

        /// <summary>
        /// Turns a non-2xx response into a user-visible status notification, preferring the
        /// envelope's own messages when the server sent one.
        /// </summary>
        private async Task ReportFailureAsync(HttpResponseMessage response, string url)
        {
            var statusCode = (int)response.StatusCode;

            var messages = new List<string>();

            try
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(jsonString))
                {
                    var envelope = TryDeserializeEnvelope<object>(jsonString);

                    if (envelope != null)
                    {
                        messages.AddRange(CollectMessages(envelope));

                        //Gövdedeki kod, HTTP kodundan daha spesifik olabilir (ör. 200 dönen ama
                        //içinde 409 taşıyan eski uçlar).
                        if (envelope.StatusCode.HasValue)
                            statusCode = envelope.StatusCode.Value;
                    }
                }
            }
            catch
            {
                // Body was not our envelope (an HTML error page, a proxy response). The status
                // code alone is still enough to tell the user something useful.
            }

            if (statusCode >= (int)HttpStatusCode.InternalServerError)
            {
                await _clientLogService.LogErrorAsync($"Server Error: {url}", response.ReasonPhrase, url, statusCode);
            }
            else
            {
                await _clientLogService.LogWarningAsync($"Client Error {statusCode}: {url}");
            }

            _statusNotifier.Raise(statusCode, messages, url);
        }

        private async Task<T?> TryReadBodyAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(jsonString))
                    return default;

                return JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        private ResponseObject<T>? TryDeserializeEnvelope<T>(string jsonString)
        {
            try
            {
                return JsonSerializer.Deserialize<ResponseObject<T>>(jsonString, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static List<string> CollectMessages(ResponseObject envelope)
        {
            if (envelope.Messages == null)
                return new List<string>();

            return envelope.Messages
                           .Where(m => !m.ShowWhenSuccess && !string.IsNullOrWhiteSpace(m.Text))
                           .Select(m => m.Text!)
                           .ToList();
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

            _statusNotifier.Raise((int?)statusCode ?? 500, new[] { ex.Message }, url);
        }

        private async Task HandleTimeout(TaskCanceledException ex, string url)
        {
            // Timeout = Critical → LOG to backend
            await _clientLogService.LogErrorAsync(
                message: $"Request Timeout: {url}",
                stackTrace: ex.ToString(),
                url: url,
                statusCode: 408); // Request Timeout

            _statusNotifier.Raise(408, null, url);
        }

        private async Task HandleUnexpectedError(Exception ex, string url)
        {
            // Unknown error = Critical → LOG to backend
            await _clientLogService.LogErrorAsync(
                message: $"Unexpected Error: {url}",
                stackTrace: ex.ToString(),
                url: url);

            _statusNotifier.Raise(500, new[] { ex.Message }, url);
        }
    }
}

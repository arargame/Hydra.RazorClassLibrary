using System;
using System.Threading.Tasks;

using System.Collections.Generic;
using Hydra.RazorClassLibrary.Services.Http;

namespace Hydra.RazorClassLibrary.Services.Http
{
    public interface IHttpClientService
    {
        Task<TResponse?> GetAsync<TResponse>(string url);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data);
        Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data);
        Task<TResponse?> DeleteAsync<TResponse>(string url);
        
        // Overloads for non-returning (void) or specific cases could be added
        Task PostAsync<TRequest>(string url, TRequest data);


        // Envelope Pattern
        Task<T?> GetEnvelopeAsync<T>(string url);
        Task<T?> GetEnvelopeAsync<T>(string controller, string action, Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null);
        
        Task<T?> PostEnvelopeAsync<T>(string url, object payload);
        Task<T?> PostEnvelopeAsync<T>(string controller, string action, object payload, Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null);

        Task<T?> PutEnvelopeAsync<T>(string url, object payload);
        Task<T?> PutEnvelopeAsync<T>(string controller, string action, object payload, Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null);

        Task<T?> DeleteEnvelopeAsync<T>(string url);
        Task<T?> DeleteEnvelopeAsync<T>(string controller, string action, Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null);
    }
}

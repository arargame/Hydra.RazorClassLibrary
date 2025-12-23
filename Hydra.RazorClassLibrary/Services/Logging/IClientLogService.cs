namespace Hydra.RazorClassLibrary.Services.Logging
{
    /// <summary>
    /// Service for logging client-side errors to the backend.
    /// Only logs critical errors (5xx, timeouts, unexpected errors).
    /// </summary>
    public interface IClientLogService
    {
        /// <summary>
        /// Logs an error to the backend asynchronously.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="stackTrace">Stack trace (optional)</param>
        /// <param name="url">Request URL that caused the error (optional)</param>
        /// <param name="statusCode">HTTP status code (optional)</param>
        Task LogErrorAsync(string message, string? stackTrace = null, string? url = null, int? statusCode = null);
        Task LogWarningAsync(string message, string? url = null);
    }
}

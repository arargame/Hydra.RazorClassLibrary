using Hydra.DTOs;
using Hydra.FileOperations;
using Hydra.RazorClassLibrary.Services.Http;

namespace Hydra.RazorClassLibrary.Services.Core
{
    /// <summary>
    /// The generic ApiClient&lt;T&gt; (registered as an open generic, see
    /// ServiceCollectionExtensions.AddHydraRazorLibrary) only covers the standard
    /// Create/Update/Delete/Get/Select surface. CustomFile needs two things beyond that,
    /// mirroring CustomFileController on the server:
    ///
    ///   - UploadAsync: posts a FileUploadDTO to CustomFile/Upload.
    ///   - GetDownloadUrl: CustomFile/Download/{id} returns a real file response, not JSON, so
    ///     it can't go through Http.GetEnvelopeAsync — the caller needs the raw URL, to open in
    ///     a new tab or hand to an &lt;a href&gt;.
    ///
    /// Registered explicitly (see ServiceCollectionExtensions) — inject this instead of
    /// ApiClient&lt;CustomFile&gt; wherever upload/download is needed.
    /// </summary>
    public class CustomFileClient : ApiClient<CustomFile>
    {
        public CustomFileClient(IHttpClientService httpClientService) : base(httpClientService) { }

        public async Task<List<CustomFile>?> UploadAsync(FileUploadDTO uploadDTO)
        {
            return await Http.PostEnvelopeAsync<List<CustomFile>>(
                controller: Controller,
                action: "Upload",
                payload: uploadDTO
            );
        }

        public async Task<List<CustomFile>?> GetByEntityAsync(string entityType, string entityId)
        {
            return await Http.GetEnvelopeAsync<List<CustomFile>>(
                controller: Controller,
                action: "ByEntity",
                pathLikeParameter: $"{entityType}/{entityId}"
            );
        }

        public string GetDownloadUrl(Guid id)
        {
            var relative = $"{Controller}/Download/{id}";

            return Http.BaseAddress != null
                ? new Uri(Http.BaseAddress, relative).ToString()
                : relative;
        }
    }
}

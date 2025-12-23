using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.Services.Storage
{
    public interface ILocalStorageService
    {
        Task SetItemAsync<T>(string key, T value);
        Task<T?> GetItemAsync<T>(string key);
        Task RemoveItemAsync(string key);
    }
}

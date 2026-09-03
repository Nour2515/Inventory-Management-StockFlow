using StockFlow.core;
using StockFlow.Models;

namespace StockFlow.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan expiration);

        Task RemoveAsync(string key);

        Task RemoveManyAsync(params string[] keys);

        Task InvalidateInventoryCacheAsync(Inventory inventory);

    }
}

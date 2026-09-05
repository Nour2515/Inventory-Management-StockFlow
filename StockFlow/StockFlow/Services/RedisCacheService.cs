using Microsoft.Extensions.Caching.Distributed;
using StockFlow.core;
using StockFlow.Interfaces;
using StockFlow.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StockFlow.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;
        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
            // Configure JSON serialization options 
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<T?> GetAsync<T>(string key)
        {

            var cachedData = await _cache.GetStringAsync(key);
            //default return null for Dtos and default value for value types
            if (string.IsNullOrEmpty(cachedData))
                return default;

            return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);

        }
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            //serialize convert value to json
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _cache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
        public async Task RemoveManyAsync(params string[] keys)
        {
            foreach (var key in keys)
            {
                await RemoveAsync(key);
            }
        }
        public async Task InvalidateInventoryCacheAsync(Inventory inventory)
        {
            await RemoveManyAsync(

                CacheKeys.InventoryAll,

                CacheKeys.InventoryById(
                    inventory.Id),

                CacheKeys.InventoryByProduct(
                    inventory.ProductId),

                CacheKeys.InventoryByWarehouse(
                    inventory.WarehouseId)
            );

        }
    }
}
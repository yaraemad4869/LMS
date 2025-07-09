// Services/IRedisCacheService.cs
using System.Text.Json;
using LearningManagementSystem.IServices;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _redis = redis;
        _logger = logger;
    }

    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (cachedData == null)
            {
                _logger.LogDebug("Cache miss for key {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data from cache for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
            };

            await _cache.SetStringAsync(key, serializedValue, options);
            _logger.LogDebug("Data cached for key {Key} with expiry {Expiry}", key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting data to cache for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogDebug("Cache removed for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing data from cache for key {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking key existence in cache for key {Key}", key);
            return false;
        }
    }
}
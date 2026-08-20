using StackExchange.Redis;
using System.Text.Json;

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration)
    {
        // implementation
    }

    public async Task RemoveAsync(string key)
    {
        // implementation
    }
}
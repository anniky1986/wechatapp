using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Agility.Zoey.Web.Core.Shared.Services;

public class CacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;

    public CacheService(IMemoryCache memoryCache, IDistributedCache? distributedCache = null)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
    }

    public bool TryGetValue<T>(string key, out T? value)
    {
        if (_memoryCache.TryGetValue(key, out value))
        {
            return true;
        }

        if (_distributedCache != null)
        {
            var bytes = _distributedCache.Get(key);
            if (bytes != null && bytes.Length > 0)
            {
                var deserialized = JsonSerializer.Deserialize<T>(bytes);
                if (deserialized != null)
                {
                    value = deserialized;
                    _memoryCache.Set(key, value, TimeSpan.FromMinutes(1));
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    public T GetOrCreate<T>(string key, Func<ICacheEntry, T> factory) where T : class?
    {
        return _memoryCache.GetOrCreate(key, entry =>
        {
            var result = factory(entry);

            if (_distributedCache != null && result != null)
            {
                var bytes = JsonSerializer.SerializeToUtf8Bytes(result);
                var options = new DistributedCacheEntryOptions();

                if (entry.AbsoluteExpirationRelativeToNow.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = entry.AbsoluteExpirationRelativeToNow.Value;
                }
                else if (entry.SlidingExpiration.HasValue)
                {
                    options.SlidingExpiration = entry.SlidingExpiration.Value;
                }
                else
                {
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }

                _distributedCache.Set(key, bytes, options);
            }

            return result;
        })!;
    }

    public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        var memOptions = new MemoryCacheEntryOptions();
        if (absoluteExpiration.HasValue)
        {
            memOptions.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
        }
        if (slidingExpiration.HasValue)
        {
            memOptions.SlidingExpiration = slidingExpiration.Value;
        }

        _memoryCache.Set(key, value, memOptions);

        if (_distributedCache != null)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            var distOptions = new DistributedCacheEntryOptions();

            if (absoluteExpiration.HasValue)
            {
                distOptions.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
            }
            else if (slidingExpiration.HasValue)
            {
                distOptions.SlidingExpiration = slidingExpiration.Value;
            }
            else
            {
                distOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            }

            _distributedCache.Set(key, bytes, distOptions);
        }
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _distributedCache?.Remove(key);
    }
}
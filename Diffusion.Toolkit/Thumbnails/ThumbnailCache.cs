using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit.Thumbnails;

public class CacheEntry
{
    public DateTime Created { get; set; }
    public BitmapSource BitmapSource { get; set; }
}

public class ThumbnailCache
{
    private readonly int _maxItems;
    private readonly int _evictItems;
    private ConcurrentDictionary<string, CacheEntry> _cache = new ConcurrentDictionary<string, CacheEntry>();

    private object _lock = new object();
    private static ThumbnailCache _instance;

    public static ThumbnailCache Instance => _instance;

    private ThumbnailCache(int maxItems, int evictItems)
    {
        _maxItems = maxItems;
        _evictItems = evictItems;
    }

    public bool TryGetThumbnail(string path, out BitmapSource? thumbnail)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(path, out var cacheEntry))
            {
                thumbnail = cacheEntry.BitmapSource;
                return true;
            }

            thumbnail = null;

            return false;
        }
    }

    public void AddThumbnail(string path, BitmapSource? thumbnail)
    {
        lock (_lock)
        {
            if (_cache.Count + 1 > _maxItems)
            {
                var evictions = _cache.OrderByDescending(c => c.Value.Created).Take(_evictItems);

                foreach (var eviction in evictions)
                {
                    _cache.TryRemove(eviction);
                }
            }
        }


        if (!_cache.TryAdd(path, new CacheEntry()
        {
            BitmapSource = thumbnail,
            Created = DateTime.Now
        }))
        {

        }
    }

    //public static void CreateInstance()
    //{
    //    _instance = new ThumbnailCache(500, 100);
    //}


    public static void CreateInstance(int maxItems, int evictItems)
    {
        _instance = new ThumbnailCache(maxItems, evictItems);
    }
}
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit.Thumbnails;

public class ThumbnailCache
{
    private ConcurrentDictionary<string, BitmapSource> _cache = new ConcurrentDictionary<string, BitmapSource>();

    private HashSet<string> _queued = new HashSet<string>();

    private object _qlock = new object();

    private static ThumbnailCache _instance;

    public void Dequeue(string path)
    {
        _queued.Remove(path);
    }

    public bool TryQueue(string path)
    {
        lock (_qlock)
        {
            if (!_queued.Contains(path))
            {
                _queued.Add(path);
                return false;
            }

            return true;
        }
    }

    public static ThumbnailCache Instance => _instance;

    public bool TryGetThumbnail(string path, out BitmapSource? thumbnail)
    {
        return _cache.TryGetValue(path, out thumbnail);
    }

    public void AddThumbnail(string path, BitmapSource? thumbnail)
    {
        if (!_cache.TryAdd(path, thumbnail))
        {
            var x = 1;
        }
    }


    public static void CreateInstance()
    {
        _instance = new ThumbnailCache();
    }
}
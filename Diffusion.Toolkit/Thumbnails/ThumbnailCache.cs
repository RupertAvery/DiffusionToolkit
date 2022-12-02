using System.Collections.Concurrent;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit.Thumbnails;

public class ThumbnailCache
{
    private ConcurrentDictionary<string, BitmapSource> _cache = new ConcurrentDictionary<string, BitmapSource>();

    private static ThumbnailCache _instance;

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
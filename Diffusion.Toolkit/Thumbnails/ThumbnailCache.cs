using Diffusion.Common;
using SQLite;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit.Thumbnails;

public class CacheEntry
{
    public DateTime Created { get; set; }
    public BitmapSource BitmapSource { get; set; }
}

public class ThumbnailCacheOld
{
    private readonly int _maxItems;
    private readonly int _evictItems;
    private ConcurrentDictionary<string, CacheEntry> _cache = new ConcurrentDictionary<string, CacheEntry>();

    private object _lock = new object();
    private static ThumbnailCacheOld _instance;

    public static ThumbnailCacheOld Instance => _instance;

    private ThumbnailCacheOld(int maxItems, int evictItems)
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
        _instance = new ThumbnailCacheOld(maxItems, evictItems);
    }

    public void Clear()
    {
        _cache.Clear();
    }
}

public class Thumbnail
{
    public string Filename { get; set; }
    public byte[] Data { get; set; }
    public int Size { get; set; }
}


public class ConnectionCacheEntry
{
    private readonly string _path;
    private readonly ConcurrentDictionary<string, ConnectionCacheEntry> _connectionPool;
    private readonly SQLiteConnection _connection;
    private Action _timeout;

    public ConnectionCacheEntry(string path, ConcurrentDictionary<string, ConnectionCacheEntry> connectionPool, SQLiteConnection connection)
    {
        _path = path;
        _connectionPool = connectionPool;
        _connection = connection;
        _timeout = Utility.Debounce(() =>
        {
            if (_connectionPool.TryRemove(_path, out var cacheEntry))
            {
                Connection.Close();
            }
        }, 2000);
    }

    public SQLiteConnection Connection => _connection;


    public void ResetTimeout()
    {
        _timeout();
    }

}

public class ThumbnailCache
{
    private static ThumbnailCache _instance;

    public static ThumbnailCache Instance => _instance;

    private readonly ConcurrentDictionary<string, ConnectionCacheEntry> _connectionPool;

    private ThumbnailCache()
    {
        _connectionPool = new ConcurrentDictionary<string, ConnectionCacheEntry>();
    }

    private object _lock = new object();

    public SQLiteConnection OpenConnection(string path)
    {
        var dbPath = Path.GetDirectoryName(path);

        lock (_lock)
        {
            if (!_connectionPool.TryGetValue(dbPath, out var cacheItem))
            {
                var db = new SQLiteConnection(Path.Combine(dbPath, "dt_thumbnails.db"));
                db.CreateTable<Thumbnail>();
                db.CreateIndex<Thumbnail>(t => t.Filename, true);

                cacheItem = new ConnectionCacheEntry(dbPath, _connectionPool, db);

                _connectionPool[dbPath] = cacheItem;

                cacheItem.ResetTimeout();
            }
            else
            {
                cacheItem.ResetTimeout();
            }

            return cacheItem.Connection;
        }
       
    }


    public bool Unload(string path)
    {
        if (_connectionPool.TryRemove(path, out var cacheEntry))
        {
            cacheEntry.Connection.Close();
            return true;
        }

        return false;
    }

    public bool TryGetThumbnail(string path, int size, out BitmapSource? thumbnail)
    {
        var db = OpenConnection(path);

        var filename = Path.GetFileName(path);

        var data = db.Query<Thumbnail>("SELECT Filename, Data FROM Thumbnail WHERE Filename = ? AND Size = ?", filename, size);

        var result = false;

        if (data.Count > 0)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(data[0].Data);
            result = true;
            bitmap.EndInit();
            bitmap.Freeze();
            thumbnail = bitmap;
        }
        else
        {
            thumbnail = null;
        }

        return result;
    }

    public void AddThumbnail(string path, int size, BitmapImage bitmapImage)
    {
        if (File.Exists(path))
        {
            var db = OpenConnection(path);

            var filename = Path.GetFileName(path);
            var data = ((MemoryStream)bitmapImage.StreamSource).ToArray();

            var command = db.CreateCommand("REPLACE INTO Thumbnail (Filename, Data, Size) VALUES (@Filename, @Data, @Size)");
            command.Bind("@Filename", filename);
            command.Bind("@Data", data);
            command.Bind("@Size", size);
            command.ExecuteNonQuery();
        }
    }


    public static void CreateInstance(int maxItems, int evictItems)
    {
        _instance = new ThumbnailCache();
    }

    public void Clear()
    {
    }
}
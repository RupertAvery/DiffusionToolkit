using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;

namespace Diffusion.Toolkit.Thumbnails;

public class ThumbailResult
{
    public bool Success { get; }
    public BitmapSource? Image { get; }

    public ThumbailResult(BitmapSource image)
    {
        Image = image;
        Success = true;
    }

    private ThumbailResult(bool success)
    {
        Success = success;
    }

    public static ThumbailResult Failed => new ThumbailResult(false);

}

public class ThumbnailLoader
{
    private static ThumbnailLoader? _instance;
    private readonly Channel<Job<ThumbnailJob, ThumbailResult>> _channel = Channel.CreateUnbounded<Job<ThumbnailJob, ThumbailResult>>();
    private readonly int _degreeOfParallelism = 2;

    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private Stream _defaultStream;

    private ThumbnailLoader()
    {

        _defaultStream = new MemoryStream();

        _enableCache = true;

        StreamResourceInfo sri = Application.GetResourceStream(new Uri("Images/thumbnail.png", UriKind.Relative));
        if (sri != null)
        {
            using (Stream s = sri.Stream)
            {
                sri.Stream.CopyTo(_defaultStream);
                _defaultStream.Position = 0;
            }
        }
    }

    public int Size
    {
        get => _size;
        set
        {
            _size = value;
            ThumbnailCache.Instance.Clear();
        }
    }

    public bool EnableCache
    {
        get => _enableCache;
        set
        {
            _enableCache = value;
            if (!value)
            {
                ThumbnailCache.Instance.Clear();
            }
        }
    }

    public static void CreateInstance()
    {
        _instance = new ThumbnailLoader();
    }

    public static ThumbnailLoader Instance => _instance;

    public void Stop()
    {
        cancellationTokenSource.Cancel();
        _channel.Writer.Complete();
    }

    public Task Start()
    {
        var consumers = new List<Task>();

        for (var i = 0; i < _degreeOfParallelism; i++)
        {
            consumers.Add(ProcessTaskAsync(cancellationTokenSource.Token));
        }

        //_channel.Writer.Complete();

        return Task.WhenAll(consumers);
    }

    public Task StartRun()
    {
        var consumers = new List<Task>();

        for (var i = 0; i < _degreeOfParallelism; i++)
        {
            consumers.Add(Task.Run(async () => await ProcessTaskAsync(cancellationTokenSource.Token)));
        }

        //_channel.Writer.Complete();

        return Task.WhenAll(consumers);
    }

    private long _requestId;
    private int _size = 128;
    private bool _enableCache;

    public void SetCurrentRequestId(long requestId)
    {
        _requestId = requestId;
    }

    private async Task ProcessTaskAsync(CancellationToken token)
    {

        while (await _channel.Reader.WaitToReadAsync(token))
        {
            try
            {
                var job = await _channel.Reader.ReadAsync(token);

                if (job.Data.RequestId != _requestId)
                {
                    continue;
                }

                if (_enableCache)
                {
                    if (!ThumbnailCache.Instance.TryGetThumbnail(job.Data.Path,
                            out BitmapSource? thumbnail))
                    {
                        // Debug.WriteLine($"Loading from disk");
                        if (job.Data.EntryType == EntryType.File)
                        {
                            if (!File.Exists(job.Data.Path))
                            {
                                job.Completion(ThumbailResult.Failed);
                                continue;
                            }

                            thumbnail = GetThumbnailImmediate(job.Data.Path, job.Data.Width, job.Data.Height, Size);
                        }
                        else
                        {
                            thumbnail = GetDefaultThumbnailImmediate();
                        }

                        ThumbnailCache.Instance.AddThumbnail(job.Data.Path, thumbnail);

                        job.Completion(new ThumbailResult(thumbnail));

                    }
                    else
                    {
                        job.Completion(new ThumbailResult(thumbnail));
                    }
                }
                else
                {
                    BitmapImage thumbnail;

                    // Debug.WriteLine($"Loading from disk");
                    if (job.Data.EntryType == EntryType.File)
                    {
                        if (!File.Exists(job.Data.Path))
                        {
                            job.Completion(ThumbailResult.Failed);
                            continue;
                        }
                        thumbnail = GetThumbnailImmediate(job.Data.Path, job.Data.Width, job.Data.Height, Size);
                    }
                    else
                    {
                        thumbnail = GetDefaultThumbnailImmediate();
                    }

                    ThumbnailCache.Instance.AddThumbnail(job.Data.Path, thumbnail);

                    job.Completion(new ThumbailResult(thumbnail));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


        }
    }

    public async Task QueueAsync(ThumbnailJob job, Action<ThumbailResult> completion)
    {
        await _channel.Writer.WriteAsync(new Job<ThumbnailJob, ThumbailResult>() { Data = job, Completion = completion });
    }

    private static Stream GenerateThumbnail(string path, int width, int height, int size)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var bitmap = new BitmapImage();
        //bitmap.UriSource = new Uri(path);
        bitmap.BeginInit();

        if (width > height)
        {
            bitmap.DecodePixelWidth = size;
        }
        else
        {
            bitmap.DecodePixelHeight = size;
        }
        //bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();

        var encoder = new JpegBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap, null, null, null));

        var ministream = new MemoryStream();

        encoder.Save(ministream);

        ministream.Seek(0, SeekOrigin.Begin);

        return ministream;
    }

    public BitmapImage GetThumbnailDirect(string path, int width, int height)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        if (File.Exists(path))
        {
            bitmap.StreamSource = GenerateThumbnail(path, width, height, Size);
        }
        else
        {
            bitmap.StreamSource = null;
        }
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    public BitmapImage GetThumbnailImmediate(string path, int width, int height, int size)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        if (File.Exists(path))
        {
            bitmap.StreamSource = GenerateThumbnail(path, width, height, size);
        }
        else
        {
            bitmap.StreamSource = null;
        }
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private BitmapImage GetDefaultThumbnailImmediate()
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = _defaultStream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }



    private static BitmapImage GetThumbnail(string path, int width, int height)
    {
        BitmapImage bitmap = null;
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        bitmap = new BitmapImage();
        //bitmap.UriSource = new Uri(path);
        bitmap.BeginInit();

        var size = 128;
        bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
        if (width > height)
        {
            bitmap.DecodePixelWidth = size;
        }
        else
        {
            bitmap.DecodePixelHeight = size;
        }
        //bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

}
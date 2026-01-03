using Diffusion.Toolkit.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Resources;
using System.Windows.Threading;
using Diffusion.Toolkit.Models;

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

public class ThumbnailService
{
    private static ThumbnailService? _instance;
    private readonly Channel<Job<ThumbnailJob, ThumbailResult>> _channel = Channel.CreateUnbounded<Job<ThumbnailJob, ThumbailResult>>();
    private readonly int _degreeOfParallelism = 2;

    private CancellationTokenSource cancellationTokenSource;

    private Stream _defaultStream;

    public ThumbnailService()
    {

        _defaultStream = new MemoryStream();

        _enableCache = true;

        //StreamResourceInfo sri = Application.GetResourceStream(new Uri("Images/thumbnail.png", UriKind.Relative));
        //if (sri != null)
        //{
        //    using (Stream s = sri.Stream)
        //    {
        //        sri.Stream.CopyTo(_defaultStream);
        //        _defaultStream.Position = 0;
        //    }
        //}
    }

    private Dispatcher _dispatcher => ServiceLocator.Dispatcher;


    public void QueueImage(ImageEntry image)
    {
        image.LoadState = LoadState.Loading;

        var job = new ThumbnailJob()
        {
            BatchId = image.BatchId,
            EntryType = image.EntryType,
            Path = image.Path,
            Height = image.Height,
            Width = image.Width
        };

        _ = QueueAsync(job, (d) =>
        {
            image.LoadState = LoadState.Loaded;

            if (d.Success)
            {
                _dispatcher.Invoke(() =>
                {
                    image.Thumbnail = d.Image;
                    image.ThumbnailHeight = d.Image.Height;
                    image.ThumbnailWidth = d.Image.Width;
                });
            }
            else
            {
                _dispatcher.Invoke(() => { image.Unavailable = true; });
            }

            //Debug.WriteLine($"Finished job {job.RequestId}");
            //OnPropertyChanged(nameof(Thumbnail));
        });
    }


    public int Size
    {
        get;
        set
        {
            field = value;
            ThumbnailCache.Instance.Clear();
        }
    } = 128;

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

    public void Stop()
    {
        cancellationTokenSource.Cancel();
        _channel.Writer.Complete();
    }

    public Task Start()
    {
        cancellationTokenSource = new CancellationTokenSource();

        var consumers = new List<Task>();

        for (var i = 0; i < _degreeOfParallelism; i++)
        {
            consumers.Add(ProcessTaskAsync(cancellationTokenSource.Token));
        }

        return Task.WhenAll(consumers);
    }

    public Task StartAsync()
    {
        cancellationTokenSource = new CancellationTokenSource();

        var consumers = new List<Task>();

        for (var i = 0; i < _degreeOfParallelism; i++)
        {
            consumers.Add(Task.Run(async () => await ProcessTaskAsync(cancellationTokenSource.Token)));
        }

        return Task.WhenAll(consumers);
    }

    private bool _enableCache;


    private Random r = new Random();

    private long _currentBatchId;

    public long StartBatch()
    {
        return _currentBatchId = r.NextInt64(long.MaxValue);
    }


    public void StopCurrentBatch()
    {

    }


    private async Task ProcessTaskAsync(CancellationToken token)
    {

        while (await _channel.Reader.WaitToReadAsync(token))
        {
            try
            {
                var job = await _channel.Reader.ReadAsync(token);

                // Exit early if the batch has changed
                if (job.Data.BatchId != _currentBatchId)
                {
                    continue;
                }

                if (_enableCache)
                {
                    if (!ThumbnailCache.Instance.TryGetThumbnail(job.Data.Path, Size,
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

                        ThumbnailCache.Instance.AddThumbnail(job.Data.Path, Size, (BitmapImage)thumbnail);

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

                    ThumbnailCache.Instance.AddThumbnail(job.Data.Path, Size, thumbnail);

                    job.Completion(new ThumbailResult(thumbnail));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


        }
        Debug.WriteLine("Shutting down Thumbnail Task");
    }

    public async Task QueueAsync(ThumbnailJob job, Action<ThumbailResult> completion)
    {
        if (!cancellationTokenSource.IsCancellationRequested)
        {
            await _channel.Writer.WriteAsync(new Job<ThumbnailJob, ThumbailResult>() { Data = job, Completion = completion });
        }
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

    //public (BitmapImage, Stream) GetThumbnailImmediateStream(string path, int width, int height, int size)
    //{
    //    var bitmap = new BitmapImage();
    //    bitmap.BeginInit();

    //    Stream stream = null;

    //    if (File.Exists(path))
    //    {
    //        stream = GenerateThumbnail(path, width, height, size);
    //    }

    //    bitmap.StreamSource = stream;

    //    bitmap.EndInit();
    //    bitmap.Freeze();
    //    return (bitmap;
    //}

    public (int, int) GetBitmapSize(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            BitmapDecoder decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.DelayCreation,
                BitmapCacheOption.None);

            int width = decoder.Frames[0].PixelWidth;
            int height = decoder.Frames[0].PixelHeight;

            return (width, height);
        }
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
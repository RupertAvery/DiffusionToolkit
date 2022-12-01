using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Diffusion.Toolkit.Thumbnails;

public class ThumbnailLoader
{
    private readonly Dispatcher _dispatcher;
    private static ThumbnailLoader? _instance;
    private readonly Channel<Job<ThumbnailJob, BitmapSource>> _channel = Channel.CreateUnbounded<Job<ThumbnailJob, BitmapSource>>();
    private readonly int _degreeOfParallelism = 2;

    private ThumbnailLoader(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public static void CreateInstance(Dispatcher dispatcher)
    {
        _instance = new ThumbnailLoader(dispatcher);
    }

    public static ThumbnailLoader? Instance => _instance;

    public void Stop()
    {
        _channel.Writer.Complete();
    }

    public Task Start(CancellationToken token)
    {
        var consumers = new List<Task>();

        for (var i = 0; i < _degreeOfParallelism; i++)
        {
            consumers.Add(ProcessTaskAsync(token));
        }

        //_channel.Writer.Complete();

        return Task.WhenAll(consumers);
    }

    public Task StartRun(CancellationToken token)
    {
        var consumers = new List<Task>();

        for (var i = 0; i < _degreeOfParallelism; i++)
        {
            consumers.Add(Task.Run(async () => await ProcessTaskAsync(token), token));
        }

        //_channel.Writer.Complete();

        return Task.WhenAll(consumers);
    }

    //public void StartThreaded(CancellationToken token)
    //{
    //    var consumers = new List<Thread>();

    //    for (var i = 0; i < _degreeOfParallelism; i++)
    //    {
    //        consumers.Add(new Thread(o => ProcessTaskAsync(token));
    //    }

    //    //_channel.Writer.Complete();

    //    return Task.WhenAll(consumers);
    //}


    private async Task ProcessTaskAsync(CancellationToken token)
    {

        while (await _channel.Reader.WaitToReadAsync(token))
        {
            var job = await _channel.Reader.ReadAsync(token);


            if (!ThumbnailCache.Instance.TryGetThumbnail(job.Data.Path,
                    out BitmapSource? thumbnail))
            {
                if (!ThumbnailCache.Instance.TryQueue(job.Data.Path))
                {
                    var s = new AutoResetEvent(false);

                    var t = new Thread((o) =>
                    {
                        thumbnail = GetThumbnail(job.Data.Path, job.Data.Width, job.Data.Height);
                        ThumbnailCache.Instance.AddThumbnail(job.Data.Path, thumbnail);

                        _dispatcher.Invoke(() =>
                        {
                            job.Completion(thumbnail);
                        }, token);

                        s.Set();
                    });


                    t.Start();

                    s.WaitOne();

                    ThumbnailCache.Instance.Dequeue(job.Data.Path);
                }


                //_ = Task.Run(() =>
                //{
                //    thumbnail = GetThumbnail(job.Data.Path, job.Data.Width, job.Data.Height);
                //    ThumbnailCache.Instance.AddThumbnail(job.Data.Path, thumbnail);

                //    _dispatcher.Invoke(() =>
                //    {
                //        job.Completion(thumbnail);
                //    });

                //}, token);
            }
            else
            {
                _dispatcher.Invoke(() =>
                {
                    job.Completion(thumbnail);
                }, token);
            }


        }
    }

    public async Task QueueAsync(ThumbnailJob job, Action<BitmapSource> completion)
    {
        await _channel.Writer.WriteAsync(new Job<ThumbnailJob, BitmapSource>() { Data = job, Completion = completion });
    }

    public static BitmapImage GetThumbnail(string path, int width, int height)
    {
        BitmapImage bitmap = null;
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
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
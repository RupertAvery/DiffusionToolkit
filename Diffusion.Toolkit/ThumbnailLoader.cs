using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Diffusion.IO;

namespace Diffusion.Toolkit;

public class ThumbnailLoader
{
    private readonly Dispatcher _dispatcher;
    private static ThumbnailLoader _instance;
    private readonly Channel<Job<FileParameters, BitmapSource>> _channel = Channel.CreateUnbounded<Job<FileParameters, BitmapSource>>();
    private int _degreeOfParallelism = 2;

    public ThumbnailLoader(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public static void CreateInstance(Dispatcher dispatcher)
    {
        _instance = new ThumbnailLoader(dispatcher);
    }

    public static ThumbnailLoader Instance
    {
        get
        {
            return _instance;
        }
    }


    public void Stop()
    {
        _channel.Writer.Complete();
    }

    public Task Start(CancellationToken token)
    {
        var consumers = new List<Task>();

        for (var i = 0; i < _degreeOfParallelism; i++)
        {
            consumers.Add(ProcessTask(token));
        }

        //_channel.Writer.Complete();

        return Task.WhenAll(consumers);
    }


    private async Task ProcessTask(CancellationToken token)
    {

        while (await _channel.Reader.WaitToReadAsync(token))
        {
            var job = await _channel.Reader.ReadAsync(token);
            await Task.Run(async () =>
            {
                var thumbnail = GetThumbnail(job.Data.Path, job.Data.Width, job.Data.Height);
                _dispatcher.Invoke(() =>
                {
                    job.Completion(thumbnail);
                });
                await Task.Delay(10);
            }, token);
        }
    }

    public async Task Queue(FileParameters file, Action<BitmapSource> completion)
    {
        await _channel.Writer.WriteAsync(new Job<FileParameters, BitmapSource>() { Data = file, Completion = completion });
    }

    public static BitmapImage GetThumbnail(string path, int width, int height)
    {
        BitmapImage bitmap = null;
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        bitmap = new BitmapImage();
        //bitmap.UriSource = new Uri(path);
        bitmap.BeginInit();
        bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
        if (width > height)
        {
            bitmap.DecodePixelWidth = 128;
        }
        else
        {
            bitmap.DecodePixelHeight = 128;
        }
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

}
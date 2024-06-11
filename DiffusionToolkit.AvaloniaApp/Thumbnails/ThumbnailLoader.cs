using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Services;

namespace DiffusionToolkit.AvaloniaApp.Thumbnails
{
    public class ThumbnailLoader
    {
        private readonly Channel<Job<ThumbnailJob, ThumbailResult>> _channel = Channel.CreateUnbounded<Job<ThumbnailJob, ThumbailResult>>();
        private readonly int _degreeOfParallelism = 4;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _channel.Writer.Complete();
        }

        public Task Start()
        {
            var consumers = new List<Task>();

            for (var i = 0; i < _degreeOfParallelism; i++)
            {
                consumers.Add(ProcessTaskAsync(_cancellationTokenSource.Token));
            }

            //_channel.Writer.Complete();

            return Task.WhenAll(consumers);
        }

        public Task StartRun()
        {
            var consumers = new List<Task>();

            for (var i = 0; i < _degreeOfParallelism; i++)
            {
                consumers.Add(Task.Factory.StartNew(async () => await ProcessTaskAsync(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning));
                //consumers.Add(Task.Run(async () => await ProcessTaskAsync(_cancellationTokenSource.Token)));
            }

            //_channel.Writer.Complete();

            return Task.WhenAll(consumers);
        }


        private async Task ProcessTaskAsync(CancellationToken token)
        {

            while (await _channel.Reader.WaitToReadAsync(token))
            {
                var job = await _channel.Reader.ReadAsync(token);

                try
                {
                    //var bitmap = LoadThumbnail(job.Data);

                    //job.Completion(ThumbailResult.FromBitmap(bitmap));

                    var cachedEntry = ServiceLocator.ThumbnailCache.GetThumbnail(job.Data.Id);

                    if (cachedEntry == null || cachedEntry.Size != job.Data.Size)
                    {
                        var (bitmap, data) = LoadThumbnail(job.Data);

                        if (cachedEntry == null)
                        {
                            ServiceLocator.ThumbnailCache.AddThumbnail(new ThumbnailEntry()
                            {
                                Id = job.Data.Id,
                                Height = (int)bitmap.Size.Height,
                                Width = (int)bitmap.Size.Width,
                                Path = job.Data.Path,
                                Size = job.Data.Size,
                                Data = data,
                            });
                        }
                        else
                        {
                            ServiceLocator.ThumbnailCache.UpdateThumbnail(new ThumbnailEntry()
                            {
                                Id = job.Data.Id,
                                Height = (int)bitmap.Size.Height,
                                Width = (int)bitmap.Size.Width,
                                Path = job.Data.Path,
                                Size = job.Data.Size,
                                Data = data,
                            });
                        }


                        job.Completion(ThumbailResult.FromBitmap(bitmap));
                    }
                    else
                    {
                        using var stream = new MemoryStream(cachedEntry.Data);
                        var bitmap = Bitmap.DecodeToWidth(stream, job.Data.Size);

                        job.Completion(ThumbailResult.FromBitmap(bitmap));
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    job.Completion(ThumbailResult.Failed);
                }
            }
        }

        public async Task QueueAsync(ThumbnailJob job, Action<ThumbailResult> completion)
        {
            await _channel.Writer.WriteAsync(new Job<ThumbnailJob, ThumbailResult>() { Data = job, Completion = completion });
        }

        private (Bitmap, byte[]) LoadThumbnail(ThumbnailJob job)
        {
            using var stream = File.Open(job.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            var bitmap = Bitmap.DecodeToWidth(stream, job.Size);

            using var data = new MemoryStream();

            bitmap.Save(data);

            data.Seek(0, SeekOrigin.Begin);


            return (bitmap, data.ToArray());
        }

    }
}
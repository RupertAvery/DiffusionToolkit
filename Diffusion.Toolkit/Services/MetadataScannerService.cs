using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Input;
using Diffusion.Common;
using Diffusion.IO;
using Diffusion.Toolkit.Configuration;

namespace Diffusion.Toolkit.Services;

public class MetadataScannerService
{
    private Channel<FileScanJob> _channel;
    private Channel<FileScanJob> _queueChannel = Channel.CreateUnbounded<FileScanJob>();

    private CancellationTokenSource _cancellationTokenSource;

    private readonly int _degreeOfParallelism = 2;

    private Settings _settings => ServiceLocator.Settings!;


    public async Task QueueBatchAsync(IEnumerable<string> paths, CancellationToken cancellationToken)
    {
        var dt = ServiceLocator.DatabaseWriterService.StartAsync(cancellationToken);

        if (!dt.IsStarted)
        {
            dt.Task.ContinueWith(d =>
            {
                ServiceLocator.ProgressService.CompleteTask();
                ServiceLocator.ProgressService.ClearProgress();
                ServiceLocator.ProgressService.SetStatus("");
            });
        }

        var mt = StartAsync(cancellationToken);


        if (!mt.IsStarted)
        {
            mt.Task.ContinueWith(d =>
            {
                ServiceLocator.DatabaseWriterService.Complete();
            });
        }

        var i = 0;

        foreach (var path in paths)
        {
            await _channel.Writer.WriteAsync(new FileScanJob() { Path = path });

            i++;
            if (i % 33 == 0)
            {
                ServiceLocator.ProgressService.AddTotal(i);
                i = 0;
            }
        }

        ServiceLocator.ProgressService.AddTotal(i);

        _channel.Writer.Complete();
    }

    private bool _queueRunning;

    public void StartQueue(CancellationToken cancellationToken)
    {
        if (!_queueRunning)
        {
            Task.Run(async () => await ProcessQueueTaskAsync(cancellationToken));
            _queueRunning = true;
        }
    }

    public async Task QueueAsync(string path, CancellationToken cancellationToken)
    {
        ServiceLocator.DatabaseWriterService.StartQueueAsync(cancellationToken);
        StartQueue(cancellationToken);

        await _queueChannel.Writer.WriteAsync(new FileScanJob() { Path = path });
    }

    private bool _isStarted;
    private bool _isCompleted;
    private Task<int> _currentTask;

    public StartResult<int> StartAsync(CancellationToken token)
    {
        if (_isStarted && !_isCompleted)
        {
            return new StartResult<int>() { IsStarted = true, Task = _currentTask };
        }

        _isStarted = true;
        _isCompleted = false;

        ServiceLocator.ProgressService.ResetTotal();

        _cancellationTokenSource = new CancellationTokenSource();
        _channel = Channel.CreateUnbounded<FileScanJob>();

        CancellationTokenSource linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token);

        var consumers = new List<Task<int>>();

        for (var i = 0; i < _degreeOfParallelism; i++)
        {
            consumers.Add(Task.Run(async () => await ProcessTaskAsync(linkedCts.Token)));
        }

        _currentTask = Task.WhenAll(consumers).ContinueWith(t =>
        {
            _isCompleted = true;

            if (!t.IsCanceled)
            {
                return t.Result.Sum();
            }

            return 0;
        });

        return new StartResult<int>() { Task = _currentTask };
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _channel.Writer.Complete();
    }

    public void Close()
    {
        _channel.Writer.Complete();
    }


    private async Task<int> ProcessTaskAsync(CancellationToken token)
    {
        var count = 0;

        while (await _channel.Reader.WaitToReadAsync(token))
        {
            var job = await _channel.Reader.ReadAsync(token);

            try
            {
                if (File.Exists(job.Path))
                {
                    var fileParameters = Metadata.ReadFromFile(job.Path);

                    count++;

                    if (ServiceLocator.DataStore.ImageExists(job.Path))
                    {
                        await ServiceLocator.DatabaseWriterService.QueueUpdateAsync(fileParameters, _settings.StoreMetadata, _settings.StoreWorkflow);
                    }
                    else
                    {
                        await ServiceLocator.DatabaseWriterService.QueueAddAsync(fileParameters, _settings.StoreMetadata, _settings.StoreWorkflow);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error scanning {job.Path}:" + ex.Message);
            }
        }

        return count;
    }



    private async Task<int> ProcessQueueTaskAsync(CancellationToken token)
    {
        var count = 0;

        while (await _queueChannel.Reader.WaitToReadAsync(token))
        {
            var job = await _queueChannel.Reader.ReadAsync(token);

            try
            {
                if (File.Exists(job.Path))
                {
                    var fileParameters = Metadata.ReadFromFile(job.Path);

                    count++;

                    await ServiceLocator.DatabaseWriterService.QueueAsync(fileParameters, _settings.StoreMetadata, _settings.StoreWorkflow);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error scanning {job.Path}:" + ex.Message);
            }
        }

        return count;
    }
}
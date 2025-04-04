using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.IO;
using static SQLite.SQLite3;

namespace Diffusion.Toolkit.Services;

public class DatabaseWriteReport
{
    public int Added { get; set; }
    public int Updated { get; set; }
}

public class StartResult<T>
{
    public bool IsStarted { get; set; }
    public Task<T> Task { get; set; }
}

public class DatabaseWriterService
{
    private Channel<RecordJob> _updateChannel;
    private Channel<RecordJob> _addChannel;

    private CancellationTokenSource _cancellationTokenSource;
    private Settings _settings => ServiceLocator.Settings!;

    public async Task QueueAddAsync(FileParameters fileParameters, bool storeMetadata, bool storeWorkflow)
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            var job = new RecordJob()
            {
                FileParameters = fileParameters,
                StoreMetadata = storeMetadata,
                StoreWorkflow = storeWorkflow
            };

            await _addChannel.Writer.WriteAsync(job);
        }
    }

    public async Task QueueUpdateAsync(FileParameters fileParameters, bool storeMetadata, bool storeWorkflow)
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            var job = new RecordJob()
            {
                FileParameters = fileParameters,
                StoreMetadata = storeMetadata,
                StoreWorkflow = storeWorkflow
            };

            await _updateChannel.Writer.WriteAsync(job);
        }
    }

    private bool _isStarted;
    private bool _isCompleted = false;
    private Task<DatabaseWriteReport> _currentTask;

    public StartResult<DatabaseWriteReport> StartAsync(CancellationToken token)
    {
        if (_isStarted && !_isCompleted)
        {
            return new StartResult<DatabaseWriteReport>() { IsStarted = true, Task = _currentTask };
        }

        _isStarted = true;
        _isCompleted = false;

        _cancellationTokenSource = new CancellationTokenSource();
        _updateChannel = Channel.CreateUnbounded<RecordJob>();
        _addChannel = Channel.CreateUnbounded<RecordJob>();

        CancellationTokenSource linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token);

        var addedTask = Task.Run(async () => await ProcessAddTaskAsync(linkedCts.Token));
        var updatedTask = Task.Run(async () => await ProcessUpdateTaskAsync(linkedCts.Token));

        _currentTask = Task.WhenAll(addedTask, updatedTask).ContinueWith(t =>
        {
            _isCompleted = true;

            if (!t.IsCanceled)
            {
                return new DatabaseWriteReport()
                {
                    Added = t.Result[0],
                    Updated = t.Result[1]
                };
            }

            return new DatabaseWriteReport()
            {
                Added = 0,
                Updated = 0
            };
        });

        return new StartResult<DatabaseWriteReport>
        {
            Task = _currentTask
        };
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _addChannel.Writer.Complete();
        _updateChannel.Writer.Complete();
    }

    public void Complete()
    {
        _addChannel.Writer.Complete();
        _updateChannel.Writer.Complete();
    }

    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    private void UpdateStatus()
    {
        var count = _addChannel.Reader.Count + _updateChannel.Reader.Count;
        ServiceLocator.ProgressService.SetStatus($"Scanning: {ServiceLocator.MainModel.CurrentProgress} of {ServiceLocator.MainModel.TotalProgress}");
    }

    private async Task<int> ProcessAddTaskAsync(CancellationToken token)
    {
        var folderIdCache = new Dictionary<string, int>();

        var newImages = new List<Image>();
        var newNodes = new List<IO.Node>();

        var includeProperties = new List<string>();

        if (_settings.AutoTagNSFW)
        {
            includeProperties.Add(nameof(Image.NSFW));
        }

        if (_settings.StoreMetadata)
        {
            includeProperties.Add(nameof(Image.Workflow));
        }

        int added = 0;

        while (await _addChannel.Reader.WaitToReadAsync(token))
        {
            var job = await _addChannel.Reader.ReadAsync(token);

            var fp = job.FileParameters;

            try
            {
                try
                {
                    var (image, nodes) = ServiceLocator.ScanningService.ProcessFile(fp, _settings.StoreMetadata, _settings.StoreWorkflow);

                    newImages.Add(image);
                    newNodes.AddRange(nodes);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }

                if (newImages.Count == 33 || _addChannel.Reader.Count == 0)
                {
                    await _lock.WaitAsync(token);

                    try
                    {
                        added += ServiceLocator.ScanningService.AddImages(newImages, newNodes, includeProperties, folderIdCache, _settings.StoreWorkflow, token);
                        ServiceLocator.ProgressService.AddProgress(newImages.Count);
                        UpdateStatus();
                    }
                    finally
                    {
                        _lock.Release();
                    }

                    newNodes.Clear();
                    newImages.Clear();
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        return added;
    }

    private async Task<int> ProcessUpdateTaskAsync(CancellationToken token)
    {
        var folderIdCache = new Dictionary<string, int>();

        var newImages = new List<Image>();
        var newNodes = new List<IO.Node>();

        var includeProperties = new List<string>();

        if (_settings.AutoTagNSFW)
        {
            includeProperties.Add(nameof(Image.NSFW));
        }

        if (_settings.StoreMetadata)
        {
            includeProperties.Add(nameof(Image.Workflow));
        }

        int updated = 0;

        while (await _updateChannel.Reader.WaitToReadAsync(token))
        {
            var job = await _updateChannel.Reader.ReadAsync(token);

            var fp = job.FileParameters;

            try
            {
                try
                {
                    var (image, nodes) = ServiceLocator.ScanningService.ProcessFile(fp, _settings.StoreMetadata, _settings.StoreWorkflow);

                    newImages.Add(image);
                    newNodes.AddRange(nodes);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }


                if (newImages.Count == 33 || _updateChannel.Reader.Count == 0)
                {
                    await _lock.WaitAsync(token);

                    try
                    {
                        updated += ServiceLocator.ScanningService.UpdateImages(newImages, newNodes, includeProperties, folderIdCache, _settings.StoreWorkflow, token);
                        ServiceLocator.ProgressService.AddProgress(newImages.Count);
                        UpdateStatus();
                    }
                    finally
                    {
                        _lock.Release();
                    }

                    newNodes.Clear();
                    newImages.Clear();
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        return updated;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Diffusion.Common;
using Diffusion.Database.Models;
using Diffusion.IO;
using Diffusion.Toolkit.Configuration;

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
    private Channel<RecordJob> _queueChannel = Channel.CreateUnbounded<RecordJob>();
    private Channel<RecordJob> _updateChannel;
    private Channel<RecordJob> _addChannel;
    private Channel<RecordJob> _moveChannel = Channel.CreateUnbounded<RecordJob>();

    private CancellationTokenSource _cancellationTokenSource;
    private Settings _settings => ServiceLocator.Settings!;
    private Action<int> _debounceQueueNotification;
    private Action<int> _debounceQueueMovedNotification;
    private int _queueTotal = 0;
    private int _queueMovedTotal = 0;

    public DatabaseWriterService()
    {
        _debounceQueueNotification = Utility.Debounce<int>((a) =>
        {
            var diff = a - _queueTotal;
            ServiceLocator.ToastService.Toast($"Added {diff} new images", "");
            _queueTotal = a;
        }, 2000);

        _debounceQueueMovedNotification = Utility.Debounce<int>((a) =>
        {
            var diff = a - _queueMovedTotal;
            ServiceLocator.ToastService.Toast($"Moved {diff} images", "");
            _queueMovedTotal = a;
        }, 2000);
    }

    public async Task QueueAsync(FileParameters fileParameters, bool storeMetadata, bool storeWorkflow)
    {
        var job = new RecordJob()
        {
            IsMoved = true,
            FileParameters = fileParameters,
            StoreMetadata = storeMetadata,
            StoreWorkflow = storeWorkflow
        };

        await _queueChannel.Writer.WriteAsync(job);
    }

    public async Task QueueMoveAsync(FileParameters fileParameters,bool storeMetadata, bool storeWorkflow)
    {
        var job = new RecordJob()
        {
            IsMoved = true,
            FileParameters = fileParameters,
            StoreMetadata = storeMetadata,
            StoreWorkflow = storeWorkflow
        };

        await _moveChannel.Writer.WriteAsync(job);
    }

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

    private bool _queueRunning;

    public void StartQueueAsync(CancellationToken token)
    {
        if (!_queueRunning)
        {
            Task.Run(async () => await ProcessQueueTaskAsync(token));
            Task.Run(async () => await ProcessQueueMoveTaskAsync(token));
            _queueRunning = true;
        }
    }

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
        ServiceLocator.ProgressService.SetStatus($"Scanning: {ServiceLocator.MainModel.CurrentProgress} of {ServiceLocator.MainModel.TotalProgress}");
    }


    private async Task<int> ProcessQueueTaskAsync(CancellationToken token)
    {
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

        var folderCache = ServiceLocator.DataStore.GetFolders().ToDictionary(d => d.Path);

        int added = 0;

        while (await _queueChannel.Reader.WaitToReadAsync(token))
        {
            var job = await _queueChannel.Reader.ReadAsync(token);

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

                if (newImages.Count == 33 || _queueChannel.Reader.Count == 0)
                {
                    await _lock.WaitAsync(token);

                    try
                    {
                        added += ServiceLocator.ScanningService.AddImages(newImages, newNodes, includeProperties, folderCache, _settings.StoreWorkflow, token);
                        _debounceQueueNotification(added);
                    }
                    finally
                    {
                        _lock.Release();
                    }

                    newNodes.Clear();
                    newImages.Clear();
                }

                if (!ServiceLocator.MainModel.IsBusy)
                {
                    var path = job.FileParameters.Path;
                    if (path.Length > 100)
                    {
                        path = "...\\" + path[path.LastIndexOf("\\", 100, StringComparison.Ordinal)..];
                    }
                    else if (path.Length > 70)
                    {
                        path = "...\\" + path[path.LastIndexOf("\\", 70, StringComparison.Ordinal)..];
                    }
                    ServiceLocator.ProgressService.SetStatus($"Scanning: {path}");
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        return added;
    }


    private async Task<int> ProcessQueueMoveTaskAsync(CancellationToken token)
    {
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

        var folderCache = ServiceLocator.DataStore.GetFolders().ToDictionary(d => d.Path);

        int moved = 0;

        while (await _moveChannel.Reader.WaitToReadAsync(token))
        {
            var job = await _moveChannel.Reader.ReadAsync(token);

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

                if (newImages.Count == 33 || _moveChannel.Reader.Count == 0)
                {
                    await _lock.WaitAsync(token);

                    try
                    {
                        moved += ServiceLocator.ScanningService.UpdateImages(newImages, newNodes, includeProperties, folderCache, _settings.StoreWorkflow, token);
                        _debounceQueueMovedNotification(moved);
                    }
                    finally
                    {
                        _lock.Release();
                    }

                    newNodes.Clear();
                    newImages.Clear();
                }

                if (!ServiceLocator.MainModel.IsBusy)
                {
                    var path = job.FileParameters.Path;
                    if (path.Length > 100)
                    {
                        path = "...\\" + path[path.LastIndexOf("\\", 100, StringComparison.Ordinal)..];
                    }
                    else if (path.Length > 70)
                    {
                        path = "...\\" + path[path.LastIndexOf("\\", 70, StringComparison.Ordinal)..];
                    }
                    ServiceLocator.ProgressService.SetStatus($"Moving: {path}");
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        return moved;
    }


    private async Task<int> ProcessAddTaskAsync(CancellationToken token)
    {

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

        var folderCache = ServiceLocator.DataStore.GetFolders().ToDictionary(d => d.Path);

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
                        added += ServiceLocator.ScanningService.AddImages(newImages, newNodes, includeProperties, folderCache, _settings.StoreWorkflow, token);
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

        var folderCache = ServiceLocator.DataStore.GetFolders().ToDictionary(d => d.Path);

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
                        updated += ServiceLocator.ScanningService.UpdateImages(newImages, newNodes, includeProperties, folderCache, _settings.StoreWorkflow, token);
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
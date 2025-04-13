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


public delegate int WriteDelegate(IReadOnlyCollection<Image> images, IReadOnlyCollection<IO.Node> nodes,
    IReadOnlyCollection<string> includeProperties, Dictionary<string, Folder> folderCache, bool storeWorkflow,
    CancellationToken cancellationToken);

public class UnboundDataWriterQueue
{
    private static Settings Settings => ServiceLocator.Settings!;
    private readonly Channel<RecordJob> _queueChannel = Channel.CreateUnbounded<RecordJob>();
    private int _queueTotal = 0;
    private readonly Action<int> _debounceQueueNotification;
    private bool _queueRunning;

    public UnboundDataWriterQueue(string completionMessage)
    {
        _debounceQueueNotification = Utility.Debounce<int>((a) =>
        {
            var diff = a - _queueTotal;
            ServiceLocator.ToastService.Toast(string.Format(completionMessage, diff), "");
            _queueRunning = false;
            ServiceLocator.ProgressService.CompleteTask();
            ServiceLocator.ProgressService.ClearProgress();
            ServiceLocator.ProgressService.ClearStatus();
            _ = ServiceLocator.FolderService.LoadFolders();
            _queueTotal = a;
        }, 1000);
    }

    public async Task QueueAsync(FileParameters fileParameters, bool storeMetadata, bool storeWorkflow)
    {
        var job = new RecordJob()
        {
            FileParameters = fileParameters,
            StoreMetadata = storeMetadata,
            StoreWorkflow = storeWorkflow
        };

        await _queueChannel.Writer.WriteAsync(job);
    }

    private Task currentTask;

    public Task StartAsync(CancellationToken token)
    {
        if (!_queueRunning)
        {
            _queueTotal = 0;
            currentTask = Task.Run(async () => await ProcessQueueTask(token));
            _queueRunning = true;
        }

        return currentTask;
    }

    //public abstract int Write(IReadOnlyCollection<Image> images, IReadOnlyCollection<IO.Node> nodes,
    //    IReadOnlyCollection<string> includeProperties, Dictionary<string, Folder> folderCache, bool storeWorkflow,
    //    CancellationToken cancellationToken);

    public WriteDelegate Write { get; set; }


    private async Task ProcessQueueTask(CancellationToken token)
    {
        var newImages = new List<Image>();
        var newNodes = new List<IO.Node>();

        var includeProperties = new List<string>();

        if (Settings.AutoTagNSFW)
        {
            includeProperties.Add(nameof(Image.NSFW));
        }

        if (Settings.StoreMetadata)
        {
            includeProperties.Add(nameof(Image.Workflow));
        }

        var folderCache = ServiceLocator.DataStore.GetFolders().ToDictionary(d => d.Path);

        int completed = 0;

        while (await _queueChannel.Reader.WaitToReadAsync(token))
        {
            var job = await _queueChannel.Reader.ReadAsync(token);

            var fp = job.FileParameters;

            try
            {
                try
                {
                    var (image, nodes) = ServiceLocator.ScanningService.ProcessFile(fp, Settings.StoreMetadata, Settings.StoreWorkflow);

                    newImages.Add(image);
                    newNodes.AddRange(nodes);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }

                if (newImages.Count == 33 || _queueChannel.Reader.Count == 0)
                {
                    completed += Write(newImages, newNodes, includeProperties, folderCache, Settings.StoreWorkflow, token);
                    _debounceQueueNotification(completed);

                    newNodes.Clear();
                    newImages.Clear();
                }

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
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }
    }
}

public enum QueueType
{
    Add,
    Update,
    Move
}

public class DatabaseWriterService
{
    private Channel<RecordJob> _updateChannel;
    private Channel<RecordJob> _addChannel;

    private CancellationTokenSource _cancellationTokenSource;
    private Settings _settings => ServiceLocator.Settings!;

    private readonly UnboundDataWriterQueue _addQueue;
    private readonly UnboundDataWriterQueue _updateQueue;
    private readonly UnboundDataWriterQueue _moveQueue;

    public DatabaseWriterService()
    {
        _addQueue = new UnboundDataWriterQueue("Added {0} images")
        {
            Write = ServiceLocator.ScanningService.AddImages
        };

        _updateQueue = new UnboundDataWriterQueue("Updated {0} images")
        {
            Write = ServiceLocator.ScanningService.UpdateImages
        };

        _moveQueue = new UnboundDataWriterQueue("Moved {0} images")
        {
            Write = ServiceLocator.ScanningService.UpdateImages
        };
    }


    private bool _queueRunning;

    private Task _queueTask;

    public Task StartQueueAsync(CancellationToken token)
    {
        if (!_queueRunning)
        {
            var taskA = _addQueue.StartAsync(token);
            var taskB = _updateQueue.StartAsync(token);
            var taskC = _moveQueue.StartAsync(token);
            _queueTask = Task.WhenAll(taskA, taskB, taskC);
            _queueRunning = true;
        }
        return _queueTask;
    }

    public Task QueueAsync(FileParameters fileParameters, QueueType queueType, bool storeMetadata, bool storeWorkflow)
    {
        switch (queueType)
        {
            case QueueType.Add:
                return _addQueue.QueueAsync(fileParameters, storeMetadata, storeWorkflow);
            case QueueType.Update:
                return _updateQueue.QueueAsync(fileParameters, storeMetadata, storeWorkflow);
            case QueueType.Move:
                return _moveQueue.QueueAsync(fileParameters, storeMetadata, storeWorkflow);
        }

        throw new ArgumentOutOfRangeException(nameof(queueType));
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

            var report = new DatabaseWriteReport();

            if (addedTask.IsCompletedSuccessfully)
            {
                report.Added = addedTask.Result;
            }
            if (updatedTask.IsCompletedSuccessfully)
            {
                report.Updated = updatedTask.Result;
            }

            return report;
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

        try
        {
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
        }
        catch (TaskCanceledException e) when (token.IsCancellationRequested)
        {
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

        try
        {
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
        }
        catch (TaskCanceledException e) when (token.IsCancellationRequested)
        {
        }


        return updated;
    }
}
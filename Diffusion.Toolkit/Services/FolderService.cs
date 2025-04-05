using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Diffusion.Database;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Thumbnails;

namespace Diffusion.Toolkit.Services
{
    public class FolderService
    {
        private Settings _settings => ServiceLocator.Settings!;

        private DataStore _dataStore => ServiceLocator.DataStore!;

        private Dispatcher _dispatcher => ServiceLocator.Dispatcher;

        public FolderService()
        {

        }

        private void CleanupFolderChanges(IEnumerable<FolderChange> folderChanges)
        {
            var removed = 0;
            var updated = 0;

            foreach (var folderChange in folderChanges)
            {
                if (folderChange.ChangeType == ChangeType.Remove)
                {
                    removed += _dataStore.RemoveFolder(folderChange.Path);
                }
                else if (folderChange.ChangeType == ChangeType.ChangePath)
                {
                    updated += _dataStore.ChangeFolderPath(folderChange.Path, folderChange.NewPath);
                }
            }
        }


        public async Task LoadFolders()
        {
            var folders = _settings.ImagePaths;

            _dispatcher.Invoke(() =>
            {
                ServiceLocator.MainModel.Folders = new ObservableCollection<FolderViewModel>(folders.Select(path => new FolderViewModel()
                {
                    HasChildren = true,
                    Visible = true,
                    Depth = 0,
                    Name = path,
                    Path = path,
                    IsUnavailable = !Directory.Exists(path)
                }));
            });

            await ServiceLocator.ScanningService.CheckUnavailableFolders();

        }

        public async Task ApplyFolderChanges(IEnumerable<FolderChange> folderChanges)
        {
            var addedFolders = new List<string>();

            CleanupFolderChanges(folderChanges);

            foreach (var folderChange in folderChanges)
            {
                if (folderChange.ChangeType == ChangeType.Add)
                {
                    addedFolders.Add(folderChange.Path);
                }
            }

            RemoveWatchers();

            // If any folders changed
            if (_settings.WatchFolders)
            {
                CreateWatchers();
            }

            await LoadFolders();

            // if excluded paths changed?
            // CleanExcludedPaths(_settings.ExcludePaths);

            // possible Write contention with OnSettingsChanged

            if (addedFolders.Any())
            {
                var filesToScan = new List<string>();

                // TODO: what if there is already a task running?

                await ServiceLocator.ProgressService.StartTask();

                var cancellationToken = ServiceLocator.ProgressService.CancellationToken;

                foreach (var folder in addedFolders)
                {
                    filesToScan.AddRange(await ServiceLocator.ScanningService.GetFilesToScan(folder, new HashSet<string>(), cancellationToken));
                }
                
                await ServiceLocator.MetadataScannerService.QueueBatchAsync(filesToScan, cancellationToken);

                //await Task.Run(async () =>
                //{
                //    if (await ServiceLocator.ProgressService.TryStartTask())
                //    {
                //        try
                //        {
                //            var filesToScan = new List<string>();

                //            foreach (var folder in addedFolders)
                //            {
                //                filesToScan.AddRange(await ServiceLocator.ScanningService.GetFilesToScan(folder, new HashSet<string>(), ServiceLocator.ProgressService.CancellationToken));
                //            }

                //            await  ServiceLocator.MetadataScannerService.QueueAsync(filesToScan);

                //            //var (added, elapsed) = ServiceLocator.ScanningService.ScanFiles(filesToScan, false, _settings.StoreMetadata, _settings.StoreWorkflow, ServiceLocator.ProgressService.CancellationToken);

                //            //ServiceLocator.ScanningService.Report(added, 0, elapsed, false, false, false);
                //        }
                //        finally
                //        {
                //            ServiceLocator.ProgressService.CompleteTask();
                //        }
                //    }
                //});
            }
        }

        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();


        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                ServiceLocator.ProgressService.StartTask().ContinueWith(t =>
                {
                    _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, ServiceLocator.ProgressService.CancellationToken);
                });

            }
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            var wasTmp = Path.GetExtension(e.OldFullPath).ToLowerInvariant() == ".tmp";

            if (wasTmp && _settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                ServiceLocator.ProgressService.StartTask().ContinueWith(t =>
                {
                    _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, ServiceLocator.ProgressService.CancellationToken);
                });

            }
        }

        public void CreateWatchers()
        {
            foreach (var path in _settings.ImagePaths)
            {
                if (Directory.Exists(path))
                {
                    var watcher = new FileSystemWatcher(path)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = _settings.RecurseFolders.GetValueOrDefault(true)
                    };
                    watcher.Created += WatcherOnCreated;
                    watcher.Renamed += WatcherOnRenamed;
                    _watchers.Add(watcher);
                }
            }
        }

        public void DisableWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        public void EnableWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        private void RemoveWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
            _watchers.Clear();
        }

        private void ProcessQueueCallback(object? state)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    var currentWindow = Application.Current.Windows.OfType<Window>().First();
            //    if (currentWindow.IsActive)
            //    {
            //        ServiceLocator.ScanningService.Report(added, 0, elapsed, false, false, false);
            //    }
            //    else
            //    {
            //        lock (_lock)
            //        {
            //            addedTotal += added;
            //        }
            //    }
            //});

            //if (_settings.AutoRefresh)
            //{
            //    _search.ReloadMatches(null);
            //}
            //}
        }
    }
}

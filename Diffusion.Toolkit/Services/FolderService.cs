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

namespace Diffusion.Toolkit.Services
{
    public enum ChangeType
    {
        Add,
        Remove,
        ChangePath,
    }

    public class FolderChange
    {
        public string Path { get; set; }
        public string NewPath { get; set; }
        public ChangeType ChangeType { get; set; }
    }


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
                await Task.Run(async () =>
                {
                    if (await ServiceLocator.ProgressService.TryStartTask())
                    {
                        try
                        {
                            var filesToScan = new List<string>();

                            foreach (var folder in addedFolders)
                            {
                                filesToScan.AddRange(await ServiceLocator.ScanningService.GetFilesToScan(folder, new HashSet<string>(), ServiceLocator.ProgressService.CancellationToken));
                            }

                            var (added, elapsed) = ServiceLocator.ScanningService.ScanFiles(filesToScan, false, _settings.StoreMetadata, _settings.StoreWorkflow, ServiceLocator.ProgressService.CancellationToken);

                            ServiceLocator.ScanningService.Report(added, 0, elapsed, false, false, false);
                        }
                        finally
                        {
                            ServiceLocator.ProgressService.CompleteTask();
                        }
                    }
                });
            }
        }

        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private List<string>? _detectedFiles;
        private Timer? t = null;
        private object _lock = new object();



        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                QueueFile(e.FullPath);
            }
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            var wasTmp = Path.GetExtension(e.OldFullPath).ToLowerInvariant() == ".tmp";

            if (wasTmp && _settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                QueueFile(e.FullPath);
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

        private void QueueFile(string path)
        {
            lock (_lock)
            {
                if (File.Exists(path))
                {
                    var attr = File.GetAttributes(path);

                    if (attr.HasFlag(FileAttributes.Directory))
                        return;

                    if (t == null)
                    {
                        _detectedFiles = new List<string>();
                        t = new Timer(ProcessQueueCallback, null, 2000, Timeout.Infinite);
                    }
                    else
                    {
                        t.Change(2000, Timeout.Infinite);
                    }
                    _detectedFiles.Add(path);
                }
            }
        }

        private int addedTotal = 0;

        private void ProcessQueueCallback(object? state)
        {
            int added;
            long elapsed;

            lock (_lock)
            {
                t?.Dispose();
                t = null;

                var filteredFiles = _detectedFiles.Where(f => !_settings.ExcludePaths.Any(p => f.StartsWith(p))).ToList();

                (added, elapsed) = ServiceLocator.ScanningService.ScanFiles(filteredFiles, false, _settings.StoreMetadata, _settings.StoreWorkflow, CancellationToken.None);
            }

            if (added > 0)
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
            }
        }
    }
}

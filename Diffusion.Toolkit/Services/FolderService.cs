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
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Configuration;
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
            ServiceLocator.DataStore.DataChanged += DataChangedEventHandler;
        }

        private void DataChangedEventHandler(object? sender, DataChangedEventArgs e)
        {
            if (e.EntityType == EntityType.Folder)
            {
                switch (e.SourceType)
                {
                    case SourceType.Item:
                        if (e.Property is "Excluded")
                        {
                            _isExcludedFoldersDirty = true;
                        }
                        else if (e.Property is "Archived")
                        {
                            _isArchivedFoldersDirty = true;
                        }
                        break;
                    case SourceType.Collection:
                        _isExcludedFoldersDirty = true;
                        _isArchivedFoldersDirty = true;
                        _isFoldersDirty = true;
                        break;
                }
            }
        }

        private void ApplyDBFolderChanges(IEnumerable<FolderChange> folderChanges)
        {
            var removed = 0;
            var updated = 0;
            var added = 0;

            foreach (var folderChange in folderChanges.Where(d => d.FolderType == FolderType.Watched))
            {
                if (folderChange.ChangeType == ChangeType.Add)
                {
                    added += _dataStore.AddRootFolder(folderChange.Path, folderChange.Recursive);
                }
                else if (folderChange.ChangeType == ChangeType.Remove)
                {
                    removed += _dataStore.RemoveFolder(folderChange.Path);
                }
                else if (folderChange.ChangeType == ChangeType.ChangePath)
                {
                    updated += _dataStore.ChangeFolderPath(folderChange.Path, folderChange.NewPath);
                }
            }


            foreach (var folderChange in folderChanges.Where(d => d.FolderType == FolderType.Excluded))
            {
                if (folderChange.ChangeType == ChangeType.Add)
                {
                    _dataStore.SetFolderExcluded(folderChange.Path, true, false);
                }
                else if (folderChange.ChangeType == ChangeType.Remove)
                {
                    _dataStore.SetFolderExcluded(folderChange.Path, true, false);
                }
                //else if (folderChange.ChangeType == ChangeType.ChangePath)
                //{
                //    updated += _dataStore.ChangeFolderPath(folderChange.Path, folderChange.NewPath);
                //}
            }
        }

        public bool HasRootFolders
        {
            get
            {
                return RootFolders.Any();
            }
        }

        private bool _isFoldersDirty = true;
        private IReadOnlyCollection<Folder> _folders;

        public IReadOnlyCollection<Folder> RootFolders
        {
            get
            {
                if (_isFoldersDirty)
                {
                    _folders = ServiceLocator.DataStore.GetRootFolders().ToList();
                    _isFoldersDirty = false;
                }
                return _folders;
            }
        }

        //public IEnumerable<Folder> GetRootFolders()
        //{
        //    return ServiceLocator.DataStore.GetRootFolders();
        //}

        private void UpdateFolder(FolderViewModel folderView, Dictionary<int, Folder> lookup)
        {
            if (lookup.TryGetValue(folderView.Id, out var folder))
            {
                folderView.IsArchived = folder.Archived;
                folderView.IsUnavailable = folder.Unavailable;
                folderView.IsExcluded = folder.Excluded;
                //if (folderView.Children is { Count: > 0 })
                //{
                //    foreach (var child in folderView.Children)
                //    {
                //        UpdateFolder(child, lookup);
                //    }
                //}
            }
        }

        public async Task LoadFolders()
        {
            var folders = ServiceLocator.DataStore.GetFolders().ToList();

            var lookup = folders.ToDictionary(d => d.Id);

            _dispatcher.Invoke(() =>
            {
                if (ServiceLocator.MainModel.Folders == null || ServiceLocator.MainModel.Folders.Count == 0)
                {
                    ServiceLocator.MainModel.Folders = new ObservableCollection<FolderViewModel>(folders.Where(d => d.IsRoot).Select(folder => new FolderViewModel()
                    {
                        Id = folder.Id,
                        HasChildren = true,
                        Visible = true,
                        Depth = 0,
                        Name = Path.GetFileName(folder.Path),
                        Path = folder.Path,
                        IsArchived = folder.Archived,
                        IsExcluded = folder.Excluded,
                        IsUnavailable = !Directory.Exists(folder.Path),
                    }));
                }
                else
                {
                    foreach (var folder in ServiceLocator.MainModel.Folders)
                    {
                        UpdateFolder(folder, lookup);
                    }
                }
            });

            await ServiceLocator.ScanningService.CheckUnavailableFolders();

        }


        public ObservableCollection<FolderViewModel> GetSubFolders(FolderViewModel folder)
        {
            if (!Directory.Exists(folder.Path))
            {
                return new ObservableCollection<FolderViewModel>();
            }

            var subfolders = ServiceLocator.DataStore.GetSubfolders(folder.Id);

            var subViews = subfolders.Select(sub => new FolderViewModel()
            {
                Id = sub.Id,
                Parent = folder,
                HasChildren = true,
                Visible = true,
                Depth = folder.Depth + 1,
                Name = Path.GetFileName(sub.Path),
                Path = sub.Path,
                IsArchived = sub.Archived,
                IsUnavailable = sub.Unavailable,
                IsExcluded = sub.Excluded,
            }).ToList();

            var lookup = subViews.Select(p => p.Path).ToHashSet();

            var directories = Directory.GetDirectories(folder.Path, "*", new EnumerationOptions()
            {
                IgnoreInaccessible = true
            }).Where(path => !lookup.Contains(path)).Select(sub => new FolderViewModel()
            {
                Parent = folder,
                HasChildren = true,
                Visible = true,
                Depth = folder.Depth + 1,
                Name = Path.GetFileName(sub),
                Path = sub,
            });

            return new ObservableCollection<FolderViewModel>(subViews.Concat(directories).OrderBy(d => d.Name));
        }



        public async Task ApplyFolderChanges(IEnumerable<FolderChange> folderChanges)
        {
            var addedFolders = new List<string>();

            ApplyDBFolderChanges(folderChanges);

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
                if (!ServiceLocator.FolderService.IsExcludedOrArchivedFile(e.FullPath))
                {
                    ServiceLocator.ProgressService.StartTask().ContinueWith(t =>
                    {
                        _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, ServiceLocator.ProgressService.CancellationToken);
                    });
                }
            }
        }

        private bool _isExcludedFoldersDirty = true;

        IReadOnlyCollection<Folder> _excludedFolders;

        public IReadOnlyCollection<Folder> ExcludedFolders
        {
            get
            {
                if (_isExcludedFoldersDirty)
                {
                    _excludedFolders = ServiceLocator.DataStore.GetExcludedFolders().ToList();
                    _excludedOrArchivedFolderPaths = ServiceLocator.DataStore.GetExcludedFolders().Concat(ServiceLocator.DataStore.GetArchivedFolders()).Select(d => d.Path).ToHashSet();

                    _isExcludedFoldersDirty = false;
                }

                return _excludedFolders;
            }
        }

        private bool _isArchivedFoldersDirty = true;

        IReadOnlyCollection<Folder> _archivedFolders;

        public IReadOnlyCollection<Folder> ArchivedFolders
        {
            get
            {
                if (_isArchivedFoldersDirty)
                {
                    _archivedFolders = ServiceLocator.DataStore.GetArchivedFolders().ToList();
                    _excludedOrArchivedFolderPaths = ServiceLocator.DataStore.GetExcludedFolders().Concat(ServiceLocator.DataStore.GetArchivedFolders()).Select(d => d.Path).ToHashSet();

                    _isArchivedFoldersDirty = false;
                }

                return _archivedFolders;
            }
        }


        private HashSet<string> _excludedOrArchivedFolderPaths;
        public HashSet<string> ExcludedOrArchivedFolderPaths
        {
            get
            {
                return _excludedOrArchivedFolderPaths;
            }
        }

        private bool IsExcludedOrArchivedFile(string path)
        {
            var directory = Path.GetDirectoryName(path);
            return ExcludedOrArchivedFolderPaths.Contains(directory);
        }

        private bool IsExcludedOrArchivedFolder(string path)
        {
            return ExcludedOrArchivedFolderPaths.Contains(path);
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            var wasTmp = Path.GetExtension(e.OldFullPath).ToLowerInvariant() == ".tmp";

            if (wasTmp && _settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                if (!ServiceLocator.FolderService.IsExcludedOrArchivedFile(e.FullPath))
                {
                    ServiceLocator.ProgressService.StartTask().ContinueWith(t => { _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, ServiceLocator.ProgressService.CancellationToken); });
                }
            }
        }

        public void CreateWatchers()
        {
            foreach (var path in ServiceLocator.FolderService.RootFolders.Select(d => d.Path))
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

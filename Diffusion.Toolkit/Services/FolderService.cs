using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Database.Models;
using Diffusion.IO;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Thumbnails;
using File = System.IO.File;

namespace Diffusion.Toolkit.Services
{
    public class PathComparer : IEqualityComparer<FolderViewModel>
    {
        public bool Equals(FolderViewModel? x, FolderViewModel? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Path.Equals(y.Path, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(FolderViewModel obj)
        {
            return obj.Path.GetHashCode();
        }
    }

    public class FolderService
    {
        private string GetLocalizedText(string key)
        {
            return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
        }


        private Settings _settings => ServiceLocator.Settings!;

        private DataStore _dataStore => ServiceLocator.DataStore!;

        private Dispatcher _dispatcher => ServiceLocator.Dispatcher;

        public FolderService()
        {
        }

        private void ApplyDBFolderChanges(IEnumerable<FolderChange> folderChanges)
        {
            foreach (var folderChange in folderChanges.Where(d => d.FolderType == FolderType.Watched))
            {
                if (folderChange.ChangeType == ChangeType.Add)
                {
                    _dataStore.AddRootFolder(folderChange.Path, folderChange.Recursive);
                }
                else if (folderChange.ChangeType == ChangeType.Remove)
                {
                    _dataStore.RemoveFolder(folderChange.Path);
                }
                else if (folderChange.ChangeType == ChangeType.ChangePath)
                {
                    _dataStore.ChangeFolderPath(folderChange.Path, folderChange.NewPath);
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

        public IEnumerable<FolderViewModel> SelectedFolders => ServiceLocator.MainModel.Folders.Where(d => d.IsSelected);

        public bool HasRootFolders
        {
            get
            {
                return RootFolders.Any();
            }
        }

        public void ClearCache()
        {
            _rootFolders = null;
            _allFolders = null;
            _archivedStatus = null;
            _excludedFolders = null;
            _archivedFolders = null;
            _excludedOrArchivedFolderPaths = null;
        }

        private IReadOnlyCollection<Folder>? _rootFolders;
        private IReadOnlyCollection<Folder>? _allFolders;

        private IDictionary<int, bool>? _archivedStatus;
        private IReadOnlyCollection<Folder>? _excludedFolders;
        private IReadOnlyCollection<Folder>? _archivedFolders;
        private HashSet<string>? _excludedOrArchivedFolderPaths;

        public void RefreshData()
        {
            _archivedStatus = null;
            _rootFolders = null;
            _allFolders = null;
            _archivedFolders = null;
            _excludedOrArchivedFolderPaths = null;
        }

        /// <summary>
        /// Returns true if the file path is within any of the root folders, and none of the excluded or archived folders
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsScannable(string path)
        {
            return ServiceLocator.FolderService.RootFolders.Any(d => path.StartsWith(d.Path)) &&
                !ServiceLocator.FolderService.ExcludedOrArchivedFolderPaths.Any(d => path.StartsWith(d));
        }


        public bool IsExcludedOrArchived(string path)
        {
            return ServiceLocator.FolderService.ExcludedOrArchivedFolderPaths.Any(d => path.Equals(d, StringComparison.OrdinalIgnoreCase));
        }

        public IDictionary<int, bool> ArchivedStatus
        {
            get
            {
                return _archivedStatus ?? (_archivedStatus = ServiceLocator.DataStore.GetArchivedStatus()
                    .ToDictionary(d => d.Id, d => d.Archived));
            }
        }

        public IReadOnlyCollection<Folder> RootFolders
        {
            get { return _rootFolders ?? (_rootFolders = ServiceLocator.DataStore.GetRootFolders().ToList()); }
        }

        public IReadOnlyCollection<Folder> AllFolders
        {
            get { return _allFolders ?? (_allFolders = ServiceLocator.DataStore.GetFolders().ToList()); }
        }



        public IReadOnlyCollection<Folder> ExcludedFolders
        {
            get
            {
                return _excludedFolders ?? (_excludedFolders = ServiceLocator.DataStore.GetExcludedFolders().ToList());
            }
        }

        public IReadOnlyCollection<Folder> ArchivedFolders
        {
            get
            {
                return _archivedFolders ?? (_archivedFolders = ServiceLocator.DataStore.GetArchivedFolders().ToList());
            }
        }

        public HashSet<string> ExcludedOrArchivedFolderPaths
        {
            get
            {
                return _excludedOrArchivedFolderPaths ?? (_excludedOrArchivedFolderPaths = ServiceLocator.DataStore
                    .GetExcludedFolders().Concat(ServiceLocator.DataStore.GetArchivedFolders()).Select(d => d.Path)
                    .ToHashSet());
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

        private void UpdateFolder(FolderViewModel folderView, Dictionary<string, FolderView> lookup)
        {
            if (lookup.TryGetValue(folderView.Path, out var folder))
            {
                folderView.Id = folder.Id;
                folderView.IsArchived = folder.Archived;
                folderView.IsUnavailable = folder.Unavailable;
                folderView.IsExcluded = folder.Excluded;
                folderView.IsScanned = true;
            }
        }

        public void RefreshFolder(FolderViewModel folder)
        {
            var folders = ServiceLocator.DataStore.GetFoldersView().ToList();

            var lookup = folders.ToDictionary(d => d.Path);

            var comparer = new PathComparer();

            UpdateFolder2(folder, lookup);

            //UpdateFolderChildren(folder, comparer);
        }

        public async Task LoadFolders()
        {
            ClearCache();

            var folders = ServiceLocator.DataStore.GetFoldersView().ToList();

            _dispatcher.Invoke(() =>
                {
                    if (ServiceLocator.MainModel.Folders == null || ServiceLocator.MainModel.Folders.Count == 0)
                    {

                        ServiceLocator.MainModel.Folders = new ObservableCollection<FolderViewModel>(folders.Where(d => d.IsRoot).Select(folder => new FolderViewModel()
                        {
                            Id = folder.Id,
                            HasChildren = folder.HasChildren,
                            Visible = true,
                            Depth = 0,
                            Name = Path.GetFileName(folder.Path),
                            Path = folder.Path,
                            IsArchived = folder.Archived,
                            IsExcluded = folder.Excluded,
                            IsUnavailable = !Directory.Exists(folder.Path),
                            IsScanned = true
                        }));
                    }
                    else
                    {
                        var comparer = new PathComparer();

                        var lookup = folders.ToDictionary(d => d.Path.ToLower());

                        // Start at each root folder
                        foreach (var folder in ServiceLocator.MainModel.Folders.Where(d => d.Depth == 0).ToList())
                        {
                            UpdateFolder2(folder, lookup);
                        }


                        //foreach (var folder in ServiceLocator.MainModel.Folders.ToList())
                        //{
                        //    UpdateFolder(folder, lookup);

                        //    UpdateFolderChildren(folder, comparer);
                        //}
                    }
                });

            await ServiceLocator.ScanningService.CheckUnavailableFolders();

        }

        PathComparer pathComparer = new PathComparer();

        private void UpdateFolder2(FolderViewModel folderView, Dictionary<string, FolderView> lookup)
        {
            var folderExists = Directory.Exists(folderView.Path);

            if (lookup.TryGetValue(folderView.Path.ToLower(), out var folder))
            {
                folderView.Id = folder.Id;
                folderView.IsArchived = folder.Archived;
                folderView.IsUnavailable = folderExists && folder.Unavailable;
                folderView.IsExcluded = folder.Excluded;
                folderView.IsScanned = true;
            }
            else
            {
                folderView.Id = 0;
                folderView.IsArchived = false;
                folderView.IsUnavailable = false;
                folderView.IsExcluded = false;
                folderView.IsScanned = false;
            }

            if (folderExists)
            {
                folderView.HasChildren = Directory.GetDirectories(folderView.Path, "*").Length > 0;
            }

            if (folderView.State == FolderState.Expanded)
            {
                var diskSubFolders = GetDiskSubFolders(folderView);

                var parentIndex = ServiceLocator.MainModel.Folders.IndexOf(folderView);
                var parentDepth = folderView.Depth;

                foreach (var child in diskSubFolders)
                {
                    var existingChild = folderView.Children.FirstOrDefault(d =>
                        d.Path.Equals(child.Path, StringComparison.OrdinalIgnoreCase));

                    if (existingChild != null)
                    {
                        UpdateFolder2(existingChild, lookup);
                        existingChild.HasChildren = child.HasChildren;
                    }
                    else
                    {
                        InsertChild(parentIndex, parentDepth, child);
                        folderView.Children.Add(child);
                    }
                }

                foreach (var child in folderView.Children.ToList())
                {
                    if (!Directory.Exists(child.Path) && !child.IsScanned)
                    {
                        var grandChildren = GetVisualChildren(child).ToList();

                        foreach (var grandChild in grandChildren)
                        {
                            ServiceLocator.MainModel.Folders.Remove(grandChild);
                        }

                        ServiceLocator.MainModel.Folders.Remove(child);

                        folderView.Children.Remove(child);
                    }
                }
            }
        }


        public void UpdateFolderChildrenByPath(string path)
        {
            var comparer = new PathComparer();

            var folder = ServiceLocator.MainModel.Folders.FirstOrDefault(d => d.Path == path);

            if (folder != null)
            {
                UpdateFolderChildren(folder, comparer);
            }
        }

        private void UpdateFolderChildren(FolderViewModel folder, PathComparer comparer)
        {
            if (folder.State == FolderState.Expanded && Directory.Exists(folder.Path))
            {
                var driveFolders = GetDiskSubFolders(folder);

                if (driveFolders.Any())
                {
                    if (folder.Children == null)
                    {
                        folder.Children = new ObservableCollection<FolderViewModel>();
                    }

                    driveFolders = driveFolders.Except(folder.Children, comparer);

                    var insertPoint = ServiceLocator.MainModel.Folders.IndexOf(folder) + 1;

                    ServiceLocator.Dispatcher.Invoke(() =>
                    {
                        foreach (var subFolder in driveFolders)
                        {
                            InsertChild(insertPoint, folder.Depth, subFolder);
                            folder.Children.Add(subFolder);
                        }
                    });
                }

                var visualChildren = ServiceLocator.Dispatcher.Invoke(() => GetVisualChildren(folder).ToList());

                foreach (var child in visualChildren)
                {
                    if (!Directory.Exists(child.Path))
                    {
                        ServiceLocator.Dispatcher.Invoke(() =>
                        {
                            ServiceLocator.MainModel.Folders.Remove(child);
                        });
                    }
                }
            }
        }

        public void UpdateChildPaths(FolderViewModel parentFolder)
        {
            if (parentFolder.Children != null)
            {
                var path = parentFolder.Path;

                foreach (var child in parentFolder.Children)
                {
                    child.Path = Path.Combine(path, child.Name);
                    UpdateChildPaths(child);
                }
            }
        }

        public IEnumerable<FolderViewModel> GetVisualChildren(FolderViewModel folder)
        {
            var currentIndex = ServiceLocator.MainModel.Folders.IndexOf(folder);
            var parentDepth = folder.Depth;

            FolderViewModel currentFolder;
            do
            {
                currentIndex++;
                currentFolder = ServiceLocator.MainModel.Folders[currentIndex];
                if (currentFolder.Depth <= parentDepth || currentIndex > ServiceLocator.MainModel.Folders.Count - 1)
                {
                    yield break;
                }
                yield return currentFolder;
            }
            while (currentFolder.Depth > parentDepth && currentIndex < ServiceLocator.MainModel.Folders.Count);
        }

        /// <summary>
        /// Inserts a folder as a child of another folder in the visual list, in alphabetical order
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="parentDepth"></param>
        /// <param name="childFolder"></param>
        public void InsertChild(int currentIndex, int parentDepth, FolderViewModel childFolder)
        {
            var lastIndex = ServiceLocator.MainModel.Folders.Count - 1;

            FolderViewModel currentFolder;
            do
            {
                currentIndex++;

                if (currentIndex >= lastIndex)
                {
                    currentIndex = lastIndex + 1;
                    break;
                }

                currentFolder = ServiceLocator.MainModel.Folders[currentIndex];

                if (String.Compare(childFolder.Name, currentFolder.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    break;
                }
            } while (currentFolder.Depth > parentDepth && currentIndex < ServiceLocator.MainModel.Folders.Count);

            ServiceLocator.MainModel.Folders.Insert(currentIndex, childFolder);

            childFolder.Visible = true;
        }

        public void AppendChild(FolderViewModel parentFolder, FolderViewModel childFolder)
        {
            if (parentFolder.Children == null)
            {
                parentFolder.Children = new ObservableCollection<FolderViewModel>();
            }

            var insertIndex = 0;

            foreach (var child in parentFolder.Children)
            {
                if (String.Compare(childFolder.Name, child.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    break;
                }
                insertIndex++;
            }

            parentFolder.Children.Insert(insertIndex, childFolder);

            parentFolder.HasChildren = true;

            if (parentFolder.State == FolderState.Expanded)
            {
                var parentIndex = ServiceLocator.MainModel.Folders.IndexOf(parentFolder);
                var parentDepth = parentFolder.Depth;

                InsertChild(parentIndex, parentDepth, childFolder);
            }
        }


        public void RefreshFolderOld(FolderViewModel targetFolder)
        {
            var subFolders = ServiceLocator.FolderService.GetSubFolders(targetFolder).ToList();

            // TODO: prevent updating of state and MainModel.Folders if no visual update is required

            ServiceLocator.Dispatcher.Invoke(() =>
            {

                if (targetFolder.HasChildren)
                {
                    var addedFolders = subFolders.Except(targetFolder.Children).ToList();
                    var removedFolders = targetFolder.Children.Except(subFolders).ToList();

                    var insertPoint = ServiceLocator.MainModel.Folders.IndexOf(targetFolder) + 1;

                    foreach (var folder in addedFolders)
                    {
                        targetFolder.Children.Add(folder);
                        ServiceLocator.MainModel.Folders.Insert(insertPoint, folder);
                    }

                    foreach (var folder in removedFolders)
                    {
                        targetFolder.Children.Remove(folder);
                        ServiceLocator.MainModel.Folders.Remove(folder);
                    }
                }
                else
                {
                    targetFolder.Children = new ObservableCollection<FolderViewModel>(subFolders);

                    var insertPoint = ServiceLocator.MainModel.Folders.IndexOf(targetFolder) + 1;

                    foreach (var folder in subFolders)
                    {
                        ServiceLocator.MainModel.Folders.Insert(insertPoint, folder);
                    }

                }

                if (targetFolder.HasChildren)
                {
                    targetFolder.State = FolderState.Expanded;
                }
            });

        }

        public IEnumerable<FolderViewModel> GetDiskSubFolders(FolderViewModel folder)
        {
            try
            {
                return Directory.GetDirectories(folder.Path, "*", new EnumerationOptions()
                {
                    IgnoreInaccessible = true
                }).Select(sub => new FolderViewModel()
                {
                    Parent = folder,
                    HasChildren = Directory.GetDirectories(sub, "*").Length > 0,
                    Visible = true,
                    Depth = folder.Depth + 1,
                    Name = Path.GetFileName(sub),
                    Path = sub,
                });
            }
            catch (DirectoryNotFoundException ex)
            {
                return Enumerable.Empty<FolderViewModel>();
            }
        }

        public ObservableCollection<FolderViewModel> GetSubFolders(FolderViewModel folder)
        {
            if (!Directory.Exists(folder.Path))
            {
                return new ObservableCollection<FolderViewModel>();
            }

            var subfolders = ServiceLocator.DataStore.GetSubFoldersView(folder.Id);

            var subViews = subfolders.Select(sub => new FolderViewModel()
            {
                Id = sub.Id,
                Parent = folder,
                HasChildren = sub.HasChildren,
                Visible = true,
                Depth = folder.Depth + 1,
                Name = Path.GetFileName(sub.Path),
                Path = sub.Path,
                IsArchived = sub.Archived,
                IsUnavailable = !Directory.Exists(sub.Path),
                IsExcluded = sub.Excluded,
                IsScanned = true,
            }).ToList();

            var lookup = subViews.Select(p => p.Path).ToHashSet();

            var directories = Directory.GetDirectories(folder.Path, "*", new EnumerationOptions()
            {
                IgnoreInaccessible = true
            }).Where(path => !lookup.Contains(path)).Select(sub => new FolderViewModel()
            {
                Parent = folder,
                HasChildren = Directory.GetDirectories(sub).Length > 0,
                Visible = true,
                Depth = folder.Depth + 1,
                Name = Path.GetFileName(sub),
                Path = sub,
            });

            return new ObservableCollection<FolderViewModel>(subViews.Concat(directories).OrderBy(d => d.Name));
        }

        public async Task ApplyFolderChanges(IEnumerable<FolderChange> folderChanges, bool confirmScan = false)
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
                PopupResult result = PopupResult.Yes;

                if (confirmScan)
                {
                    result = await ServiceLocator.MessageService.Show("Do you want to scan your folders now?", "Settings Updated", PopupButtons.YesNo);
                }

                if (result == PopupResult.Yes)
                {
                    var filesToScan = new List<string>();

                    if (await ServiceLocator.ProgressService.TryStartTask())
                    {
                        var cancellationToken = ServiceLocator.ProgressService.CancellationToken;

                        foreach (var folder in addedFolders)
                        {
                            filesToScan.AddRange(await ServiceLocator.ScanningService.GetFilesToScan(folder, new HashSet<string>(), cancellationToken));
                        }

                        await ServiceLocator.MetadataScannerService.QueueBatchAsync(filesToScan, null, cancellationToken);
                    }
                }

            }
        }

        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

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
                    watcher.Changed += WatcherOnChanged;
                    watcher.Deleted += WatcherOnDeleted;
                    _watchers.Add(watcher);
                }
            }
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (Path.GetFileName(e.FullPath) == "dt_thumbnails.db-journal")
                {
                    return;
                }

                FileAttributes attr = File.GetAttributes(e.FullPath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    if (!IsExcludedOrArchived(e.FullPath))
                    {
                        //var parent = Path.GetDirectoryName(e.FullPath);
                        ////TODO: BUggy when a folder is copied in
                        //UpdateFolderChildrenByPath(parent);

                        //ServiceLocator.ProgressService.StartTask().ContinueWith(t =>
                        //{
                        //    var extensions = ServiceLocator.Settings.FileExtensions;
                        //    var excludePaths = ServiceLocator.FolderService.ExcludedOrArchivedFolderPaths;

                        //    foreach (var file in MetadataScanner.GetFiles(e.FullPath, extensions, true, excludePaths, CancellationToken.None))
                        //    {
                        //        _ = ServiceLocator.MetadataScannerService.QueueAsync(file, ServiceLocator.ProgressService.CancellationToken);
                        //    }
                        //});
                    }

                }
                else
                {
                    if (ServiceLocator.FileService.IsRegisteredExtension(e.FullPath))
                    {
                        if (!IsExcludedOrArchived(Path.GetDirectoryName(e.FullPath)))
                        {
                            ServiceLocator.ProgressService.StartTask().ContinueWith(t =>
                            {
                                _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, CancellationToken.None);
                            });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Log(exception.Message + " " + exception.StackTrace);
            }

        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(e.FullPath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    if (!IsExcludedOrArchived(e.FullPath))
                    {
                        if (ServiceLocator.DataStore.FolderHasImages(e.OldFullPath))
                        {
                            ServiceLocator.DataStore.ChangeFolderPath(e.OldFullPath, e.FullPath);
                        }
                        else
                        {
                            UpdateFolderChildrenByPath(e.FullPath);
                        }
                    }
                }
                else
                {
                    if (ServiceLocator.FileService.IsRegisteredExtension(e.FullPath))
                    {
                        if (!IsExcludedOrArchivedFile(e.FullPath))
                        {
                            ServiceLocator.ProgressService.StartTask().ContinueWith(t => { _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, ServiceLocator.ProgressService.CancellationToken); });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Log(exception.Message + " " + exception.StackTrace);
            }
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileName(e.FullPath) == "dt_thumbnails.db-journal")
            {
                return;
            }

            var x = e;
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            var x = e;
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

        public void SetFoldersDirty()
        {

        }

        public void Delete(string path)
        {
            if (ServiceLocator.Settings.PermanentlyDelete)
            {
                Directory.Delete(path);
            }
            else
            {
                Win32FileAPI.Recycle(path);
            }
        }

        public async Task<bool> ShowDeleteFolderDialog(List<FolderViewModel> selectedFolders)
        {
            PopupResult result;

            var title = GetLocalizedText("Actions.Folders.Delete.Title");

            if (selectedFolders.Count > 1)
            {
                result = await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.DeleteSelection.Message"), title, PopupButtons.YesNo);
            }
            else
            {
                result = await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Delete.Message").Replace("{folder}", selectedFolders[0].Name), title, PopupButtons.YesNo);
            }

            if (result == PopupResult.Yes)
            {

                var count = 0;
                foreach (var model in selectedFolders)
                {
                    if (Directory.Exists(model.Path))
                    {
                        ThumbnailCache.Instance.Unload(model.Path);
                        ServiceLocator.FolderService.Delete(model.Path);
                    }

                    ServiceLocator.DataStore.RemoveFolder(model.Path);

                    ServiceLocator.Dispatcher.Invoke(() =>
                    {
                        //ServiceLocator.MainModel.Folders.Remove(model);
                        RemoveFolder(model);
                        model.IsSelected = false;
                        model.Parent!.Children!.Remove(model);
                    });

                    count++;
                }

                return true;
            }

            return false;
        }

        private void RemoveFolder(FolderViewModel folder)
        {
            if (folder.Children != null)
            {
                foreach (var child in folder.Children)
                {
                    RemoveFolder(child);
                }
            }

            ServiceLocator.MainModel.Folders.Remove(folder);
        }


        public async Task ShowCreateFolderDialog(FolderViewModel parentFolder)
        {
            var title = GetLocalizedText("Actions.Folders.Create.Title");

            var (result, name) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Folders.Create.Message"), title);

            if (result == PopupResult.OK)
            {


                if (!FileUtility.IsValidFilename(name))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Invalid.Message"), title);
                    return;
                }

                var newPath = Path.Combine(parentFolder.Path, name);

                if (Directory.Exists(newPath))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Exists.Message").Replace("{folder}", name), title);
                    return;
                }

                var directory = new DirectoryInfo(parentFolder.Path);

                if (directory.Exists)
                {
                    directory.CreateSubdirectory(name);

                    ServiceLocator.FolderService.AppendChild(parentFolder, new FolderViewModel()
                    {
                        Parent = parentFolder,
                        Name = name,
                        Depth = parentFolder.Depth + 1,
                        Path = newPath,
                        IsScanned = false
                    });


                    //ServiceLocator.FolderService.RefreshFolder(parentFolder);
                }

            }
        }

        public async Task<bool> ShowRemoveFolderDialog(FolderViewModel folder)
        {
            var title = GetLocalizedText("Actions.Folders.Remove.Title");

            var result = await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Remove.Message").Replace("{folder}", folder.Name), title, PopupButtons.YesNo);

            if (result == PopupResult.Yes)
            {
                ServiceLocator.DataStore.RemoveFolder(folder.Id);

                UpdateFolder2(folder, new Dictionary<string, FolderView>());
                return true;
            }
            return false;
        }

        public async Task<bool> ShowRenameFolderDialog(FolderViewModel folder)
        {
            var title = GetLocalizedText("Actions.Folders.Rename.Title");

            var oldName = folder.Name;
            var oldPath = folder.Path;
            var id = folder.Id;

            var (result, newName) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Folders.Rename.Message"), title, oldName);

            if (result == PopupResult.OK)
            {
                var parentPath = Path.GetDirectoryName(oldPath);
                var newPath = Path.Combine(parentPath, newName);

                if (!FileUtility.IsValidFilename(newName))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Invalid.Message"), title);
                    return false;
                }

                if (!oldPath.Equals(newPath, StringComparison.CurrentCultureIgnoreCase) && Directory.Exists(newPath))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Exists.Message").Replace("{folder}", newName), title);
                    return false;
                }

                //if (oldName.Equals(newName, StringComparison.CurrentCultureIgnoreCase))
                //{
                //    return (false, null, null);
                //}

                try
                {
                    DisableWatchers();
                    //Update filesystem
                    ThumbnailCache.Instance.Unload(oldPath);

                    Directory.Move(oldPath, newPath);

                    // Update database
                    ServiceLocator.DataStore.ChangeFolderPath(oldPath, newPath);

                    // Update UI

                    var visualChildren = GetVisualChildren(folder).ToList();
                    foreach (var child in visualChildren)
                    {
                        Debug.WriteLine(child.Name);
                        ServiceLocator.MainModel.Folders.Remove(child);
                    }
                    ServiceLocator.MainModel.Folders.Remove(folder);
                    folder.Parent.Children.Remove(folder);

                    folder.Name = newName;
                    folder.Path = newPath;

                    UpdateChildPaths(folder);
                    AppendChild(folder.Parent, folder);
                    ReinsertChildren(folder);

                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show(ServiceLocator.WindowService.CurrentWindow, ex.Message, "Error moving folder",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    return false;
                }
                finally
                {
                    EnableWatchers();
                }

                return true;
            }

            return false;
        }

        private void ReinsertChildren(FolderViewModel folder)
        {
            if (folder.HasChildren && folder.Children != null && folder.State == FolderState.Expanded)
            {
                var parentIndex = ServiceLocator.MainModel.Folders.IndexOf(folder);
                var parentDepth = folder.Depth;
                foreach (var child in folder.Children)
                {
                    InsertChild(parentIndex, parentDepth, child);
                    ReinsertChildren(child);
                }
            }
        }


        private void RenameFolderViews(FolderViewModel folder, string source, string dest)
        {
            if (folder.Children != null)
            {
                foreach (var child in folder.Children)
                {
                    RenameFolderViews(child, source, dest);
                }
            }

            var subPath = folder.Path.Substring(source.Length);
            var newPath = Path.Join(dest, subPath);

            folder.Path = newPath;
        }

        //private async Task<bool> MovePath(SQLiteConnection db, string sourcePath, string destinationPath)
        //{
        //    //TODO: Check that destinationPath is under a watched folder
        //    var moved = 0;

        //    if (await ServiceLocator.ProgressService.TryStartTask())
        //    {
        //        try
        //        {
        //            ThumbnailCache.Instance.Unload(sourcePath);

        //            var images = _dataStore.GetAllPathImages(sourcePath).ToList();

        //            ServiceLocator.FolderService.DisableWatchers();

        //            ServiceLocator.ProgressService.InitializeProgress(images.Count);

        //            var folderCache = ServiceLocator.FolderService.AllFolders.ToDictionary(d => d.Path);

        //            foreach (var image in images)
        //            {
        //                var subPath = image.Path.Substring(sourcePath.Length);
        //                var newPath = Path.Join(destinationPath, subPath);

        //                if (image.Path != newPath)
        //                {
        //                    _dataStore.MoveImage(db, image.Id, newPath, folderCache);

        //                    var moved1 = moved;
        //                    if (moved % 33 == 0)
        //                    {
        //                        image.Path = newPath;

        //                        ServiceLocator.ProgressService.SetProgress(moved1);
        //                        ServiceLocator.ProgressService.SetStatus(
        //                            $"Moving {ServiceLocator.MainModel.CurrentProgress:#,###,###} of {ServiceLocator.MainModel.TotalProgress:#,###,###}...");
        //                    }

        //                    moved++;
        //                }
        //                else
        //                {
        //                    ServiceLocator.MainModel.TotalProgress--;
        //                }
        //            }

        //            return true;
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Log("MovePath: " + e.Message);
        //            await ServiceLocator.MessageService.ShowMedium($"An error occured while moving a folder:\r\n{e.Message}", "Move images", PopupButtons.OK);
        //            return false;
        //        }
        //        finally
        //        {
        //            ServiceLocator.ToastService.Toast($"{moved} files were moved.", "Move images");

        //            ServiceLocator.ProgressService.CompleteTask();
        //            ServiceLocator.ProgressService.ClearProgress();
        //            ServiceLocator.ProgressService.SetStatus("");
        //            ServiceLocator.FolderService.EnableWatchers();
        //        }
        //    }

        //    return false;
        //}
        public void ClearSelection()
        {
            foreach (var model in SelectedFolders)
            {
                model.IsSelected = false;
            }
        }


        public void NavigateToParentFolder()
        {
            if (ServiceLocator.SearchService.CurrentViewMode == ViewMode.Folder)
            {
                var path = ServiceLocator.MainModel.CurrentFolder.Path;

                var parentPath = Path.GetDirectoryName(path);

                var folder = ServiceLocator.MainModel.Folders.FirstOrDefault(d => d.Path == parentPath);

                if (folder == null)
                {
                    // Fake the folder
                    folder = new FolderViewModel()
                    {
                        Path = path,
                        IsScanned = false
                    };

                    if (!Directory.Exists(parentPath))
                    {
                        folder.IsUnavailable = true;
                    }
                }

                ServiceLocator.FolderService.ClearSelection();

                ServiceLocator.SearchService.ExecuteOpenFolder(folder);
            }
        }

        public IEnumerable<ImagePath> GetFiles(int folderId, bool recursive)
        {
            return ServiceLocator.DataStore.GetFolderImages(folderId, recursive);
        }
    }
}

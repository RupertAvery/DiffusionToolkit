using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;

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
            return x.Path == y.Path;
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

        public IEnumerable<FolderViewModel> SelectedFolders => ServiceLocator.MainModel.Folders.Where(d => d.IsSelected);

        public bool HasRootFolders
        {
            get
            {
                return RootFolders.Any();
            }
        }

        private bool _isFoldersDirty = true;
        private bool _isArchivedFoldersDirty = true;

        private IReadOnlyCollection<Folder> _rootFolders;
        private IReadOnlyCollection<Folder> _allFolders;

        private IDictionary<int, bool> _archivedStatus;
        private IReadOnlyCollection<Folder> _excludedFolders;
        private IReadOnlyCollection<Folder> _archivedFolders;
        private HashSet<string> _excludedOrArchivedFolderPaths;

        private void RefreshData()
        {
            _archivedStatus = ServiceLocator.DataStore.GetArchivedStatus().ToDictionary(d => d.Id, d => d.Archived);
            _rootFolders = ServiceLocator.DataStore.GetRootFolders().ToList();
            _allFolders = ServiceLocator.DataStore.GetFolders().ToList();
            _archivedFolders = ServiceLocator.DataStore.GetArchivedFolders().ToList();
        }


        public IDictionary<int, bool> ArchivedStatus
        {
            get
            {
                if (_isArchivedFoldersDirty)
                {
                    _archivedStatus = ServiceLocator.DataStore.GetArchivedStatus().ToDictionary(d => d.Id, d => d.Archived);
                    _archivedFolders = ServiceLocator.DataStore.GetArchivedFolders().ToList();
                    _isArchivedFoldersDirty = false;
                }

                return _archivedStatus;
            }
        }

        public IReadOnlyCollection<Folder> RootFolders
        {
            get
            {
                if (_isFoldersDirty)
                {
                    _rootFolders = ServiceLocator.DataStore.GetRootFolders().ToList();
                    _allFolders = ServiceLocator.DataStore.GetFolders().ToList();
                    _isFoldersDirty = false;
                }
                return _rootFolders;
            }
        }

        public IReadOnlyCollection<Folder> AllFolders
        {
            get
            {
                if (_isFoldersDirty)
                {
                    _rootFolders = ServiceLocator.DataStore.GetRootFolders().ToList();
                    _allFolders = ServiceLocator.DataStore.GetFolders().ToList();
                    _isFoldersDirty = false;
                }
                return _allFolders;
            }
        }


        private bool _isExcludedFoldersDirty = true;


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

        public IReadOnlyCollection<Folder> ArchivedFolders
        {
            get
            {
                if (_isArchivedFoldersDirty)
                {
                    _archivedStatus = ServiceLocator.DataStore.GetArchivedStatus().ToDictionary(d => d.Id, d => d.Archived);
                    _archivedFolders = ServiceLocator.DataStore.GetArchivedFolders().ToList();
                    _isArchivedFoldersDirty = false;
                }

                return _archivedFolders;
            }
        }

        public HashSet<string> ExcludedOrArchivedFolderPaths
        {
            get
            {
                if (_isExcludedFoldersDirty || _isArchivedFoldersDirty || _isFoldersDirty)
                {
                    _excludedOrArchivedFolderPaths = ExcludedFolders.Concat(ArchivedFolders).Select(d => d.Path).ToHashSet();
                }

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

        public async Task LoadFolders()
        {
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

                        var lookup = folders.ToDictionary(d => d.Path);

                        foreach (var folder in ServiceLocator.MainModel.Folders.ToList())
                        {
                            UpdateFolder(folder, lookup);

                            UpdateFolderChildren(folder, comparer);
                        }
                    }
                });

            await ServiceLocator.ScanningService.CheckUnavailableFolders();

        }

        public void UpdateFolderChildrenByPath(string path)
        {
            var comparer = new PathComparer();

            var folder = ServiceLocator.MainModel.Folders.FirstOrDefault(d => d.Path == path);

            UpdateFolderChildren(folder, comparer);
        }

        private void UpdateFolderChildren(FolderViewModel folder, PathComparer comparer)
        {
            if (folder.State == FolderState.Expanded && Directory.Exists(folder.Path))
            {
                var driveFolders = GetDriveSubFolders(folder);

                if (folder.Children != null)
                {
                    driveFolders = driveFolders.Except(folder.Children, comparer);
                }

                if (driveFolders.Any())
                {
                    if (folder.Children == null)
                    {
                        folder.Children = new ObservableCollection<FolderViewModel>();
                    }

                    foreach (var subFolder in driveFolders.Reverse())
                    {
                        if (folder.Children.FirstOrDefault(d => d.Path == subFolder.Path) == null)
                        {
                            var insertPoint = ServiceLocator.MainModel.Folders.IndexOf(folder) + 1;
                            var targetFolder = ServiceLocator.MainModel.Folders[insertPoint];

                            while (subFolder.Path.CompareTo(targetFolder.Path) > 0 && targetFolder.Depth == subFolder.Depth)
                            {
                                insertPoint++;
                                targetFolder = ServiceLocator.MainModel.Folders[insertPoint];
                            }
                            ServiceLocator.MainModel.Folders.Insert(insertPoint, subFolder);
                            folder.Children.Add(subFolder);
                        }
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

        /// <summary>
        /// Inserts a folder as a child of another folder in the visual list, in alphabetical order
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="parentDepth"></param>
        /// <param name="childFolder"></param>
        public void InsertChild(int currentIndex, int parentDepth, FolderViewModel childFolder)
        {
            FolderViewModel currentFolder;
            do
            {
                currentIndex++;
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


        public void RefreshFolder(FolderViewModel targetFolder)
        {
            var subFolders = ServiceLocator.FolderService.GetSubFolders(targetFolder).ToList();

            // TODO: prevent updating of state and MainModel.Folders if no visual update is required

            ServiceLocator.Dispatcher.Invoke(() =>
            {

                if (targetFolder.HasChildren)
                {
                    var addedFolders = subFolders.Except(targetFolder.Children);
                    var removedFolders = targetFolder.Children.Except(subFolders);

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

        public IEnumerable<FolderViewModel> GetDriveSubFolders(FolderViewModel folder)
        {
            try
            {
                return Directory.GetDirectories(folder.Path, "*", new EnumerationOptions()
                {
                    IgnoreInaccessible = true
                }).Select(sub => new FolderViewModel()
                {
                    Parent = folder,
                    HasChildren = Directory.GetDirectories(folder.Path, "*").Length > 0,
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
                    var parent = Path.GetDirectoryName(e.FullPath);
                    //TODO: BUggy when a folder is copied in
                    UpdateFolderChildrenByPath(parent);
                }
                else
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
                    // TODO: doesn't work if the folder is not in the list
                    if (!IsExcludedOrArchivedFolder(e.FullPath))
                    {
                        if (ServiceLocator.DataStore.FolderHasImages(e.OldFullPath))
                        {
                            ServiceLocator.DataStore.ChangeFolderPath(e.OldFullPath, e.FullPath);
                        }
                        // ServiceLocator.ProgressService.StartTask().ContinueWith(t => { _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, ServiceLocator.ProgressService.CancellationToken); });
                    }
                }
                else
                {
                    if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
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
            _isArchivedFoldersDirty = true;
            _isExcludedFoldersDirty = true;
            _isFoldersDirty = true;
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

        public async Task<(bool, string?, string?)> RenameFolder(int id, string oldName, string oldPath)
        {
            var title = GetLocalizedText("Actions.Folders.Rename.Title");

            var (result, newName) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Folders.Rename.Message"), title, oldName);

            if (result == PopupResult.OK)
            {
                var parentPath = Path.GetDirectoryName(oldPath);
                var newPath = Path.Combine(parentPath, newName);

                if (!FileUtility.IsValidFilename(newName))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Invalid.Message"), title);
                    return (false, null, null);
                }

                if (!oldPath.Equals(newPath, StringComparison.CurrentCultureIgnoreCase) && Directory.Exists(newPath))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Exists.Message").Replace("{folder}", newName), title);
                    return (false, null, null);
                }

                //if (oldName.Equals(newName, StringComparison.CurrentCultureIgnoreCase))
                //{
                //    return (false, null, null);
                //}

                try
                {
                    DisableWatchers();
                    //Update filesystem
                    Directory.Move(oldPath, newPath);

                    // Update database
                    ServiceLocator.DataStore.ChangeFolderPath(oldPath, newPath);

                    // Update UI
                    var folder = ServiceLocator.MainModel.Folders.FirstOrDefault(d => d.Path == oldPath);

                    if (folder != null)
                    {
                        ServiceLocator.MainModel.Folders.Remove(folder);
                        folder.Name = newName;
                        folder.Path = newPath;
                        UpdateChildPaths(folder);
                        AppendChild(folder.Parent, folder);
                    }

                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show(ServiceLocator.WindowService.CurrentWindow, ex.Message, "Error moving folder",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    return (false, null, null);
                }
                finally
                {
                    EnableWatchers();
                }

                return (true, newName, newPath);
            }

            return (false, null, null);
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
                if (ServiceLocator.MainModel.CurrentFolder != null)
                {
                    var folder = ServiceLocator.MainModel.CurrentFolder.Parent;
                    ServiceLocator.SearchService.ExecuteOpenFolder(folder);
                }
            }
        }

        public IEnumerable<ImagePath> GetFiles(int folderId, bool recursive)
        {
            return ServiceLocator.DataStore.GetFolderImages(folderId, recursive);
        }
    }
}

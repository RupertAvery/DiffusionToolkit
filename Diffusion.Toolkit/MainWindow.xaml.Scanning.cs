using Diffusion.IO;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private async Task RescanTask(object o)
        {
            if (_settings.ImagePaths.Any())
            {
                _ = Scan().ContinueWith((t) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        _search.ThumbnailListView.ReloadThumbnailsView();
                    });
                });
            }
            else
            {
                await _messagePopupManager.Show("No image paths configured!", "Rescan Folders");
                ShowSettings(null);
            }
        }


        private async Task RebuildTask(object o)
        {
            if (_settings.ImagePaths.Any())
            {
                var message = "This will update the metadata of ALL existing files in the database with current metadata in actual files.\r\n\r\n" +
                              "You only need to do this if you've updated the metadata in the files since they were added or if they contain metadata that an older version of this program didn't store.\r\n\r\n" +
                              "Are you sure you want to continue?";

                var result = await _messagePopupManager.ShowCustom(message, "Rebuild Metadata", PopupButtons.YesNo, 500, 400);
                if (result == PopupResult.Yes)
                {
                    await Rebuild();
                }
            }
            else
            {
                await _messagePopupManager.Show("No image paths configured!", "Rebuild Metadata");
                ShowSettings(null);
            }
        }
        
        private Task Scan(bool isFirstTime = false)
        {
            return Task.Run(async () =>
            {
                if (await ServiceLocator.ProgressService.TryStartTask())
                {
                    try
                    {
                        if (isFirstTime)
                        {
                            _ = Task.Delay(10000).ContinueWith(t =>
                            {
                                ServiceLocator.SearchService.ExecuteSearch();
                                ServiceLocator.MessageService.Show("You may now view your images while we continue to scan your folders in the background", "Welcome to Diffusion Toolkit", PopupButtons.OK);
                            });
                        }

                        var result = await ServiceLocator.ScanningService.ScanWatchedFolders(false, true, ServiceLocator.ProgressService.CancellationToken);
                        if (result && _search != null)
                        {
                            LoadFolders();
                            _search.SearchImages();
                        }
                    }
                    finally
                    {
                        ServiceLocator.ProgressService.CompleteTask();
                        ServiceLocator.ProgressService.SetStatus(GetLocalizedText("Actions.Scanning.Completed"));
                    }
                }

            });
        }

        private async Task Rebuild()
        {
            await Task.Run(async () =>
            {
                if (await ServiceLocator.ProgressService.TryStartTask())
                {
                    try
                    {
                        var result = await ServiceLocator.ScanningService.ScanWatchedFolders(true, true, ServiceLocator.ProgressService.CancellationToken);
                        if (result)
                        {
                            LoadFolders();
                            _search.SearchImages();
                        }
                    }
                    finally
                    {
                        ServiceLocator.ProgressService.CompleteTask();
                        ServiceLocator.ProgressService.SetStatus(GetLocalizedText("Actions.Scanning.Completed"));
                    }
                }

            });
        }

        private async void FixFolders()
        {
            var message = "This will fix scanned images missing in folders\r\n\r\n" +
                          "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Fix Folders", PopupButtons.YesNo, 500, 250);
            if (result == PopupResult.Yes)
            {
                var updated = await Task.Run(FixFoldersInternal);

                if (updated > 0)
                {
                    _search.SearchImages();
                }
            }
        }

        private int FixFoldersInternal()
        {
            int progress = 0;
            int updated = 0;

            var images = _dataStore.GetImagePaths().ToList();
            var folders = _dataStore.GetFolders();

            var folderIdCache = folders.ToDictionary(f => f.Path, f => f.Id);

            Dispatcher.Invoke(() =>
            {
                _model.Status = "Fixing Folders...s";
                _model.TotalProgress = images.Count;
                _model.CurrentProgress = 0;
            });

            foreach (var image in images)
            {
                var dirName = Path.GetDirectoryName(image.Path);

                var skip = false;

                if (folderIdCache.TryGetValue(dirName, out var folderId))
                {
                    if (image.FolderId == folderId)
                    {
                        skip = true;
                    }
                }

                if (!skip)
                {
                    _dataStore.UpdateImageFolderId(image.Id, image.Path, folderIdCache);
                    updated++;
                }

                var currentProgress = progress;
                if (currentProgress % 113 == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _model.CurrentProgress = currentProgress;
                        _model.Status = $"Checking {_model.CurrentProgress:#,###,###} of {_model.TotalProgress:#,###,###}...";
                    });
                }

                progress++;
            }

            _model.Status = "";
            _model.TotalProgress = Int32.MaxValue;
            _model.CurrentProgress = 0;

            ServiceLocator.ToastService.Toast($"{updated} files were updated.", "Fix folders");

            return updated;
        }

        private async Task MoveFiles(ICollection<ImageEntry> images, string targetPath, bool remove)
        {

            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
            }

            Dispatcher.Invoke(() =>
            {
                _model.TotalProgress = images.Count;
                _model.CurrentProgress = 0;
            });

            var moved = 0;

            var folderIdCache = new Dictionary<string, int>();

            foreach (var image in images)
            {

                string newPath = "";
                string newTxtPath = "";
                string fileName = "";
                string fileNameOnly = "";
                string extension = "";
                int increment = 0;

                var directoryName = Path.GetDirectoryName(image.Path);


                string originalFileNameOnly = Path.GetFileNameWithoutExtension(image.Path);

                fileName = Path.GetFileName(image.Path);
                extension = Path.GetExtension(image.Path);

                var txtFileName = $"{originalFileNameOnly}.txt";
                var txtPath = Path.Join(directoryName, txtFileName);

                newPath = Path.Join(targetPath, fileName);
                newTxtPath = Path.Join(targetPath, txtFileName);

                // append number if file exists at target 
                while (File.Exists(newPath))
                {
                    increment++;
                    fileNameOnly = $"{originalFileNameOnly} ({increment})";
                    fileName = $"{fileNameOnly}{extension}";
                    txtFileName = $"{fileNameOnly}.txt";

                    newPath = Path.Join(targetPath, fileName);
                    newTxtPath = Path.Join(targetPath, txtFileName);
                }
                ;


                if (image.Path != newPath)
                {
                    File.Move(image.Path, newPath);

                    if (File.Exists(txtPath))
                    {
                        File.Move(txtPath, newTxtPath);
                    }

                    if (remove)
                    {
                        _dataStore.DeleteImage(image.Id);
                    }
                    else
                    {
                        _dataStore.MoveImage(image.Id, newPath, folderIdCache);
                    }

                    var moved1 = moved;
                    if (moved % 113 == 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            image.Path = newPath;
                            _model.CurrentProgress = moved1;
                            _model.Status = $"Moving {_model.CurrentProgress:#,###,###} of {_model.TotalProgress:#,###,###}...";
                        });
                    }

                    moved++;
                }
                else
                {
                    _model.TotalProgress--;
                }
            }


            _model.Status = $"Moving {_model.TotalProgress:#,###,###} of {_model.TotalProgress:#,###,###}...";
            _model.TotalProgress = Int32.MaxValue;
            _model.CurrentProgress = 0;
            ServiceLocator.ToastService.Toast($"{moved} files were moved.", "Move images");

            //await _search.ReloadMatches();

            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = true;
            }


        }
        
        private async void CleanExcludedPaths()
        {
            var message = "This will remove any remaining images in excluded folders from the database. The images on disk will not be deleted.\r\n\r\n" +
                          "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Remove Excluded Images", PopupButtons.YesNo, 500, 250);
            if (result == PopupResult.Yes)
            {
                var total = CleanExcludedPaths(_settings.ExcludePaths);

                ServiceLocator.ToastService.Toast($"{total} images removed from database", "");
            }
        }

        private int CleanExcludedPaths(IEnumerable<string> excludedPaths)
        {
            int total = 0;

            foreach (var excludedPath in excludedPaths)
            {
                var folder = _dataStore.GetFolder(excludedPath);
                if (folder != null)
                {
                    var images = _dataStore.GetFolderImages(folder.Id).ToList();
                    if (images.Any())
                    {
                        _dataStore.RemoveImages(images.Select(i => i.Id));
                    }
                    total += images.Count();
                }
            }

            return total;
        }

        private async Task CleanRemovedFolders(object o)
        {
            var message = "This will remove any remaining images in removed folders from the database. The images on disk will not be deleted.\r\n\r\n" +
                          "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Clean Removed Folders", PopupButtons.YesNo, 500, 250);
            if (result == PopupResult.Yes)
            {
                CleanRemovedFoldersInternal();
            }
        }

        private void CleanRemovedFoldersInternal()
        {
            var total = _dataStore.CleanRemovedFolders(_settings.ImagePaths);

            if (total > 0)
            {
                _search.ReloadMatches(null);

                ServiceLocator.ToastService.Toast($"{total} images removed from database", "");
            }
        }

        private void SortAlbums()
        {
            var window = new AlbumSortWindow(_dataStore, _settings);
            window.Owner = this;
            window.ShowDialog();
            LoadAlbums();
        }

        private void ClearAlbums()
        {
            foreach (var album in _model.Albums)
            {
                album.IsTicked = false;
            }

            _model.HasSelectedAlbums = false;
        }

        private void ClearModels()
        {
            foreach (var model in _model.ImageModels)
            {
                model.IsTicked = false;
            }

            _model.HasSelectedModels = false;
        }

        public async Task CancelProgress()
        {
            await ServiceLocator.ProgressService.CancelTask();
        }
    }


}
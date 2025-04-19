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
            if (ServiceLocator.FolderService.HasRootFolders)
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
            if (ServiceLocator.FolderService.HasRootFolders)
            {
                var message = GetLocalizedText("Menu.Tools.RebuildMetadata.Message");

                var result = await _messagePopupManager.ShowMedium(message, GetLocalizedText("Menu.Tools.RebuildMetadata.Title"), PopupButtons.YesNo);
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

        private Task Scan()
        {
            return Task.Run(async () =>
            {
                if (await ServiceLocator.ProgressService.TryStartTask())
                {
                    await ServiceLocator.ScanningService.ScanWatchedFolders(false, true, ServiceLocator.ProgressService.CancellationToken);

                    //try
                    //{
                    //    if (isFirstTime)
                    //    {
                    //        _ = Task.Delay(10000).ContinueWith(t =>
                    //        {
                    //            ServiceLocator.SearchService.ExecuteSearch();
                    //            ServiceLocator.MessageService.Show(GetLocalizedText("FirstScan.Message"), GetLocalizedText("FirstScan.Title"), PopupButtons.OK);
                    //        });
                    //    }

                    //    await ServiceLocator.ScanningService.ScanWatchedFolders(false, true, ServiceLocator.ProgressService.CancellationToken);
                    //}
                    //finally
                    //{
                    //    ServiceLocator.ProgressService.CompleteTask();
                    //    ServiceLocator.ProgressService.SetStatus(GetLocalizedText("Actions.Scanning.Completed"));
                    //}
                }

            });
        }

        private async Task Rebuild()
        {
            await Task.Run(async () =>
            {
                if (await ServiceLocator.ProgressService.TryStartTask())
                {
                    await ServiceLocator.ScanningService.ScanWatchedFolders(true, true, ServiceLocator.ProgressService.CancellationToken);
                    //try
                    //{
                    //    await ServiceLocator.ScanningService.ScanWatchedFolders(true, true, ServiceLocator.ProgressService.CancellationToken);
                    //}
                    //finally
                    //{
                    //    ServiceLocator.ProgressService.CompleteTask();
                    //    ServiceLocator.ProgressService.SetStatus(GetLocalizedText("Actions.Scanning.Completed"));
                    //}
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

            var folderCache = folders.ToDictionary(f => f.Path);

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

                if (folderCache.TryGetValue(dirName, out var folder))
                {
                    if (image.FolderId == folder.Id)
                    {
                        skip = true;
                    }
                }

                if (!skip)
                {
                    _dataStore.UpdateImageFolderId(image.Id, image.Path, folderCache);
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

        

        // TODO: When will this be called?
        private async void CleanExcludedPaths()
        {
            var message = "This will remove any remaining images in excluded folders from the database. The images on disk will not be deleted.\r\n\r\n" +
                          "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Remove Excluded Images", PopupButtons.YesNo, 500, 250);
            if (result == PopupResult.Yes)
            {
                var total = CleanExcludedPaths(ServiceLocator.FolderService.ExcludedFolders.Select(d => d.Path));

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
                    var images = _dataStore.GetFolderImages(folder.Id, false).ToList();
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
                await CleanRemovedFoldersInternal();
            }
        }

        private async Task CleanRemovedFoldersInternal()
        {
            await Task.Run(() =>
            {
                var total = _dataStore.CleanRemovedFolders();

                if (total > 0)
                {
                    ServiceLocator.SearchService.RefreshResults();
                    ServiceLocator.ToastService.Toast($"{total} images removed from removed folders", "");
                }
            });
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
            ServiceLocator.MainModel.SelectedAlbumsCount = 0;
            ServiceLocator.MainModel.HasSelectedAlbums = false;

            ServiceLocator.SearchService.ExecuteSearch();
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
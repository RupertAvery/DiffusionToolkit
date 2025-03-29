using Diffusion.Toolkit.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Diffusion.Database;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private async Task<bool> CheckIfQueryEmpty(string title)
        {
            if (_search.IsQueryEmpty())
            {
                await _messagePopupManager.Show("Query cannot be empty", title, PopupButtons.OK);
                return false;
            }

            return true;
        }

        private async void RescanResults()
        {
            if (!await CheckIfQueryEmpty("Rescan results"))
            {
                return;
            }

            await Task.Run(async () =>
            {
                if (await ServiceLocator.ProgressService.TryStartTask())
                {
                    var paths = GetSearchResults().Select(m => m.Path).ToList();

                    try
                    {
                        ServiceLocator.ScanningService.ScanFiles(paths, true, ServiceLocator.Settings.StoreMetadata, ServiceLocator.Settings.StoreWorkflow, ServiceLocator.ProgressService.CancellationToken);
                        ServiceLocator.SearchService.ExecuteSearch();
                    }
                    finally
                    {
                        ServiceLocator.ProgressService.CompleteTask();
                        var status = GetLocalizedText("Actions.Scanning.Completed");
                        ServiceLocator.ProgressService.SetStatus(status);
                    }
                }
            });
        }

        private IEnumerable<ImageView> GetSearchResults()
        {
            return _search.UseFilter ? _dataStore.Search(_search.Filter, _search.QueryOptions, _search.Sorting) : _dataStore.Search(_search.QueryOptions, _search.Sorting);
        }

        private async void RemoveFromDatabase()
        {
            if (!await CheckIfQueryEmpty("Remove images from Database"))
            {
                return;
            }

            var message = "This will remove all matching images from the database. No files will be deleted.\r\n\r\n" +
                          "You should use this if you previously removed a folder and you want to remove the images associated with the folder\r\n\r\n" +
                          "Are you sure you want to continue?";

            var result = await ServiceLocator.MessageService.ShowCustom(message, "Remove images from Database", PopupButtons.YesNo, 500, 400);

            if (result == PopupResult.Yes)
            {
                var ids = GetSearchResults().Select(m => m.Id).ToList();

                _dataStore.RemoveImages(ids);

                message = $"{ids.Count} images were removed";

                await ServiceLocator.MessageService.ShowMedium(message, "Remove images from Database", PopupButtons.OK);

                ServiceLocator.SearchService.RefreshResults();

                //await _search.ReloadMatches();
            }
        }

        private async void MarkAllForDeletion()
        {
            if (!await CheckIfQueryEmpty("Mark images for deletion"))
            {
                return;
            }

            var message = "This will mark all matching images for deletion.\r\n\r\n" + "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowMedium(message, "Mark images for deletion", PopupButtons.YesNo);

            if (result == PopupResult.Yes)
            {
                var ids = GetSearchResults().Select(m => m.Id).ToList();

                _dataStore.SetDeleted(ids, true);

                _search.ReloadMatches(null);

                //await _search.ReloadMatches();
            }
        }

        private async void UnmarkAllForDeletion()
        {
            if (!await CheckIfQueryEmpty("Unmark images for deletion"))
            {
                return;
            }

            var message = "This will unmark all matching images for deletion.\r\n\r\n" + "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowMedium(message, "Unmark images for deletion", PopupButtons.YesNo);

            if (result == PopupResult.Yes)
            {
                var ids = GetSearchResults().Select(m => m.Id).ToList();

                _dataStore.SetDeleted(ids, false);

                _search.ReloadMatches(null);

                //await _search.ReloadMatches();
            }
        }

        private async void AutoTagNSFW()
        {
            if (!await CheckIfQueryEmpty("Auto Tag NSFW"))
            {
                return;
            }


            var message = "This will tag ALL images in the database that contain the NSFW Tags in Settings as NSFW.\r\n\r\n" + "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowMedium(message, "Auto Tag NSFW", PopupButtons.YesNo);

            if (result == PopupResult.Yes)
            {
                var matches = _dataStore.QueryAll();

                var ids = matches.Where(m => _settings.NSFWTags.Any(t => m.Prompt != null && m.Prompt.ToLower().Contains(t.Trim().ToLower()))).Select(m => m.Id).ToList();

                _dataStore.SetNSFW(ids, true, true);

                message = $"{ids.Count} images were tagged as NSFW";

                await _messagePopupManager.ShowMedium(message, "Auto Tag NSFW", PopupButtons.OK);

                _search.ReloadMatches(null);

                //await _search.ReloadMatches();
            }

        }


        private void UpdateByBatch(ICollection<int> ids, int size, Action<int[]> updateAction)
        {
            int processed = 0;
            var oldStatus = _model.Status;

            Dispatcher.Invoke(() =>
            {
                _model.TotalProgress = ids.Count;
                _model.CurrentProgress = processed;
                _model.Status = $"Updating {_model.CurrentProgress:n0} of {_model.TotalProgress:n0}...";
            });

            foreach (var chunk in ids.Chunk(size))
            {
                updateAction(chunk);
                processed += chunk.Length;
                Dispatcher.Invoke(() =>
                {
                    _model.CurrentProgress = processed;
                    _model.Status = $"Updating {_model.CurrentProgress:n0} of {_model.TotalProgress:n0}...";
                });
            }

            Dispatcher.Invoke(() =>
            {
                _model.TotalProgress = 999;
                _model.CurrentProgress = 0;
                _model.Status = oldStatus;
            });
        }

        private async void SaveQuery()
        {
            if (!await CheckIfQueryEmpty("Save Query/Filter"))
            {
                return;
            }
        }

        private async void AddAllToAlbum()
        {
            if (!await CheckIfQueryEmpty("Add images to album"))
            {
                return;
            }

            var selector = new AlbumListWindow(_dataStore)
            {
                Owner = this
            };

            var result = selector.ShowDialog();


            if (result.HasValue && result.Value)
            {
                var ids = GetSearchResults().Select(m => m.Id).ToList();

                string albumName = "";

                if (selector.IsNewAlbum)
                {
                    var album = _dataStore.CreateAlbum(new Album() { Name = selector.AlbumName });
                    albumName = selector.AlbumName;

                    await Task.Run(() =>
                    {
                        _dataStore.AddImagesToAlbum(album.Id, ids);
                    });

                }
                else
                {
                    var albumId = selector.SelectedAlbum.Id;
                    albumName = selector.SelectedAlbum.Name;

                    await Task.Run(() =>
                    {
                        _dataStore.AddImagesToAlbum(albumId, ids);
                    });

                }

                _search.ReloadMatches(null);

                ServiceLocator.ToastService.Toast($"{ids.Count} images added to album {albumName}.", "Add to Album");
            }
        }
    }
}
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
        private async void RescanResults()
        {
            if (_search.IsQueryEmpty())
            {
                await _messagePopupManager.Show("Query cannot be empty", "Rescan results", PopupButtons.OK);
                return;
            }

            await Task.Run(() =>
            {
                var paths = GetSearchResults().Select(m => m.Path).ToList();

                try
                {
                    ServiceLocator.ScanningService.Scan(paths, true);
                    ServiceLocator.SearchService.ExecuteSearch();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            });
        }

        private IEnumerable<ImageView> GetSearchResults()
        {
            return _search.UseFilter ? _dataStore.Search(_search.Filter, _search.QueryOptions, _search.Sorting) : _dataStore.Search(_search.QueryOptions, _search.Sorting);
        }

        private async void RemoveFromDatabase()
        {
            if (_search.IsQueryEmpty())
            {
                await _messagePopupManager.Show("Query cannot be empty", "Remove images from Database", PopupButtons.OK);
                return;
            }

            var message = "This will remove all matching images from the database. No files will be deleted.\r\n\r\n" +
                          "You should use this if you previously removed a folder and you want to remove the images associated with the folder\r\n\r\n" +
                          "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Remove images from Database", PopupButtons.YesNo, 500, 400);

            if (result == PopupResult.Yes)
            {
                var ids = GetSearchResults().Select(m => m.Id).ToList();

                await Task.Run(() =>
                {
                    UpdateByBatch(ids, 100, subset => _dataStore.RemoveImages(subset));
                });

                message = $"{ids.Count} images were removed";

                await _messagePopupManager.ShowMedium(message, "Remove images from Database", PopupButtons.OK);

                _search.ReloadMatches(null);

                //await _search.ReloadMatches();
            }
        }

        private async void MarkAllForDeletion()
        {

            if (_search.IsQueryEmpty())
            {
                await _messagePopupManager.Show("Query cannot be empty", "Mark images for deletion", PopupButtons.OK);
                return;
            }

            var message = "This will mark all matching images for deletion.\r\n\r\n" + "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowMedium(message, "Mark images for deletion", PopupButtons.YesNo);

            if (result == PopupResult.Yes)
            {
                var ids = GetSearchResults().Select(m => m.Id).ToList();

                await Task.Run(() =>
                {
                    UpdateByBatch(ids, 50, subset => _dataStore.SetDeleted(subset, true));
                });

                _search.ReloadMatches(null);

                //await _search.ReloadMatches();
            }
        }

        private async void UnmarkAllForDeletion()
        {
            if (_search.IsQueryEmpty())
            {
                await _messagePopupManager.Show("Query cannot be empty", "Unmark images for deletion", PopupButtons.OK);
                return;
            }

            var message = "This will unmark all matching images for deletion.\r\n\r\n" + "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowMedium(message, "Unmark images for deletion", PopupButtons.YesNo);

            if (result == PopupResult.Yes)
            {
                var ids = GetSearchResults().Select(m => m.Id).ToList();

                await Task.Run(() =>
                {
                    UpdateByBatch(ids, 50, subset => _dataStore.SetDeleted(subset, false));
                });

                _search.ReloadMatches(null);

                //await _search.ReloadMatches();
            }
        }

        private async void AutoTagNSFW()
        {
            var message = "This will tag ALL images in the database that contain the NSFW Tags in Settings as NSFW.\r\n\r\n" + "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowMedium(message, "Auto Tag NSFW", PopupButtons.YesNo);

            if (result == PopupResult.Yes)
            {
                var matches = _dataStore.QueryAll();

                var ids = matches.Where(m => _settings.NSFWTags.Any(t => m.Prompt != null && m.Prompt.ToLower().Contains(t.Trim().ToLower()))).Select(m => m.Id).ToList();

                await Task.Run(() =>
                {
                    UpdateByBatch(ids, 50, subset => _dataStore.SetNSFW(subset, true, true));
                });

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
            if (_search.IsQueryEmpty())
            {
                await _messagePopupManager.Show("Query cannot be empty", "Add images to album", PopupButtons.OK);
                return;
            }
        }

        private async void AddAllToAlbum()
        {
            if (_search.IsQueryEmpty())
            {
                await _messagePopupManager.Show("Query cannot be empty", "Add images to album", PopupButtons.OK);
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

                Toast($"{ids.Count} images added to album {albumName}.", "Add to Album");
            }

            //_search.ReloadMatches();

            //await _search.ReloadMatches();
        }
    }
}
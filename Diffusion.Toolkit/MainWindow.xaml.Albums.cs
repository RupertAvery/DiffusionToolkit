using Diffusion.Database;
using System.Windows.Controls;
using System.Windows;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using Diffusion.Database.Models;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void InitAlbums()
        {
            _model.AddSelectedImagesToAlbum = AddSelectedImagesToAlbum;


            _model.CreateAlbumCommand = new AsyncCommand<object>(async (o) =>
            {
                var title = GetLocalizedText("Actions.Albums.Create.Title");

                var (result, name) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Albums.Create.Message"), title);

                name = name.Trim();

                if (result == PopupResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Albums.CannotBeEmpty.Message"), title, PopupButtons.OK);
                        return;
                    }

                    await CreateAlbum(name);
                }
            });

            _model.AddAlbumCommand = new AsyncCommand<object>(async (o) =>
            {
                var title = GetLocalizedText("Actions.Albums.Create.Title");

                var (result, name) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Albums.Create.Message"), title);

                name = name.Trim();

                if (result == PopupResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Albums.CannotBeEmpty.Message"), title, PopupButtons.OK);
                        return;
                    }

                    await CreateAlbum(name, _model.SelectedImages);
                }
            });

            _model.AddToAlbumCommand = new RelayCommand<object>((o) =>
            {
                var album = (Album)((MenuItem)o).Tag;
                var images = _model.SelectedImages.Select(x => x.Id).ToList();

                if (_dataStore.AddImagesToAlbum(album.Id, images))
                {
                    ServiceLocator.ToastService.Toast($"{images.Count} image{(images.Count == 1 ? "" : "s")} added to \"{album.Name} \".", "Add to Album");
                    LoadAlbums();
                    _search.ReloadMatches(null);
                }
                else
                    MessageBox.Show("Album not found, please refresh and try again", "No Album");
            });
            
            _model.RenameAlbumCommand = new AsyncCommand<AlbumModel>(async (album) =>
            {
                var title = GetLocalizedText("Actions.Albums.Rename.Title");

                var (result, name) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Albums.Rename.Message"), title, album.Name);

                name = name.Trim();

                if (result == PopupResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Albums.CannotBeEmpty.Message"), title, PopupButtons.OK);
                        return;
                    }

                    _dataStore.RenameAlbum(album.Id, name);
                    //UpdateAlbums();
                    //SearchImages(null);
                    LoadAlbums();
                }
            });

            _model.RemoveFromAlbumCommand = new RelayCommand<object>((o) =>
            {
                var album = ((MenuItem)o).Tag as Album;
                var images = _model.SelectedImages.Select(x => x.Id).ToList();
                var count = _dataStore.RemoveImagesFromAlbum(album.Id, images);

                var message = GetLocalizedText("Actions.Albums.RemoveImages.Toast")
                    .Replace("{images}", $"{count}")
                    .Replace("{album}", $"{album.Name}");

                ServiceLocator.ToastService.Toast(message, "");

                LoadAlbums();
                _search.ReloadMatches(null);
            });

            _model.RemoveAlbumCommand = new AsyncCommand<AlbumModel>(async (album) =>
            {
                var title = GetLocalizedText("Actions.Albums.Remove.Title");

                var result = await _messagePopupManager.Show(GetLocalizedText("Actions.Albums.Remove.Message").Replace("{album}", album.Name), title, PopupButtons.YesNo);

                if (result == PopupResult.Yes)
                {
                    _dataStore.RemoveAlbum(album.Id);

                    if (_search.QueryOptions.AlbumIds is { Count: > 0 })
                    {
                        if (_search.QueryOptions.AlbumIds.Contains(album.Id))
                        {
                            _search.QueryOptions.AlbumIds = _search.QueryOptions.AlbumIds.Except(new[] { album.Id }).ToList();
                        }
                    }

                    LoadAlbums();

                    _search.ReloadMatches(null);
                }
            });


        }

        private void LoadAlbums()
        {
            var currentAlbums = _model.Albums is { } ? _model.Albums.ToList() : Enumerable.Empty<AlbumModel>();

            var albums = _dataStore.GetAlbumsView().Select(a => new AlbumModel()
            {
                Id = a.Id,
                Name = a.Name,
                LastUpdated = a.LastUpdated,
                ImageCount = a.ImageCount,
                Order = a.Order,
            }).ToList();

            foreach (var album in albums)
            {
                var prevAlbum = currentAlbums.FirstOrDefault(d => d.Id == album.Id);

                if (prevAlbum != null)
                {
                    album.IsTicked = prevAlbum.IsTicked;
                }

                album.PropertyChanged += Album_PropertyChanged;
            }

            switch (_settings.SortAlbumsBy)
            {
                case "Name":
                    _model.Albums = new ObservableCollection<AlbumModel>(albums.OrderBy(a => a.Name));
                    break;
                case "Date":
                    _model.Albums = new ObservableCollection<AlbumModel>(albums.OrderBy(a => a.LastUpdated));
                    break;
                case "Custom":
                    _model.Albums = new ObservableCollection<AlbumModel>(albums.OrderBy(a => a.Order));
                    break;
            }


        }

        private void Album_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AlbumModel.IsTicked))
            {
                //var selectedAlbums = ServiceLocator.MainModel.Albums.Where(d => d.IsTicked).ToList();
                //ServiceLocator.MainModel.SelectedAlbumsCount = selectedAlbums.Count;
                //ServiceLocator.MainModel.HasSelectedAlbums = selectedAlbums.Any();

                //ServiceLocator.SearchService.ExecuteSearch();
            }
        }

        private async Task CreateAlbum(string name, IEnumerable<ImageEntry>? imageEntries = null)
        {
            var title = GetLocalizedText("Actions.Albums.Create.Title");

            try
            {
                var album = _dataStore.CreateAlbum(new Album() { Name = name });

                var images = (imageEntries ?? Enumerable.Empty<ImageEntry>()).ToList();

                if (images.Any())
                {
                    _dataStore.AddImagesToAlbum(album.Id, images.Select(i => i.Id));

                    foreach (var imageEntry in images)
                    {
                        imageEntry.AlbumCount++;
                    }
                }

                ServiceLocator.ToastService.Toast(GetLocalizedText("Actions.Albums.Created.Toast").Replace("{album}", album.Name), title);

                LoadAlbums();
            }
            catch (SQLiteException ex)
            {
                await ServiceLocator.MessageService.Show($"Album {name} already exists!\r\n Please use another name.", "New Album", PopupButtons.OK);
            }
        }

        private void AddSelectedImagesToAlbum(IAlbumInfo album)
        {
            if (_model.SelectedImages != null)
            {
                var images = _model.SelectedImages.Select(x => x.Id).ToList();
                if (_dataStore.AddImagesToAlbum(album.Id, images))
                {
                    var message = GetLocalizedText("Actions.Albums.AddImages.Toast")
                        .Replace("{images}", $"{images.Count}")
                        .Replace("{album}", $"{album.Name}");

                    ServiceLocator.ToastService.Toast(message, "");

                    LoadAlbums();

                    foreach (var image in _model.SelectedImages)
                    {
                        image.AlbumCount++;
                    }
                    //_search.ReloadMatches(null);
                }
                else
                    MessageBox.Show("Album not found, please refresh and try again", "No Album");
            }
        }

        private void MoveSelectedImagesToFolder(FolderViewModel folder)
        {
            if (_model.SelectedImages != null)
            {
                Task.Run(async () =>
                {
                    await MoveFiles(_model.SelectedImages, folder.Path, false);
                    _search.SearchImages(null);
                });
            }
        }

    }
}
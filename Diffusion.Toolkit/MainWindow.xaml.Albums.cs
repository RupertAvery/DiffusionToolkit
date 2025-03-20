using Diffusion.Database;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Editing;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void InitAlbums()
        {
            _model.AddSelectedImagesToAlbum = AddSelectedImagesToAlbum;


            _model.AddAlbumCommand = new AsyncCommand<object>(async (o) =>
            {
                var (result, text) = await _messagePopupManager.ShowInput("Enter a name for the new album", "New Album");

                if (result == PopupResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        await _messagePopupManager.Show("Album name cannot be empty.", "New Album", PopupButtons.OK);
                        return;
                    }

                    await CreateAlbum(text, _model.SelectedImages);
                }
            });

            _model.AddToAlbumCommand = new RelayCommand<object>((o) =>
            {
                var album = (Album)((MenuItem)o).Tag;
                var images = _model.SelectedImages.Select(x => x.Id).ToList();
                if (_dataStore.AddImagesToAlbum(album.Id, images))
                {
                    Toast($"{images.Count} image{(images.Count == 1 ? "" : "s")} added to \"{album.Name} \".", "Add to Album");
                    LoadAlbums();
                    _search.ReloadMatches(null);
                }
                else
                    MessageBox.Show("Album not found, please refresh and try again", "No Album");
            });

            _model.RemoveFromAlbumCommand = new RelayCommand<object>((o) =>
            {
                var album = _model.CurrentAlbum;
                var images = _model.SelectedImages.Select(x => x.Id).ToList();
                _dataStore.RemoveImagesFromAlbum(album.Id, images);
                Toast($"{images.Count} image{(images.Count == 1 ? "" : "s")} removed from \"{album.Name}\".", "Remove from Album");
                //_search.SearchImages(null);
                LoadAlbums();
                _search.ReloadMatches(null);
            });

            _model.RemoveAlbumCommand = new AsyncCommand<AlbumModel>(async (album) =>
            {
                var result = await _messagePopupManager.Show($"Are you sure you want to remove \"{album.Name}\"?", "Remove Album", PopupButtons.YesNo);

                if (result == PopupResult.Yes)
                {
                    _dataStore.RemoveAlbum(album.Id);

                    LoadAlbums();

                    _search.ReloadMatches(null);
                }
            });

            _model.RenameAlbumCommand = new AsyncCommand<AlbumModel>(async (album) =>
            {
                var (result, text) = await _messagePopupManager.ShowInput("Enter a new name for the album", "Rename Album", album.Name);

                if (result == PopupResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        await _messagePopupManager.Show("Album name cannot be empty.", "Rename Album", PopupButtons.OK);
                        return;
                    }

                    _dataStore.RenameAlbum(album.Id, text);
                    //UpdateAlbums();
                    //SearchImages(null);
                    LoadAlbums();
                }
            });

            _model.CreateAlbumCommand = new AsyncCommand<object>(async (o) =>
            {
                var (result, text) = await _messagePopupManager.ShowInput("Enter a name for the new album", "New Album");

                if (result == PopupResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        await _messagePopupManager.Show("Album name cannot be empty.", "New Album", PopupButtons.OK);
                        return;
                    }

                    await CreateAlbum(text);
                }
            });

        }

        private void LoadAlbums()
        {
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
                _search.SearchImages();
            }
        }

        private async Task CreateAlbum(string name, IEnumerable<ImageEntry>? imageEntries = null)
        {
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

                    Toast($"{images.Count} image{(images.Count == 1 ? "" : "s")} added to new album \"{album.Name}\".", "Add to Album");
                }
                else
                {
                    Toast($"Album \"{album.Name}\" created.", "Add to Album");
                }

                LoadAlbums();
            }
            catch (SQLiteException ex)
            {
                await _messagePopupManager.Show($"Album {name} already exists!\r\n Please use another name.", "New Album", PopupButtons.OK);
            }
        }
        
        private void AddSelectedImagesToAlbum(IAlbumInfo album)
        {
            if (_model.SelectedImages != null)
            {
                var images = _model.SelectedImages.Select(x => x.Id).ToList();
                if(_dataStore.AddImagesToAlbum(album.Id, images))
                {
                    Toast($"{images.Count} image{(images.Count == 1 ? "" : "s")} added to \"{album.Name}\".", "Add to Album");
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
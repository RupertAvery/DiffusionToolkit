using Diffusion.Database;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
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


            _model.AddAlbumCommand = new RelayCommand<object>((o) =>
            {
                NewAlbumName.Text = null;
                AddAlbumPopup.Tag = "AddImages";
                AddAlbumPopup.IsOpen = true;
                NewAlbumName.Focus();
            });

            _model.AddToAlbumCommand = new RelayCommand<object>((o) =>
            {
                var album = (Album)((MenuItem)o).Tag;
                var images = _model.SelectedImages.Select(x => x.Id).ToList();
                _dataStore.AddImagesToAlbum(album.Id, images);
                Toast($"{images.Count} image{(images.Count == 1 ? "" : "s")} added to \"{album.Name} \".", "Add to Album");
                LoadAlbums();
                _search.ReloadMatches(null);
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

            _model.RemoveAlbumCommand = new RelayCommand<object>((o) =>
            {
                var album = o as AlbumModel;

                _model.SelectedAlbum = new AlbumListItem()
                {
                    Id = album.Id,
                    Name = album.Name,
                };

                RemoveAlbumMessage.Text = $"Are you sure you want to remove \"{album.Name}\"?";
                RemoveAlbumPopup.IsOpen = true;
            });

            _model.RenameAlbumCommand = new RelayCommand<object>((o) =>
            {
                var album = o as AlbumModel;

                _model.SelectedAlbum = new AlbumListItem()
                {
                    Id = album.Id,
                    Name = album.Name,
                };

                RenewAlbumName.Text = album.Name;
                RenameAlbumPopup.IsOpen = true;
                RenewAlbumName.Focus();
            });

            _model.CreateAlbumCommand = new RelayCommand<object>((o) =>
            {
                CreateAlbum();
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
            });


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

        private void AddImagesToNewAlbum_Click(object sender, RoutedEventArgs e)
        {
            var name = NewAlbumName.Text.Trim();
            AddImagesToNewAlbum(name, _model.SelectedImages);
        }

        private void AddImagesToNewAlbum(string name, IEnumerable<ImageEntry>? imageEntries)
        {
            var images = (imageEntries ?? Enumerable.Empty<ImageEntry>()).ToList();

            if (string.IsNullOrWhiteSpace(name))
            {
                NewAlbumPopup.IsOpen = true;
                NewAlbumMessage.Text = "Album name cannot be empty.";
                return;
            }

            try
            {
                var album = _dataStore.CreateAlbum(new Album() { Name = name });

                if (AddAlbumPopup.Tag is string and "AddImages" && images.Any())
                {
                    _dataStore.AddImagesToAlbum(album.Id, imageEntries.Select(i => i.Id));

                    Toast($"{images.Count} image{(images.Count == 1 ? "" : "s")} added to new album \"{album.Name}\".", "Add to Album");
                }
                else
                {
                    Toast($"Album \"{album.Name}\" created.", "Add to Album");
                }

                LoadAlbums();

                foreach (var imageEntry in imageEntries)
                {
                    imageEntry.AlbumCount++;
                }
                //_search.ReloadMatches(null);
                AddAlbumPopup.Tag = null;
                //UpdateAlbums();
            }
            catch (SQLiteException ex)
            {
                NewAlbumPopup.IsOpen = true;
                NewAlbumMessage.Text = $"Album {name} already exists! \r\n Please use another name.";
            }

            AddAlbumPopup.IsOpen = false;
        }

        private void CancelNewAlbum_Click(object sender, RoutedEventArgs e)
        {
            AddAlbumPopup.IsOpen = false;
        }

        private void NewAlbumMessageOK_Click(object sender, RoutedEventArgs e)
        {
            NewAlbumPopup.IsOpen = false;
        }

        private void RemoveAlbumYes_Click(object sender, RoutedEventArgs e)
        {
            if (_model.SelectedAlbum != null)
            {
                var album = _dataStore.GetAlbum(_model.SelectedAlbum.Id);
                _dataStore.RemoveAlbum(album.Id);
                RemoveAlbumPopup.IsOpen = false;

                LoadAlbums();
                _search.ReloadMatches(null);
                //SearchImages(null);
                //UpdateAlbums();
            }
        }

        private void RemoveAlbumNo_Click(object sender, RoutedEventArgs e)
        {
            RemoveAlbumPopup.IsOpen = false;
        }

        private void RenameAlbumOK_Click(object sender, RoutedEventArgs e)
        {
            RenameAlbum();
        }

        private void RenameAlbum()
        {
            var name = RenewAlbumName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                NewAlbumPopup.IsOpen = true;
                NewAlbumMessage.Text = "Album name cannot be empty.";
                return;
            }

            try
            {
                if (_model.SelectedAlbum != null)
                {
                    _dataStore.RenameAlbum(_model.SelectedAlbum.Id, name);
                    //UpdateAlbums();
                    //SearchImages(null);
                    LoadAlbums();
                }
            }
            catch (SQLiteException ex)
            {
                NewAlbumPopup.IsOpen = true;
                NewAlbumMessage.Text = $"Album {name} already exists! \r\n Please use another name.";
            }

            RenameAlbumPopup.IsOpen = false;
        }
        private void RenameAlbumCancel_Click(object sender, RoutedEventArgs e)
        {
            RenameAlbumPopup.IsOpen = false;
        }

        private void RenewAlbumName_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    RenameAlbum();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    RenameAlbumPopup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }



        private void NewAlbumName_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    var name = NewAlbumName.Text.Trim();
                    AddImagesToNewAlbum(name, _model.SelectedImages);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    AddAlbumPopup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void AddSelectedImagesToAlbum(IAlbumInfo album)
        {
            if (_model.SelectedImages != null)
            {
                var images = _model.SelectedImages.Select(x => x.Id).ToList();
                _dataStore.AddImagesToAlbum(album.Id, images);
                Toast($"{images.Count} image{(images.Count == 1 ? "" : "s")} added to \"{album.Name}\".", "Add to Album");
                LoadAlbums();
                foreach (var image in _model.SelectedImages)
                {
                    image.AlbumCount++;
                }
                //_search.ReloadMatches(null);
            }
        }



        private void RenameAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            //_selectedAlbum = (Album)((MenuItem)sender).DataContext;

            if (_model.SelectedAlbum != null)
            {
                RenewAlbumName.Text = _model.SelectedAlbum.Name;
                RenewAlbumName.SelectAll();
                RenameAlbumPopup.IsOpen = true;
                RenewAlbumName.Focus();
            }

        }

        private void RemoveAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            //_selectedAlbum = (Album)((MenuItem)sender).DataContext;


            if (_model.SelectedAlbum != null)
            {
                RemoveAlbumMessage.Text = $"Are you sure you want to remove the Album \"{_model.SelectedAlbum.Name}\"?";
                RemoveAlbumPopup.IsOpen = true;
            }
        }

        private void CreateAlbum()
        {
            NewAlbumName.Text = null;
            AddAlbumPopup.Tag = "NoImages";
            AddAlbumPopup.IsOpen = true;
            NewAlbumName.Focus();
        }

    }
}
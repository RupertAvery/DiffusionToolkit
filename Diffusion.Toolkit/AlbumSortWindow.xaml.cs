using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Behaviors;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Configuration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for AlbumListWindow.xaml
    /// </summary>
    public partial class AlbumSortWindow : BorderlessWindow
    {
        private readonly AlbumSortModel _model;
        private readonly DataStore _dataStore;
        private readonly Settings _settings;
        private bool _isDirty;

        public AlbumSortWindow(DataStore dataStore, Settings settings)
        {
            _dataStore = dataStore;
            _settings = settings;
            _model = new AlbumSortModel();

            InitializeComponent();

            _model.SortAlbumsBy = _settings.SortAlbumsBy ?? "Date";
            _model.Escape = new RelayCommand<object>(o => Escape());
            _model.Albums = new ObservableCollection<Album>(dataStore.GetAlbums());
            _model.MoveUpCommand = new RelayCommand<object>(o => MoveUp());
            _model.MoveDownCommand = new RelayCommand<object>(o => MoveDown());
            _model.PropertyChanged += ModelOnPropertyChanged;

            FixOrder();

            DataContext = _model;

            Closing += OnClosing;


            UpdateSortedAlbums();
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (_isDirty&& _model.SortAlbumsBy == "Custom")
            {
                _dataStore.UpdateAlbumsOrder(_model.Albums);
            }
        }

        private void UpdateSortedAlbums()
        {
            switch (_model.SortAlbumsBy)
            {
                case "Name":
                    _model.SelectedAlbum = null;
                    _model.SortedAlbums = new ObservableCollection<Album>(_model.Albums.OrderBy(a => a.Name));
                    break;
                case "Date":
                    _model.SelectedAlbum = null;
                    _model.SortedAlbums = new ObservableCollection<Album>(_model.Albums.OrderBy(a => a.LastUpdated));
                    break;
                case "Custom":
                    _model.SortedAlbums = new ObservableCollection<Album>(_model.Albums.OrderBy(a => a.Order));
                    break;
            }

            _settings.SortAlbumsBy = _model.SortAlbumsBy;
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AlbumSortModel.SortAlbumsBy))
            {
                UpdateSortedAlbums();
            }
        }

        private void FixOrder()
        {
            if (_model.Albums.All(a => a.Order == 0))
            {
                var order = 1;
                foreach (var album in _model.Albums)
                {
                    album.Order = order;
                    order++;
                }
            }
        }

        private void Reorder()
        {
            var order = 1;

            foreach (var album in _model.Albums.OrderBy(a => a.Order))
            {
                if (album != _model.SelectedAlbum)
                {
                    if (order == newPosition)
                    {
                        album.Order = oldPosition;
                    }
                    else
                    {
                        album.Order = order;
                    }
                }
                order++;
            }

            UpdateSortedAlbums();
        }

        private int newPosition;
        private int oldPosition;

        private void MoveUp()
        {
            if (_model.SelectedAlbum == null) return;
            oldPosition = _model.SelectedAlbum.Order;
            if (oldPosition == 1) return;
            newPosition = _model.SelectedAlbum.Order - 1;
            _model.SelectedAlbum.Order = newPosition;
            _isDirty = true;
            Reorder();
        }

        private void MoveDown()
        {
            if (_model.SelectedAlbum == null) return;
            oldPosition = _model.SelectedAlbum.Order;
            if (oldPosition == _model.Albums.Count) return;
            newPosition = _model.SelectedAlbum.Order + 1;
            _model.SelectedAlbum.Order = newPosition;
            _isDirty = true;
            Reorder();
        }

        private void Escape()
        {
            Close();
        }

        private void MoveItem(object sender, RoutedEventArgs e)
        {
            if (e is DragSortDropEventArgs dropArgs)
            {
                var sourceIndex = dropArgs.SourceIndex;
                var targetIndex = dropArgs.TargetIndex;

                if (sourceIndex == targetIndex)
                    return;

                var item = _model.SortedAlbums[sourceIndex];
                _model.SortedAlbums.RemoveAt(sourceIndex);
                _model.SortedAlbums.Insert(targetIndex, item);

                for (int i = 0; i < _model.SortedAlbums.Count; i++)
                {
                    _model.SortedAlbums[i].Order = i + 1;
                }
                _isDirty = true;

                UpdateSortedAlbums();
            }
        }
    }
}

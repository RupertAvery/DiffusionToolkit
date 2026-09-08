using System.ComponentModel;
using System.Linq;
using System.Windows;
using Diffusion.Database;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for AlbumListWindow.xaml
    /// </summary>
    public partial class AlbumListWindow : BorderlessWindow
    {
        private readonly AlbumListModel _model;
        private readonly DataStore _dataStore;

        public string AlbumName { get; private set; }

        public bool IsNewAlbum { get; private set; }

        public Album SelectedAlbum { get; private set; }


        public AlbumListWindow(DataStore dataStore)
        {
            _dataStore = dataStore;
            _model = new AlbumListModel();
            _model.PropertyChanged += ModelOnPropertyChanged;

            InitializeComponent();

            _model.Escape = new RelayCommand<object>(o => Escape());
            _model.Albums = dataStore.GetAlbums().OrderBy(d => d.Name);
            DataContext = _model;
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AlbumListModel.SelectedAlbum))
            {
                _model.IsExistingAlbum = true;
            }



            _model.CanClickOk =
                (_model.IsNewAlbum && (!string.IsNullOrEmpty(_model.AlbumName) && _model.AlbumName.Trim().Length != 0))
                || (_model.IsExistingAlbum && _model.SelectedAlbum != null);

        }

        private void Escape()
        {
            DialogResult = false;
            Close();
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {


            IsNewAlbum = _model.IsNewAlbum;
            AlbumName = _model.AlbumName;
            SelectedAlbum = _model.SelectedAlbum;

            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Diffusion.Database;
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

            InitializeComponent();

            _model.Escape = new RelayCommand<object>(o => Escape());
            _model.Albums = dataStore.GetAlbums();
            DataContext = _model;
        }

        private void Escape()
        {
            DialogResult = false;
            Close();
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            if(!_model.IsNewAlbum && !_model.IsExistingAlbum) return;

            if (_model.IsNewAlbum && (string.IsNullOrEmpty(_model.AlbumName) || _model.AlbumName.Trim().Length == 0)) return;

            if (_model.IsExistingAlbum && _model.SelectedAlbum == null) return;

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

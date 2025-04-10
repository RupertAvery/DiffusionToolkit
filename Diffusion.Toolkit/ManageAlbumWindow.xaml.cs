using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Diffusion.Database;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit
{
    public class ManageAlbumModel : BaseNotify
    {
        private Album _selectedAlbum;

        public bool SortByName { get; set; }
        public bool SortByDate { get; set; }
        public bool SortManually { get; set; }

        public Album SelectedAlbum
        {
            get => _selectedAlbum;
            set => SetField(ref _selectedAlbum, value);
        }

        public ICommand Escape { get; set; }
        public IEnumerable<Album> Albums { get; set; }
    }

    /// <summary>
    /// Interaction logic for AlbumListWindow.xaml
    /// </summary>
    public partial class ManageAlbumWindow : BorderlessWindow
    {
        private readonly ManageAlbumModel _model;
        private readonly DataStore _dataStore;


        public ManageAlbumWindow(DataStore dataStore)
        {
            _dataStore = dataStore;
            _model = new ManageAlbumModel();

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
    }
}

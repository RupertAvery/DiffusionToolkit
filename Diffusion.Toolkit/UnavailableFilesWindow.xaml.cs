using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for ManageFilesWindow.xaml
    /// </summary>
    public partial class UnavailableFilesWindow : BorderlessWindow
    {
        private readonly DataStore _dataStore;
        public UnavailableFilesModel Model { get; }

        public UnavailableFilesWindow(DataStore dataStore, Settings settings)
        {
            _dataStore = dataStore;
            InitializeComponent();

            Model = new UnavailableFilesModel();
            Model.DeleteImmediately = false;
            Model.MarkForDeletion = false;
            Model.RemoveFromUnavailableRootFolders = false;
            Model.UseRootFolders = true;

            Model.ImagePaths = new ObservableCollection<ImageFileItem>(settings.ImagePaths.Select(p => new ImageFileItem()
            {
                Path = p,
                IsUnavailable = !Directory.Exists(p)
            }));

            foreach (var item in Model.ImagePaths)
            {
                item.PropertyChanged += ItemOnPropertyChanged;
            }

            Model.PropertyChanged += ModelOnPropertyChanged; 

            DataContext = Model;
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Model.IsStartEnabled = (Model.MarkForDeletion || Model.DeleteImmediately) && (Model.ImagePaths.Any(p => p.IsSelected));
        }

        private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Model.IsStartEnabled = (Model.MarkForDeletion || Model.DeleteImmediately) && (Model.ImagePaths.Any(p => p.IsSelected));
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            if (Model.DeleteImmediately || Model.MarkForDeletion)
            {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

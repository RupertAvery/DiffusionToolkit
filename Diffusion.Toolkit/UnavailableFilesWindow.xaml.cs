using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Diffusion.Database;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for ManageFilesWindow.xaml
    /// </summary>
    public partial class UnavailableFilesWindow : BorderlessWindow
    {
        private DataStore _dataStore => ServiceLocator.DataStore;
        private Settings _settings => ServiceLocator.Settings;

        public UnavailableFilesModel Model { get; }

        public UnavailableFilesWindow()
        {
            InitializeComponent();

            Model = new UnavailableFilesModel
            {
                JustUpdate = true,
                RemoveImmediately = false,
                MarkForDeletion = false,
                ShowUnavailableRootFolders = false,
                UseRootFolders = true
            };

            LoadImagePaths(false);


            Model.PropertyChanged += ModelOnPropertyChanged;

            DataContext = Model;
        }

        private void LoadImagePaths(bool showUnavailable)
        {
            var paths = _settings.ImagePaths.Select(p => new ImageFileItem()
            {
                Path = p,
                IsUnavailable = !Directory.Exists(p)
            })
            .Where(p => showUnavailable || !p.IsUnavailable);

            Model.ImagePaths = new ObservableCollection<ImageFileItem>(paths);

            foreach (var item in Model.ImagePaths)
            {
                item.PropertyChanged += ItemOnPropertyChanged;
            }
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.ShowUnavailableRootFolders))
            {
                LoadImagePaths(Model.ShowUnavailableRootFolders);
            }

            Model.IsStartEnabled = (Model.JustUpdate || Model.MarkForDeletion || Model.RemoveImmediately) && (Model.ImagePaths.Any(p => p.IsSelected));
        }

        private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Model.IsStartEnabled = (Model.JustUpdate || Model.MarkForDeletion || Model.RemoveImmediately) && (Model.ImagePaths.Any(p => p.IsSelected));
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            if (Model.JustUpdate || Model.RemoveImmediately || Model.MarkForDeletion)
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

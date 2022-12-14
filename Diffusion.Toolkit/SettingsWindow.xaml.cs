using System;
using Diffusion.Database;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Diffusion.Toolkit.Classes;
using Diffusion.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Themes;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly DataStore _dataStore;
        private readonly SettingsModel _model;

        public SettingsWindow()
        {
            InitializeComponent();

        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        public SettingsWindow(DataStore dataStore, Settings settings) : this()
        {
            _dataStore = dataStore;

            _model = new SettingsModel();
            _model.PropertyChanged  += ModelOnPropertyChanged;
            _model.ImagePaths = new ObservableCollection<string>(settings.ImagePaths);
            _model.ModelRootPath = settings.ModelRootPath;
            _model.FileExtensions = settings.FileExtensions;
            _model.PageSize = settings.PageSize;
            _model.Theme = settings.Theme;
            _model.WatchFolders = settings.WatchFolders;
            _model.CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup;
            _model.ScanForNewImagesOnStartup = settings.ScanForNewImagesOnStartup;

            DataContext = _model;

            Closing += (sender, args) =>
            {
                settings.SetPristine();
                settings.ImagePaths = _model.ImagePaths.ToList();
                settings.ModelRootPath = _model.ModelRootPath;
                settings.FileExtensions = _model.FileExtensions;
                settings.Theme = _model.Theme;
                settings.WatchFolders = _model.WatchFolders;
                settings.CheckForUpdatesOnStartup = _model.CheckForUpdatesOnStartup;
                settings.ScanForNewImagesOnStartup = _model.ScanForNewImagesOnStartup;
            };

            var str = new System.Text.StringBuilder();
            using (var writer = new System.IO.StringWriter(str))
                System.Windows.Markup.XamlWriter.Save(TabItem.Template, writer);
            System.Diagnostics.Debug.Write(str);
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsModel.Theme))
            {
                ThemeManager.ChangeTheme(_model.Theme);
            }
        }

        private void AddFolder_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                if (_model.ImagePaths.Any(d => dialog.FileName.StartsWith(d)))
                {
                    MessageBox.Show(this,
                        "The selected folder is already on the path of one of the included folders",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
                else if (_model.ImagePaths.Any(d => d.StartsWith(dialog.FileName)))
                {
                    MessageBox.Show(this,
                        "One of the included folders is on the path of the selected folder! It is recommended that you remove it.",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                _model.ImagePaths.Add(dialog.FileName);
            }

        }

        private void RemoveFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(this,
                "Are you sure you want to remove this folder? This will prevent you from scanning new images in this folder. Images in the database will not be affected.",
                "Remove folder", MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                _model.ImagePaths.RemoveAt(_model.SelectedIndex);
            }
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BrowseModelPath_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                _model.ModelRootPath = dialog.FileName;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}

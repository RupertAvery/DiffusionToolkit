using System;
using Diffusion.Database;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Diffusion.Toolkit.Classes;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Diffusion.Toolkit.Themes;
using System.Diagnostics;
using System.IO;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingsWindow : BorderlessWindow
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
            base.OnSourceInitialized(e);
        }

        public SettingsWindow(DataStore dataStore, Settings settings) : this()
        {
            _dataStore = dataStore;

            _model = new SettingsModel();
            _model.PropertyChanged += ModelOnPropertyChanged;
            _model.ImagePaths = new ObservableCollection<string>(settings.ImagePaths);
            _model.ExcludePaths = new ObservableCollection<string>(settings.ExcludePaths);
            _model.ModelRootPath = settings.ModelRootPath;
            _model.FileExtensions = settings.FileExtensions;
            _model.PageSize = settings.PageSize;
            _model.Theme = settings.Theme;
            _model.WatchFolders = settings.WatchFolders;
            _model.AutoRefresh = settings.AutoRefresh;
            _model.CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup;
            _model.ScanForNewImagesOnStartup = settings.ScanForNewImagesOnStartup;
            _model.AutoTagNSFW = settings.AutoTagNSFW;
            _model.NSFWTags = string.Join("\r\n", settings.NSFWTags);
            _model.HashCache = settings.HashCache;
            _model.PortableMode = settings.PortableMode;
            _model.RecurseFolders = settings.RecurseFolders;

            _model.UseBuiltInViewer = settings.UseBuiltInViewer.GetValueOrDefault(true);
            _model.OpenInFullScreen = settings.OpenInFullScreen.GetValueOrDefault(true);
            _model.UseSystemDefault = settings.UseSystemDefault.GetValueOrDefault(false);
            _model.UseCustomViewer = settings.UseCustomViewer.GetValueOrDefault(false);
            _model.CustomCommandLine = settings.CustomCommandLine;
            _model.CustomCommandLineArgs = settings.CustomCommandLineArgs;

            _model.Escape = new RelayCommand<object>(o => Close());

            DataContext = _model;

            Closing += (sender, args) =>
            {
                settings.SetPristine();
                settings.ImagePaths = _model.ImagePaths.ToList();
                settings.ExcludePaths = _model.ExcludePaths.ToList();
                settings.ModelRootPath = _model.ModelRootPath;
                settings.FileExtensions = _model.FileExtensions;
                settings.Theme = _model.Theme;
                settings.PageSize = _model.PageSize;
                settings.WatchFolders = _model.WatchFolders;
                settings.AutoRefresh = _model.AutoRefresh;

                settings.CheckForUpdatesOnStartup = _model.CheckForUpdatesOnStartup;
                settings.ScanForNewImagesOnStartup = _model.ScanForNewImagesOnStartup;
                settings.AutoTagNSFW = _model.AutoTagNSFW;
                settings.NSFWTags = _model.NSFWTags.Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                settings.HashCache = _model.HashCache;
                settings.PortableMode = _model.PortableMode;
                settings.RecurseFolders = _model.RecurseFolders;

                settings.UseBuiltInViewer = _model.UseBuiltInViewer;
                settings.OpenInFullScreen = _model.OpenInFullScreen;
                settings.UseSystemDefault = _model.UseSystemDefault;
                settings.UseCustomViewer = _model.UseCustomViewer;
                settings.CustomCommandLine = _model.CustomCommandLine;
                settings.CustomCommandLineArgs = _model.CustomCommandLineArgs;
            };

            //var str = new System.Text.StringBuilder();
            //using (var writer = new System.IO.StringWriter(str))
            //    System.Windows.Markup.XamlWriter.Save(TabItem.Template, writer);
            //System.Diagnostics.Debug.Write(str);
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
                if (_model.ImagePaths.Any(d => dialog.FileName.StartsWith(d + "\\")))
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
                "Are you sure you want to remove this folder? Images and any custom metadata will be removed.",
                "Remove folder", MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                _model.ImagePaths.RemoveAt(_model.SelectedIndex);
            }
        }


        private void ExcludedAddFolder_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                if (_model.ImagePaths.All(d => !dialog.FileName.StartsWith(d)))
                {
                    MessageBox.Show(this,
                        "The selected folder must be on the path of one of the included folders",
                        "Add Excluded folder", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                _model.ExcludePaths.Add(dialog.FileName);
            }

        }

        private void ExcludedRemoveFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(this,
                "Are you sure you want to remove this folder?",
                "Remove Excluded folder", MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                _model.ExcludePaths.RemoveAt(_model.ExcludedSelectedIndex);
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

        private void Open_DB_Folder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", $"/select,\"{_dataStore.DatabasePath}\"");
        }

        private void Backup_DB(object sender, RoutedEventArgs e)
        {
            _dataStore.CreateBackup();

            var result = MessageBox.Show(this,
                "A database backup has been created.",
                "Backup Database", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Restore_DB(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("SQLite databases", ".db"));
            dialog.DefaultDirectory = Path.GetDirectoryName(_dataStore.DatabasePath);
            dialog.Filters.Add(new CommonFileDialogFilter("All files", ".*"));
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                if (dialog.FileName == _dataStore.DatabasePath)
                {
                    MessageBox.Show(this,
                    "The selected file is the current database. Please try another file.",
                    "Restore Database", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);

                    return;
                }

                var result = MessageBox.Show(this,
                    $"Are you sure you want to restore the file {dialog.FileName}? Your current database will be overwritten!",
                    "Restore Database", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation, MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    if (!_dataStore.TryRestoreBackup(dialog.FileName))
                    {
                        MessageBox.Show(this,
                            "The database backup is not a Diffusion Toolkit database.",
                            "Restore Database", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show(this,
                    "The database backup has been restored.",
                    "Restore Database", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void BrowseHashCache_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("A1111 cache", "*.json"));
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                _model.HashCache = dialog.FileName;
            }
        }

        private void BrowseCustomViewer_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("Executable files", "*.exe;*.bat;*.cmd"));
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                _model.CustomCommandLine = dialog.FileName;
            }
        }
    }
}

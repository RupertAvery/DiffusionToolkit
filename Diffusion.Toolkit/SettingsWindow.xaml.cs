using System;
using System.Collections.Generic;
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
using System.Text.Json;
using Diffusion.Common;
using WPFLocalizeExtension.Engine;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Localization;
using System.Globalization;
using static Dapper.SqlMapper;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingsWindow : BorderlessWindow
    {
        private DataStore _dataStore;
        private readonly Settings _settings;
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
            _settings = settings;

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
            _model.SlideShowDelay = settings.SlideShowDelay;
            _model.ScrollNavigation = settings.ScrollNavigation;
            _model.AdvanceOnDelete = settings.AdvanceOnDelete;

            _model.Culture = settings.Culture;

            _model.Escape = new RelayCommand<object>(o => Close());

            var cultures = new List<Langauge>
            {
                new ("Default", "default"),
            };

            _model.ThemeOptions = new List<OptionValue>()
            {
                new (GetLocalizedText("Settings.Themes.Theme.System"), "System"),
                new (GetLocalizedText("Settings.Themes.Theme.Light"), "Light"),
                new (GetLocalizedText("Settings.Themes.Theme.Dark"), "Dark")
            };

            try
            {
                var configPath = Path.Combine(AppInfo.AppDir, "Localization", "languages.json");

                var langs = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(configPath));

                foreach (var (name, culture) in langs)
                {
                    cultures.Add(new Langauge(name, culture));
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading languages.json: {ex.Message}");
            }

            _model.Cultures = new ObservableCollection<Langauge>(cultures);

            DataContext = _model;

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
            if (e.PropertyName == nameof(SettingsModel.SlideShowDelay))
            {
                if (_model.SlideShowDelay < 1)
                {
                    _model.SlideShowDelay = 1;
                }
                if (_model.SlideShowDelay > 100)
                {
                    _model.SlideShowDelay = 100;
                }
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
                "Are you sure you want to remove this folder? Images and any custom metadata in the database will be removed. Images on disk will not be affected",
                "Remove folder", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation, MessageBoxResult.No);

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

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            Close();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ApplySettings()
        {
            _settings.SetPristine();
            _settings.ImagePaths = _model.ImagePaths.ToList();
            _settings.ExcludePaths = _model.ExcludePaths.ToList();
            _settings.ModelRootPath = _model.ModelRootPath;
            _settings.FileExtensions = _model.FileExtensions;
            _settings.Theme = _model.Theme;
            _settings.PageSize = _model.PageSize;
            _settings.WatchFolders = _model.WatchFolders;
            _settings.AutoRefresh = _model.AutoRefresh;

            _settings.CheckForUpdatesOnStartup = _model.CheckForUpdatesOnStartup;
            _settings.ScanForNewImagesOnStartup = _model.ScanForNewImagesOnStartup;
            _settings.AutoTagNSFW = _model.AutoTagNSFW;
            _settings.NSFWTags = _model.NSFWTags.Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            _settings.HashCache = _model.HashCache;
            _settings.PortableMode = _model.PortableMode;
            _settings.RecurseFolders = _model.RecurseFolders;

            _settings.UseBuiltInViewer = _model.UseBuiltInViewer;
            _settings.OpenInFullScreen = _model.OpenInFullScreen;
            _settings.UseSystemDefault = _model.UseSystemDefault;
            _settings.UseCustomViewer = _model.UseCustomViewer;
            _settings.CustomCommandLine = _model.CustomCommandLine;
            _settings.CustomCommandLineArgs = _model.CustomCommandLineArgs;
            _settings.SlideShowDelay = _model.SlideShowDelay;
            _settings.ScrollNavigation = _model.ScrollNavigation;
            _settings.AdvanceOnDelete = _model.AdvanceOnDelete;

            _settings.Culture = _model.Culture;
        }

        private string GetLocalizedText(string key)
        {
            return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
        }

    }
}

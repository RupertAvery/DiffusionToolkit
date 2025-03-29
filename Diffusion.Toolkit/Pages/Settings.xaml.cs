using Diffusion.Common;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Themes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Database;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Services;
using Diffusion.Civitai.Models;

namespace Diffusion.Toolkit.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        private SettingsModel _model = new SettingsModel();

        private DataStore _dataStore => ServiceLocator.DataStore;

        private string GetLocalizedText(string key)
        {
            return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
        }

        public Settings(Window window, NavigatorService navigatorService, Toolkit.Settings settings)
        {
            _window = window;
            InitializeComponent();

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
            _model.AdvanceOnTag = settings.AutoAdvance;

            _model.StoreMetadata = settings.StoreMetadata;
            _model.StoreWorkflow = settings.StoreWorkflow;
            _model.ScanUnavailable = settings.ScanUnavailable;
            _model.ExternalApplications = new ObservableCollection<ExternalApplicationModel>(settings.ExternalApplications.Select(d => new ExternalApplicationModel()
            {
                CommandLineArgs = d.CommandLineArgs,
                Name = d.Name,
                Path = d.Path
            }));

            _model.Culture = settings.Culture;

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

            _model.SetPristine();

            navigatorService.OnNavigate += NavigatorServiceOnOnNavigate;

            DataContext = _model;
        }

        private void NavigatorServiceOnOnNavigate(object? sender, NavigateEventArgs e)
        {
            if (e.CurrentUrl == "settings")
            {
                ApplySettings();
            }
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

        private Window _window;

        private void AddFolder_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
            {
                if (_model.ImagePaths.Any(d => dialog.FileName.StartsWith(d + "\\")))
                {
                    MessageBox.Show(this._window,
                        "The selected folder is already on the path of one of the included folders",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
                else if (_model.ImagePaths.Any(d => d.StartsWith(dialog.FileName)))
                {
                    MessageBox.Show(this._window,
                        "One of the included folders is on the path of the selected folder! It is recommended that you remove it.",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                _model.ImagePaths.Add(dialog.FileName);
            }

        }

        private void RemoveFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(this._window,
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
            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
            {
                if (_model.ImagePaths.All(d => !dialog.FileName.StartsWith(d)))
                {
                    MessageBox.Show(this._window,
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
            var result = MessageBox.Show(this._window,
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
            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
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

            var result = MessageBox.Show(this._window,
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
            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
            {
                if (dialog.FileName == _dataStore.DatabasePath)
                {
                    MessageBox.Show(this._window,
                    "The selected file is the current database. Please try another file.",
                    "Restore Database", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);

                    return;
                }

                var result = MessageBox.Show(this._window,
                    $"Are you sure you want to restore the file {dialog.FileName}? Your current database will be overwritten!",
                "Restore Database", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation, MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    if (!_dataStore.TryRestoreBackup(dialog.FileName))
                    {
                        MessageBox.Show(this._window,
                            "The database backup is not a Diffusion Toolkit database.",
                            "Restore Database", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show(this._window,
                    "The database backup has been restored.",
                    "Restore Database", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void BrowseHashCache_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("A1111 cache", "*.json"));
            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
            {
                _model.HashCache = dialog.FileName;
            }
        }

        private void BrowseCustomViewer_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.DefaultFileName = _model.CustomCommandLine;
            dialog.Filters.Add(new CommonFileDialogFilter("Executable files", "*.exe;*.bat;*.cmd"));
            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
            {
                _model.CustomCommandLine = dialog.FileName;
            }
        }

        public void ApplySettings()
        {
            var settings = ServiceLocator.Settings;

            if (_model.IsDirty)
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
                settings.SlideShowDelay = _model.SlideShowDelay;
                settings.ScrollNavigation = _model.ScrollNavigation;
                settings.AutoAdvance = _model.AdvanceOnTag;

                settings.StoreMetadata = _model.StoreMetadata;
                settings.StoreWorkflow = _model.StoreWorkflow;
                settings.ScanUnavailable = _model.ScanUnavailable;
                settings.ExternalApplications = _model.ExternalApplications.Select(d => new ExternalApplication()
                {
                    CommandLineArgs = d.CommandLineArgs,
                    Name = d.Name,
                    Path = d.Path
                }).ToList();

                settings.Culture = _model.Culture;
            }

        }

        private void BrowseExternalApplicationPath_OnClick(object sender, RoutedEventArgs e)
        {
            //var button = (Button)sender;
            //var dc = (ExternalApplication)button.DataContext;
            var app = _model.SelectedApplication;

            using var dialog = new CommonOpenFileDialog();
            dialog.DefaultFileName = app.Path;
            dialog.Filters.Add(new CommonFileDialogFilter("Executable files", "*.exe;*.bat;*.cmd"));
            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
            {
                app.Path = dialog.FileName;
            }
        }

        private void RemoveExternalApplication_OnClick(object sender, RoutedEventArgs e)
        {
            if (_model.SelectedApplication != null)
            {
                var result = MessageBox.Show(this._window,
                $"Are you sure you want to remove \"{_model.SelectedApplication.Name}\"?",
                "Remove Application", MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    _model.ExternalApplications.Remove(_model.SelectedApplication);
                }
            }
        }

        private void AddExternalApplication_OnClick(object sender, RoutedEventArgs e)
        {
            _model.ExternalApplications.Add(new ExternalApplicationModel()
            {
                Name = "Application",
                Path = ""
            });
        }

        private void MoveExternalApplicationUp_OnClick(object sender, RoutedEventArgs e)
        {
            if (_model.SelectedApplication != null)
            {
                var index = _model.ExternalApplications.IndexOf(_model.SelectedApplication);

                if (index > 0)
                {
                    _model.ExternalApplications.Move(index, index - 1);
                }
            }
        }

        private void MoveExternalApplicationDown_OnClick(object sender, RoutedEventArgs e)
        {
            if (_model.SelectedApplication != null)
            {
                var index = _model.ExternalApplications.IndexOf(_model.SelectedApplication);

                if (index < _model.ExternalApplications.Count)
                {
                    _model.ExternalApplications.Move(index, index + 1);
                }
            }
        }
    }



}

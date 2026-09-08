using Diffusion.Common;
using Diffusion.Toolkit.Classes;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Diffusion.Database;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Pages
{

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : NavigationPage
    {
        private SettingsModel _model = new SettingsModel();
        public Configuration.Settings _settings => ServiceLocator.Settings;

        private List<FolderChange> _folderChanges = new List<FolderChange>();

        private DataStore _dataStore => ServiceLocator.DataStore;

        private string GetLocalizedText(string key)
        {
            return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
        }

        public Settings(Window window) : base("settings")
        {
            _window = window;
            InitializeComponent();

            _model.PropertyChanged += ModelOnPropertyChanged;

            InitializeSettings();
            LoadCultures();

            _model.SetPristine();

            ServiceLocator.NavigatorService.OnNavigate += (sender, args) =>
            {
                InitializeSettings();
                if (args.TargetUri.Path.ToLower() == "settings" && args.TargetUri.Fragment != null && args.TargetUri.Fragment.ToLowerInvariant() == "externalapplications")
                {
                    ExternalApplicationsTab.IsSelected = true;
                }
            };

            DataContext = _model;
        }

        private void InitializeSettings()
        {
            _model.CheckForUpdatesOnStartup = _settings.CheckForUpdatesOnStartup;
            _model.ScanForNewImagesOnStartup = _settings.ScanForNewImagesOnStartup;

            _model.AutoRefresh = _settings.AutoRefresh;

            _model.ModelRootPath = _settings.ModelRootPath;
            _model.FileExtensions = _settings.FileExtensions;
            _model.SoftwareOnly = _settings.RenderMode == RenderMode.SoftwareOnly;

            _model.PageSize = _settings.PageSize;
            _model.UseBuiltInViewer = _settings.UseBuiltInViewer.GetValueOrDefault(true);
            _model.OpenInFullScreen = _settings.OpenInFullScreen.GetValueOrDefault(true);
            _model.UseSystemDefault = _settings.UseSystemDefault.GetValueOrDefault(false);
            _model.UseCustomViewer = _settings.UseCustomViewer.GetValueOrDefault(false);
            _model.CustomCommandLine = _settings.CustomCommandLine;
            _model.CustomCommandLineArgs = _settings.CustomCommandLineArgs;
            _model.SlideShowDelay = _settings.SlideShowDelay;
            _model.ScrollNavigation = _settings.ScrollNavigation;
            _model.AdvanceOnTag = _settings.AutoAdvance;
            _model.ShowFilenames = _settings.ShowFilenames;
            _model.PermanentlyDelete = _settings.PermanentlyDelete;
            _model.ConfirmDeletion = _settings.ConfirmDeletion;
            _model.LoopVideo = _settings.LoopVideo;

            _model.AutoTagNSFW = _settings.AutoTagNSFW;
            _model.NSFWTags = string.Join("\r\n", _settings.NSFWTags);

            _model.HashCache = _settings.HashCache;
            _model.PortableMode = _settings.PortableMode;

            _model.StoreMetadata = _settings.StoreMetadata;
            _model.StoreWorkflow = _settings.StoreWorkflow;
            _model.ScanUnavailable = _settings.ScanUnavailable;

            _model.ExternalApplications = new ObservableCollection<ExternalApplicationModel>(_settings.ExternalApplications.Select(d => new ExternalApplicationModel()
            {
                CommandLineArgs = d.CommandLineArgs,
                Name = d.Name,
                Path = d.Path
            }));

            _model.Theme = _settings.Theme;
            _model.Culture = _settings.Culture;
            _model.SetPristine();
        }

        private void LoadCultures()
        {
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

        }


        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsModel.Theme):
                    ThemeManager.ChangeTheme(_model.Theme);
                    break;
                case nameof(SettingsModel.Culture):
                    _settings.Culture = _model.Culture;
                    break;
                case nameof(SettingsModel.SlideShowDelay):
                    {
                        if (_model.SlideShowDelay < 1)
                        {
                            _model.SlideShowDelay = 1;
                        }
                        if (_model.SlideShowDelay > 100)
                        {
                            _model.SlideShowDelay = 100;
                        }

                        break;
                    }
            }
        }

        private Window _window;
        
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
            var newApplication = new ExternalApplicationModel()
            {
                Name = "Application",
                Path = ""
            };
            _model.ExternalApplications.Add(newApplication);
            _model.SelectedApplication = newApplication;
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

        private void ApplyChanges_OnClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            
            _model.SetPristine();
        }

        private void RevertChanges_OnClick(object sender, RoutedEventArgs e)
        {
            InitializeSettings();
        }


        public void ApplySettings()
        {
            if (_model.IsDirty)
            {
                _settings.SetPristine();

                _settings.ModelRootPath = _model.ModelRootPath;
                _settings.FileExtensions = _model.FileExtensions;
                _settings.Theme = _model.Theme;
                _settings.PageSize = _model.PageSize;
                _settings.AutoRefresh = _model.AutoRefresh;

                _settings.CheckForUpdatesOnStartup = _model.CheckForUpdatesOnStartup;
                _settings.ScanForNewImagesOnStartup = _model.ScanForNewImagesOnStartup;
                _settings.AutoTagNSFW = _model.AutoTagNSFW;
                _settings.NSFWTags = _model.NSFWTags.Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                _settings.HashCache = _model.HashCache;
                _settings.PortableMode = _model.PortableMode;
                _settings.RenderMode = _model.SoftwareOnly ? RenderMode.SoftwareOnly : RenderMode.Default;
                
                _settings.UseBuiltInViewer = _model.UseBuiltInViewer;
                _settings.OpenInFullScreen = _model.OpenInFullScreen;
                _settings.UseSystemDefault = _model.UseSystemDefault;
                _settings.UseCustomViewer = _model.UseCustomViewer;
                _settings.CustomCommandLine = _model.CustomCommandLine;
                _settings.CustomCommandLineArgs = _model.CustomCommandLineArgs;
                _settings.SlideShowDelay = _model.SlideShowDelay;
                _settings.ScrollNavigation = _model.ScrollNavigation;
                _settings.AutoAdvance = _model.AdvanceOnTag;
                _settings.ShowFilenames = _model.ShowFilenames;
                _settings.PermanentlyDelete = _model.PermanentlyDelete;
                _settings.ConfirmDeletion = _model.ConfirmDeletion;
                _settings.LoopVideo = _model.LoopVideo;

                _settings.StoreMetadata = _model.StoreMetadata;
                _settings.StoreWorkflow = _model.StoreWorkflow;
                _settings.ScanUnavailable = _model.ScanUnavailable;
                _settings.ExternalApplications = _model.ExternalApplications.Select(d => new ExternalApplication()
                {
                    CommandLineArgs = d.CommandLineArgs,
                    Name = d.Name,
                    Path = d.Path
                }).ToList();

                _settings.Culture = _model.Culture;

            }
        }


    }



}

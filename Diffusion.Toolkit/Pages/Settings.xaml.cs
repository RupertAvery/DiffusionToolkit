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
            InitializeFolders();

            _model.CheckForUpdatesOnStartup = _settings.CheckForUpdatesOnStartup;
            _model.ScanForNewImagesOnStartup = _settings.ScanForNewImagesOnStartup;

            _model.WatchFolders = _settings.WatchFolders;
            _model.AutoRefresh = _settings.AutoRefresh;

            _model.ModelRootPath = _settings.ModelRootPath;
            _model.FileExtensions = _settings.FileExtensions;

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

        private void InitializeFolders()
        {
            _model.ImagePaths = new ObservableCollection<string>(ServiceLocator.FolderService.RootFolders.Select(d => d.Path));
            _model.ExcludePaths = new ObservableCollection<string>(ServiceLocator.FolderService.ExcludedFolders.Select(d => d.Path));
            _model.RecurseFolders = _settings.RecurseFolders;
            _model.SetFoldersPristine();
            _folderChanges.Clear();
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

        private void AddFolder_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
            {
                var path = dialog.FileName;

                if (_model.ImagePaths.Any(d => path.StartsWith(d + "\\", StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show(this._window,
                        "The selected folder is already included the path of one of the existing folders",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
                else if (_model.ImagePaths.Any(d => d.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
                else if (_model.ImagePaths.Any(d => d.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show(this._window,
                        "One or more of the existing folders is included the path of the selected folder",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                _model.ImagePaths.Add(path);

                if (!ServiceLocator.FolderService.RootFolders.Select(d => d.Path).Contains(path))
                {
                    _folderChanges.Add(new FolderChange()
                    {
                        FolderType = FolderType.Watched,
                        ChangeType = ChangeType.Add,
                        Path = path,
                    });
                }
                else
                {
                    var removeChange = _folderChanges.Find(d =>
                        d is { FolderType: FolderType.Watched, ChangeType: ChangeType.Remove } && d.Path == path);
                    if (removeChange != null)
                    {
                        _folderChanges.Remove(removeChange);
                    }
                }

                _model.SetFoldersDirty();

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
                var path = _model.ImagePaths[_model.SelectedIndex];
                _model.ImagePaths.RemoveAt(_model.SelectedIndex);
                
                if (ServiceLocator.FolderService.RootFolders.Select(d => d.Path).Contains(path))
                {
                    _folderChanges.Add(new FolderChange()
                    {
                        FolderType = FolderType.Watched,
                        ChangeType = ChangeType.Remove,
                        Path = path,
                    });
                }
                else
                {
                    var addChange = _folderChanges.Find(d =>
                        d is { FolderType: FolderType.Watched, ChangeType: ChangeType.Add } && d.Path == path);
                    if (addChange != null)
                    {
                        _folderChanges.Remove(addChange);
                    }
                }

                _model.SetFoldersDirty();
            }
        }


        private void ExcludedAddFolder_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
            {
                var path = dialog.FileName;

                if (_model.ImagePaths.All(d => !path.StartsWith(d)))
                {
                    MessageBox.Show(this._window,
                        "The selected folder must be on the path of one of the included folders",
                        "Add Excluded folder", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                if (!ServiceLocator.FolderService.ExcludedFolders.Select(d => d.Path).Contains(path))
                {
                    _folderChanges.Add(new FolderChange()
                    {
                        FolderType = FolderType.Excluded,
                        ChangeType = ChangeType.Add,
                        Path = path,
                    });
                }
                else
                {
                    var removeChange = _folderChanges.Find(d =>
                        d is { FolderType: FolderType.Excluded, ChangeType: ChangeType.Remove } && d.Path == path);
                    if (removeChange != null)
                    {
                        _folderChanges.Remove(removeChange);
                    }
                }

                _model.ExcludePaths.Add(path);
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
                var path = _model.ExcludePaths[_model.ExcludedSelectedIndex];
                _model.ExcludePaths.RemoveAt(_model.ExcludedSelectedIndex);

                if (ServiceLocator.FolderService.ExcludedFolders.Select(d => d.Path).Contains(path))
                {
                    _folderChanges.Add(new FolderChange()
                    {
                        FolderType = FolderType.Excluded,
                        ChangeType = ChangeType.Remove,
                        Path = path,
                    });
                }
                else
                {
                    var addChange = _folderChanges.Find(d =>
                        d is { FolderType: FolderType.Excluded, ChangeType: ChangeType.Add } && d.Path == path);
                    if (addChange != null)
                    {
                        _folderChanges.Remove(addChange);
                    }
                }
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

        private void ChangeFolderPath_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.EnsurePathExists = false;
            var oldPath = _model.ImagePaths[_model.SelectedIndex];

            dialog.InitialDirectory = oldPath;

            if (dialog.ShowDialog(this._window) == CommonFileDialogResult.Ok)
            {
                var newPath = dialog.FileName;
                if (oldPath != newPath)
                {
                    _model.ImagePaths[_model.SelectedIndex] = newPath;
                    _folderChanges.Add(new FolderChange()
                    {
                        ChangeType = ChangeType.ChangePath,
                        Path = oldPath,
                        NewPath = newPath
                    });
                    _model.SetFoldersDirty();
                }
            }
        }

        private void RevertFolders_OnClick(object sender, RoutedEventArgs e)
        {
            InitializeFolders();
        }

        private void ApplyChanges_OnClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();


            if (_model.IsFoldersDirty)
            {
                if (ServiceLocator.MainModel.IsBusy)
                {
                    MessageBox.Show(ServiceLocator.WindowService.CurrentWindow, GetLocalizedText("Settings.Apply.Folders.Busy"), "Settings", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    var changes = _folderChanges.ToList();
                    _folderChanges.Clear();
                    _model.SetFoldersPristine();

                    Task.Run(async () =>
                    {
                        await ServiceLocator.FolderService.ApplyFolderChanges(changes);
                    });
                }
            }

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

                // TODO: Remove, these are no longer used
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
                _settings.AutoAdvance = _model.AdvanceOnTag;
                _settings.ShowFilenames = _model.ShowFilenames;
                _settings.PermanentlyDelete = _model.PermanentlyDelete;
                _settings.ConfirmDeletion = _model.ConfirmDeletion;

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

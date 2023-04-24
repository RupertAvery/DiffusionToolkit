using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using Diffusion.Toolkit.Classes;
using Search = Diffusion.Toolkit.Pages.Search;
using Diffusion.Toolkit.Thumbnails;
using Diffusion.IO;
using System.Threading;
using System.Threading.Tasks;
using Diffusion.Toolkit.Themes;
using Microsoft.Win32;
using Diffusion.Toolkit.Pages;
using MessageBox = System.Windows.MessageBox;
using Model = Diffusion.Common.Model;
using Diffusion.Updater;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BorderlessWindow
    {
        private readonly MainModel _model;
        private NavigatorService _navigatorService;
        private DataStoreOptions _dataStoreOptions;



        private Configuration<Settings> _configuration;
        private Settings? _settings;
        private CancellationTokenSource _scanCancellationTokenSource;

        private Search _search;
        private Pages.Models _models;
        private bool _tipsOpen;
        private MessagePopupManager _messagePopupManager;

 

        private DataStore _dataStore => _dataStoreOptions.Value;

        public MainWindow()
        {
            Logger.Log("===========================================");
            Logger.Log($"Started Diffusion Toolkit {AppInfo.Version}");

            
            var settingsPath = Path.Combine(AppInfo.AppDir, "config.json");
            var dbPath = Path.Combine(AppInfo.AppDir, "diffusion-toolkit.db");

            var isPortable = true;

            if (!File.Exists(settingsPath))
            {
                isPortable = false;
                settingsPath = Path.Combine(AppInfo.AppDataPath, "config.json");
                dbPath = Path.Combine(AppInfo.AppDataPath, "diffusion-toolkit.db");
            }

            _configuration = new Configuration<Settings>(settingsPath, isPortable);

            Logger.Log($"Opening database at {dbPath}");

            var dataStore = new DataStore(dbPath);

            _dataStoreOptions = new DataStoreOptions(dataStore);

            InitializeComponent();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

            QueryBuilder.Samplers = File.ReadAllLines("samplers.txt").ToList();

            Logger.Log($"Creating Thumbnail loader");

            ThumbnailLoader.CreateInstance(Dispatcher);

            _navigatorService = new NavigatorService(this)
            {
                OnNavigate = OnNavigate
            };

            SystemEvents.UserPreferenceChanged += SystemEventsOnUserPreferenceChanged;


            _model = new MainModel();
            _model.Rescan = new AsyncCommand(RescanTask);
            _model.Rebuild = new AsyncCommand(RebuildTask);
            _model.ReloadHashes = new AsyncCommand(async () =>
            {
                LoadModels();
                await _messagePopupManager.Show("Models have been reloaded", "Diffusion Toolkit", PopupButtons.OK);
            });
            _model.RemoveMarked = new RelayCommand<object>(RemoveMarked);
            _model.Settings = new RelayCommand<object>(ShowSettings);
            _model.CancelScan = new AsyncCommand(CancelScan);
            _model.About = new RelayCommand<object>((o) => ShowAbout());
            _model.Help = new RelayCommand<object>((o) => ShowTips());
            _model.ToggleInfo = new RelayCommand<object>((o) => ToggleInfo());
            _model.ToggleNSFWBlur = new RelayCommand<object>((o) => ToggleNSFWBlur());
            _model.ToggleHideNSFW = new RelayCommand<object>((o) => ToggleHideNSFW());
            _model.ToggleFitToPreview = new RelayCommand<object>((o) => ToggleFitToPreview());
            _model.SetThumbnailSize = new RelayCommand<object>((o) => SetThumbnailSize(int.Parse((string)o)));
            _model.TogglePreview = new RelayCommand<object>((o) => TogglePreview());
            _model.PoputPreview = new RelayCommand<object>((o) => PopoutPreview());

            _model.MarkAllForDeletion = new RelayCommand<object>((o) => MarkAllForDeletion());
            _model.UnmarkAllForDeletion = new RelayCommand<object>((o) => UnmarkAllForDeletion());
            _model.RemoveMatching = new RelayCommand<object>((o) => RemoveFromDatabase());
            _model.AutoTagNSFW = new RelayCommand<object>((o) => AutoTagNSFW());
            _model.AddMatchingToAlbum = new RelayCommand<object>((o) => AddMatchingToAlbum());

            _model.ToggleAlbum = new RelayCommand<object>((o) => ToggleAlbum());

            _model.Refresh = new RelayCommand<object>((o) => Refresh());
            _model.QuickCopy = new RelayCommand<object>((o) =>
            {
                var win = new QuickCopy(_settings);
                win.Owner = this;
                win.ShowDialog();
            });
            _model.PropertyChanged += ModelOnPropertyChanged;

            var total = _dataStore.GetTotal();

            _model.Status = $"{total:###,###,##0} images in database";
            _model.TotalFilesScan = 100;


            this.Loaded += OnLoaded;
            this.Closing += OnClosing;
            _model.Close = new RelayCommand<object>(o =>
            {
                this.Close();
            });

            DataContext = _model;


            _messagePopupManager = new MessagePopupManager(this, PopupHost, Frame, Dispatcher);

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-PT");
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-PT");
            //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
            //    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            //var str = new System.Text.StringBuilder();
            //using (var writer = new System.IO.StringWriter(str))
            //    System.Windows.Markup.XamlWriter.Save(EditMenu.Template, writer);
            //System.Diagnostics.Debug.Write(str);

            //var str = new System.Text.StringBuilder();
            //using (var writer = new System.IO.StringWriter(str))
            //    System.Windows.Markup.XamlWriter.Save(((Separator)Hello.ContextMenu.Items[1]).Template, writer);
            //System.Diagnostics.Debug.Write(str);
        }

        private void Refresh()
        {
            _search.SearchImages(null);
        }


        private void PopoutPreview(bool hidePreview = true, bool maximized = false)
        {
            if (_previewWindow == null)
            {
                if (hidePreview)
                {
                    _model.IsPreviewVisible = false;
                    _search.SetPreviewVisible(_model.IsPreviewVisible);
                }

                _previewWindow = new PreviewWindow(_dataStore, _model);
                _previewWindow.WindowState = maximized ? WindowState.Maximized : WindowState.Normal;
                _previewWindow.Owner = this;

                _previewWindow.PreviewKeyUp += _search.ExtOnKeyUp;
                _previewWindow.PreviewKeyDown += _search.ExtOnKeyDown;
                _previewWindow.OnDrop = (s) => _search.LoadPreviewImage(s);
                _previewWindow.Changed = (id) => _search.Update(id);
                _previewWindow.Closed += (sender, args) =>
                {
                    _search.OnCurrentImageChange = null;
                    _previewWindow = null;
                };
                _previewWindow.SetCurrentImage(_search.CurrentImage);
                _search.OnCurrentImageChange = (image) =>
                {
                    _previewWindow?.SetCurrentImage(image);
                };
                _previewWindow.Show();
            }
            else
            {
                _previewWindow.SetCurrentImage(_search.CurrentImage);
            }
        }


        private void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = ((Exception)e.ExceptionObject);

            Logger.Log($"An unhandled exception occured: {exception.Message}\r\n\r\n{exception.StackTrace}");

            MessageBox.Show(this, exception.Message, "An unhandled exception occured", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            
        }

        private void TogglePreview()
        {
            _model.IsPreviewVisible = !_model.IsPreviewVisible;
            _search.SetPreviewVisible(_model.IsPreviewVisible);
        }

        private void SetThumbnailSize(int size)
        {
            _settings.ThumbnailSize = size;
            ThumbnailLoader.Instance.Size = _settings.ThumbnailSize;
            _search.SetThumbnailSize(_settings.ThumbnailSize);

        }

        private void SystemEventsOnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
            {
                UpdateTheme();
            }
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainModel.ShowIcons))
            {
                _search.SetOpacityView(_model.ShowIcons);
            }
            else if (e.PropertyName == nameof(MainModel.HideIcons))
            {
                _search.SetIconVisibility(_model.HideIcons);
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_configuration.TryLoad(out _settings))
            {
                Logger.Log($"Opening Settings for first time");

                _settings = new Settings(true);

                UpdateTheme();

                var welcome = new WelcomeWindow(_settings);
                welcome.Owner = this;
                welcome.ShowDialog();

                var settings = new SettingsWindow(_dataStore, _settings);
                settings.Owner = this;
                settings.ShowDialog();

                if (_settings.IsDirty())
                {
                    _configuration.Save(_settings);
                    _settings.SetPristine();
                }

                ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);

                await TryScanFolders();

            }
            else
            {
                _settings.ShowAlbumPanel ??= true;
                _settings.RecurseFolders ??= true;

                UpdateTheme();

                if (!_settings.DontShowWelcomeOnStartup)
                {
                    var welcome = new WelcomeWindow(_settings);
                    welcome.Owner = this;
                    welcome.ShowDialog();

                    if (_settings.IsDirty())
                    {
                        _configuration.Save(_settings);
                        _settings.SetPristine();
                    }
                }

                _settings.PortableMode = _configuration.Portable;
                _settings.SetPristine();

                ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);
            }


            if (_settings.WindowState.HasValue)
            {
                this.WindowState = _settings.WindowState.Value;
            }

            if (_settings.WindowSize.HasValue)
            {
                this.Width = _settings.WindowSize.Value.Width;
                this.Height = _settings.WindowSize.Value.Height;
            }

            _model.HideNSFW = _settings.HideNSFW;
            QueryBuilder.HideNFSW = _model.HideNSFW;
            _model.NSFWBlur = _settings.NSFWBlur;
            _model.FitToPreview = _settings.FitToPreview;

            _model.ShowAlbumPanel = _settings.ShowAlbumPanel.GetValueOrDefault(true);

            Activated += OnActivated;
            StateChanged += OnStateChanged;
            SizeChanged += OnSizeChanged;

            Logger.Log($"Initializing pages");

            _models = new Pages.Models(_dataStoreOptions, _settings);

            _models.OnModelUpdated = (model) =>
            {
                var updatedModel = _modelsCollection.FirstOrDefault(m => m.Path == model.Path);
                if (updatedModel != null)
                {
                    updatedModel.SHA256 = model.SHA256;
                    _search.SetModels(_modelsCollection);
                }
            };

            _search = new Search(_navigatorService, _dataStoreOptions, _messagePopupManager, _settings, _model);

            _search.MoveFiles = (files) =>
            {
                using var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
                {
                    var path = dialog.FileName;

                    var isInPath = false;
                    foreach (var imagePath in _settings.ImagePaths)
                    {
                        if (path.StartsWith(imagePath, true, CultureInfo.InvariantCulture))
                        {
                            isInPath = true;
                            break;
                        }
                    }

                    Task.Run(async () =>
                    {
                        await MoveFiles(files, path, !isInPath);


                        if (!isInPath)
                        {
                            _search.SearchImages();
                        }
                    });

                }
            };

            _prompts = new Prompts(_dataStore, _settings);


            ThumbnailLoader.Instance.Size = _settings.ThumbnailSize;
            _search.SetThumbnailSize(_settings.ThumbnailSize);

            _search.OnPopout = () => PopoutPreview();
            _search.OnCurrentImageOpen = OnCurrentImageOpen;

            _model.ShowFavorite = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _search.ShowFavorite();
            });

            _model.ShowMarked = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _search.ShowMarked();
            });

            _model.ShowSearch = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _search.ShowSearch();
            });

            _model.ShowFolders = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _search.ShowFolders();
            });

            _model.ShowAlbums = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _search.ShowAlbums();
            });

            _model.ShowModels = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("models");
            });

            _model.ShowPrompts = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("prompts");
            });

            if (_settings.WatchFolders)
            {
                foreach (var path in _settings.ImagePaths)
                {
                    if (Directory.Exists(path))
                    {
                        var watcher = new FileSystemWatcher(path)
                        {
                            EnableRaisingEvents = true,
                            IncludeSubdirectories = true,
                        };
                        watcher.Created += WatcherOnCreated;
                        watcher.Renamed += WatcherOnCreated;
                        _watchers.Add(watcher);
                    }
                }
            }


            var pages = new Dictionary<string, Page>()
            {
                { "search", _search },
                { "models", _models },
                { "prompts", _prompts },
                //{ "config", _configPage},
                //{ "setup", new SetupPage(_navigatorService) },
            };

            _navigatorService.SetPages(pages);

            _navigatorService.Goto("search");

            Logger.Log($"Loading models");

            LoadModels();

            Logger.Log($"{_modelsCollection.Count} models loaded");

            if (_settings.CheckForUpdatesOnStartup)
            {
                var checker = new UpdateChecker();

                Logger.Log($"Checking for latest version");
                try
                {
                    var hasUpdate = await checker.CheckForUpdate();

                    if (hasUpdate)
                    {
                        var result = await _messagePopupManager.Show("An update is available. Do you want to install now?", "Diffusion Toolkit", PopupButtons.YesNo);
                        if (result == PopupResult.Yes)
                        {
                            CallUpdater();
                        }
                    }
                }
                catch (Exception exception)
                {
                    await _messagePopupManager.Show(exception.Message, "Update error", PopupButtons.OK);
                }

            }

            if (_settings.ScanForNewImagesOnStartup)
            {
                Logger.Log($"Scanning for new images");

                _scanCancellationTokenSource = new CancellationTokenSource();

                await ScanInternal(_settings, false, false, _scanCancellationTokenSource.Token);
            }

            if (_settings.ImagePaths.Any())
            {
                _search.SearchImages(null);
            }

            Logger.Log($"Init completed");

            //_previewWindow = new PreviewWindow();
            //_previewWindow.ShowInTaskbar = false;
            //_previewWindow.Owner = this;
            //_previewWindow.Show();

        }

        private void OnCurrentImageOpen(ImageViewModel obj)
        {
            //if (obj == null) return;
            //var p = obj.Path;

            //var processInfo = new ProcessStartInfo()
            //{
            //    FileName = "explorer.exe",
            //    Arguments = $"/select,\"{p}\"",
            //    UseShellExecute = true
            //};

            //Process.Start(processInfo);

            PopoutPreview(false, true);

            //Process.Start("explorer.exe", $"/select,\"{p}\"");

        }

        private async void ShowSettings(object obj)
        {
            try
            {
                var oldPathCount = _settings.ImagePaths.Count;

                var settings = new SettingsWindow(_dataStore, _settings);
                settings.Owner = this;
                settings.ShowDialog();

                if (_settings.IsDirty())
                {
                    _configuration.Save(_settings);

                    _search.Settings = _settings;

                    if (_settings.IsPropertyDirty(nameof(Settings.PageSize)))
                    {
                        ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);
                        _search.SearchImages();
                    }

                    if (_settings.IsPropertyDirty(nameof(Settings.ModelRootPath)) || _settings.IsPropertyDirty(nameof(Settings.HashCache)))
                    {
                        LoadModels();
                    }

                    if (_settings.IsPropertyDirty(nameof(Settings.Theme)))
                    {
                        UpdateTheme();
                    }

                    if (_settings.IsPropertyDirty(nameof(Settings.WatchFolders)))
                    {
                        if (_settings.WatchFolders)
                        {
                            CreateWatchers();
                        }
                        else
                        {
                            RemoveWatchers();
                        }
                    }

                    if (_settings.IsPropertyDirty(nameof(Settings.ImagePaths)))
                    {
                        await TryScanFolders();

                        if (_settings.WatchFolders)
                        {
                            RemoveWatchers();
                            CreateWatchers();
                        }
                    }


                    if (_settings.IsPropertyDirty(nameof(Settings.PortableMode)))
                    {
                        if (_settings.PortableMode)
                        {
                            GoPortable();
                        }
                        else
                        {
                            GoLocal();
                        }
                    }

                    _settings.SetPristine();

                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }


        private void UpdateTheme()
        {
            ThemeManager.ChangeTheme(_settings!.Theme);
        }


        private void OnNavigate(Page page)
        {
            _model.Page = page;
        }


        private ICollection<Model> _modelsCollection;
        private Prompts _prompts;

        private void LoadModels()
        {
            if (!string.IsNullOrEmpty(_settings.ModelRootPath) && Directory.Exists(_settings.ModelRootPath))
            {
                _modelsCollection = ModelScanner.Scan(_settings.ModelRootPath).ToList();
                if (!string.IsNullOrEmpty(_settings.HashCache))
                {
                    var hashes = JsonSerializer.Deserialize<Hashes>(File.ReadAllText(_settings.HashCache));
                    foreach (var model in _modelsCollection)
                    {
                        if (hashes.hashes.TryGetValue("checkpoint/" + model.Path, out var hash))
                        {
                            model.SHA256 = hash.sha256;
                        }
                    }
                }
            }
            else
            {
                _modelsCollection = new List<Model>();
            }
            _search.SetModels(_modelsCollection);
            _models.SetModels(_modelsCollection);
            QueryBuilder.SetModels(_modelsCollection);
        }

        private async Task TryScanFolders()
        {
            if (_settings.ImagePaths.Any())
            {
                if (await _messagePopupManager.Show("Do you want to scan your folders now?", "Setup", PopupButtons.YesNo) == PopupResult.Yes)
                {
                    await Scan();
                };
            }
            else
            {
                await _messagePopupManager.ShowMedium("You have not setup any image folders. You will not be able to search for anything yet.\r\n\r\nAdd one or more folders first, then click the Scan Folders for new images icon in the toolbar.", "Setup", PopupButtons.OK);
            }
        }

    }



    public class Hashes
    {
        public Dictionary<string, HashInfo> hashes { get; set; }
    }

    public class HashInfo
    {
        public double mtime { get; set; }
        public string sha256 { get; set; }
    }

    static class WindowExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);


        public static double ActualTop(this Window window)
        {
            switch (window.WindowState)
            {
                case WindowState.Normal:
                    return window.Top;
                case WindowState.Minimized:
                    return window.RestoreBounds.Top;
                case WindowState.Maximized:
                {
                    RECT rect;
                    GetWindowRect((new WindowInteropHelper(window)).Handle, out rect);
                    return rect.Top;
                }
            }
            return 0;
        }
        public static double ActualLeft(this Window window)
        {
            switch (window.WindowState)
            {
                case WindowState.Normal:
                    return window.Left;
                case WindowState.Minimized:
                    return window.RestoreBounds.Left;
                case WindowState.Maximized:
                {
                    RECT rect;
                    GetWindowRect((new WindowInteropHelper(window)).Handle, out rect);
                    return rect.Left;
                }
            }
            return 0;
        }
    }
}


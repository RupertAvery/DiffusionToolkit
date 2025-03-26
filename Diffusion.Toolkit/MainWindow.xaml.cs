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
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Windows.Forms;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using System.Text.Json.Serialization;
using Diffusion.Civitai.Models;
using Diffusion.Toolkit.Localization;
using WPFLocalizeExtension.Engine;
using Diffusion.Toolkit.Controls;
using System.Configuration;
using System.Windows.Input;
using Diffusion.Toolkit.Services;
using System.Windows.Threading;
using Diffusion.Toolkit.Common;

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
        

        private Search _search;
        private Pages.Models _models;
        private bool _tipsOpen;
        private MessagePopupManager _messagePopupManager;
        private string _dbPath;


        private DataStore _dataStore => _dataStoreOptions.Value;

        public MainWindow()
        {
            try
            {
                Logger.Log("===========================================");
                Logger.Log($"Started Diffusion Toolkit {AppInfo.Version}");


                var settingsPath = Path.Combine(AppInfo.AppDir, "config.json");
                _dbPath = Path.Combine(AppInfo.AppDir, "diffusion-toolkit.db");

                var isPortable = true;

                if (!File.Exists(settingsPath))
                {
                    isPortable = false;
                    settingsPath = Path.Combine(AppInfo.AppDataPath, "config.json");
                    _dbPath = Path.Combine(AppInfo.AppDataPath, "diffusion-toolkit.db");
                }

                _configuration = new Configuration<Settings>(settingsPath, isPortable);

                InitializeComponent();

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

                QueryBuilder.Samplers = File.ReadAllLines("samplers.txt").ToList();

                Logger.Log($"Creating Thumbnail loader");

                ThumbnailLoader.CreateInstance();

                _navigatorService = new NavigatorService(this);
                _navigatorService.OnNavigate += OnNavigate;

                SystemEvents.UserPreferenceChanged += SystemEventsOnUserPreferenceChanged;


                _model = new MainModel();

                ServiceLocator.MainModel = _model;

                _model.Rescan = new AsyncCommand<object>(RescanTask);
                _model.Rebuild = new AsyncCommand<object>(RebuildTask);
                _model.ReloadHashes = new AsyncCommand<object>(async (o) =>
                {
                    LoadModels();
                    await _messagePopupManager.Show("Models have been reloaded", "Diffusion Toolkit", PopupButtons.OK);
                });
                _model.RemoveMarked = new RelayCommand<object>(RemoveMarked);
                _model.SettingsCommand = new RelayCommand<object>(ShowSettings);
                _model.CancelCommand = new AsyncCommand<object>(async (o) => await CancelProgress());
                _model.AboutCommand = new RelayCommand<object>((o) => ShowAbout());
                _model.HelpCommand = new RelayCommand<object>((o) => ShowTips());
                _model.ToggleInfoCommand = new RelayCommand<object>((o) => ToggleInfo());

                _model.ToggleNSFWBlurCommand = new RelayCommand<object>((o) => ToggleNSFWBlur());

                _model.ToggleHideNSFW = new RelayCommand<object>((o) => ToggleHideNSFW());
                _model.ToggleHideDeleted = new RelayCommand<object>((o) => ToggleHideDeleted());
                _model.ToggleHideUnavailable = new RelayCommand<object>((o) => ToggleHideUnavailable());

                _model.ToggleFitToPreview = new RelayCommand<object>((o) => ToggleFitToPreview());
                _model.ToggleActualSize = new RelayCommand<object>((o) => ToggleActualSize());

                _model.ToggleAutoAdvance = new RelayCommand<object>((o) => ToggleAutoAdvance());

                _model.SetThumbnailSize = new RelayCommand<object>((o) => SetThumbnailSize(int.Parse((string)o)));
                _model.TogglePreview = new RelayCommand<object>((o) => TogglePreview());
                _model.PoputPreview = new RelayCommand<object>((o) => PopoutPreview(true, true, false));
                _model.ResetLayout = new RelayCommand<object>((o) => ResetLayout());

                _model.SaveQuery = new RelayCommand<object>((o) => SaveQuery());
                _model.RescanResults = new RelayCommand<object>((o) => RescanResults());
                _model.AddAllToAlbum = new RelayCommand<object>((o) => AddAllToAlbum());
                _model.MarkAllForDeletion = new RelayCommand<object>((o) => MarkAllForDeletion());
                _model.UnmarkAllForDeletion = new RelayCommand<object>((o) => UnmarkAllForDeletion());
                _model.RemoveMatching = new RelayCommand<object>((o) => RemoveFromDatabase());
                _model.AutoTagNSFW = new RelayCommand<object>((o) => AutoTagNSFW());
                _model.DownloadCivitai = new RelayCommand<object>((o) => DownloadCivitaiModels());

                _model.FixFoldersCommand = new RelayCommand<object>((o) => FixFolders());
                _model.RemoveExcludedImagesCommand = new RelayCommand<object>((o) => CleanExcludedPaths());
                _model.CleanRemovedFoldersCommand = new AsyncCommand<object>(CleanRemovedFolders);

                _model.UnavailableFilesCommand = new AsyncCommand<object>(UnavailableFiles);

                _model.ShowFilterCommand = new RelayCommand<object>((o) => _search?.ShowFilter());
                _model.ToggleAutoRefresh = new RelayCommand<object>((o) => ToggleAutoRefresh());

                _model.SortAlbumCommand = new RelayCommand<object>((o) => SortAlbums());
                _model.ClearAlbumsCommand = new RelayCommand<object>((o) => ClearAlbums());
                _model.ClearModelsCommand = new RelayCommand<object>((o) => ClearModels());

                _model.ToggleNavigationPane = new RelayCommand<object>((o) => ToggleNavigationPane());
                _model.ToggleVisibilityCommand = new RelayCommand<string>((p) => ToggleVisibility(p));
                _model.ShowInExplorerCommand = new RelayCommand<FolderViewModel>((p) => ShowInExplorer(p));

                InitAlbums();

                _model.Refresh = new RelayCommand<object>((o) => Refresh());
                _model.QuickCopy = new RelayCommand<object>((o) =>
                {
                    var win = new QuickCopy(_settings);
                    win.Owner = this;
                    win.ShowDialog();
                });

                _model.Escape = new RelayCommand<object>((o) => Escape());

                _model.PropertyChanged += ModelOnPropertyChanged;


                this.Loaded += OnLoaded;
                this.Closing += OnClosing;
                _model.CloseCommand = new RelayCommand<object>(o =>
                {
                    this.Close();
                });

                DataContext = _model;


                _messagePopupManager = new MessagePopupManager(this, PopupHost, Frame, Dispatcher);

                ServiceLocator.ProgressService = new ProgressService(Dispatcher);
                ServiceLocator.MessageService = new MessageService(_messagePopupManager);

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
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }

        }

        private void ShowInExplorer(FolderViewModel folder)
        {
            var processInfo = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folder.Path}\"",
                UseShellExecute = true
            };

            Process.Start(processInfo);

            //Process.Start("explorer.exe", $"/select,\"{p}\"");
        }

        private void ToggleAutoAdvance()
        {
            _model.AutoAdvance = !_model.AutoAdvance;
            _model.Settings.AutoAdvance = _model.AutoAdvance;
        }

        private async Task UnavailableFiles(object o)
        {
            var window = new UnavailableFilesWindow(_dataStore, _settings);
            window.Owner = this;
            window.ShowDialog();
            if (window.DialogResult is true)
            {
                await ScanUnavailable(window.Model);
            }
        }

        private void ToggleNavigationPane()
        {
            _model.Settings.NavigationSection.ToggleSection();
        }

        private void ResetLayout()
        {
            _search.ResetLayout();
        }

        private void ToggleVisibility(string s)
        {
            switch (s)
            {
                case "Navigation.Folders":
                    _model.Settings.NavigationSection.ShowFolders = !_model.Settings.NavigationSection.ShowFolders;
                    break;
                case "Navigation.Models":
                    _model.Settings.NavigationSection.ShowModels = !_model.Settings.NavigationSection.ShowModels;
                    break;
                case "Navigation.Albums":
                    _model.Settings.NavigationSection.ShowAlbums = !_model.Settings.NavigationSection.ShowAlbums;
                    break;
            }
        }

        private void ToggleAutoRefresh()
        {
            _settings.AutoRefresh = !_settings.AutoRefresh;
            _model.AutoRefresh = _settings.AutoRefresh;
        }

        private void Escape()
        {
            _messagePopupManager.Cancel();
        }

        private void Refresh()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                _search.SearchImages(null);
            }
            else
            {
                _search.ReloadMatches(null);
            }
        }

        private PreviewWindow? _previewWindow;

        private void PopoutPreview(bool hidePreview, bool maximized, bool fullscreen)
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

                _previewWindow.AdvanceSlideShow = _search.Advance;

                _previewWindow.OnDrop = (s) => _search.LoadPreviewImage(s);
                _previewWindow.Changed = (id) => _search.Update(id);
                _previewWindow.Closed += (sender, args) =>
                {
                    _search.OnCurrentImageChange = null;
                    _search.ThumbnailListView.FocusCurrentItem();
                    _previewWindow = null;
                    _search.NavigationCompleted -= SearchOnNavigationCompleted;
                };
                _previewWindow.SetCurrentImage(_search.CurrentImage);

                _search.NavigationCompleted += SearchOnNavigationCompleted;

                _search.OnCurrentImageChange = (image) =>
                {
                    _previewWindow?.SetCurrentImage(image);
                };

                if (fullscreen)
                {
                    _previewWindow.ShowFullScreen();
                }
                else
                {
                    _previewWindow.Show();
                }

            }
            else
            {
                _previewWindow.SetCurrentImage(_search.CurrentImage);
            }
        }

        private void SearchOnNavigationCompleted(object? sender, EventArgs e)
        {
            _previewWindow?.SetFocus();
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
            _model.ThumbnailSize = _settings.ThumbnailSize;
            _search.SetThumbnailSize(_settings.ThumbnailSize);
            _prompts.SetThumbnailSize(_settings.ThumbnailSize);
        }

        private void SystemEventsOnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
            {
                UpdateTheme(_settings.Theme);
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
            else if (e.PropertyName == nameof(MainModel.SelectedImages))
            {
                if (_model.SelectedImages.Count > 1)
                {
                    _model.Status = $"{_model.SelectedImages.Count} items selected";
                }
                else
                {
                    _model.Status = "";
                }
            }
            else if (e.PropertyName == nameof(MainModel.CurrentAlbum))
            {
                //Debug.WriteLine(_model.CurrentAlbum.Name);
                //_search.SetMode("albums");
                //_search.SearchImages(null);
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataStore = new DataStore(_dbPath);

            ServiceLocator.SetDataStore(dataStore);

            var isFirstTime = false;

            if (!_configuration.Exists())
            {
                Logger.Log($"Opening Settings for first time");

                _settings = new Settings(true);

                UpdateTheme(_settings.Theme);

                var welcome = new WelcomeWindow(_settings);
                welcome.Owner = this;
                welcome.ShowDialog();

                var settings = new SettingsWindow(dataStore, _settings);
                settings.Owner = this;
                settings.ShowDialog();

                if (_settings.IsDirty())
                {
                    _configuration.Save(_settings);
                    _settings.SetPristine();
                }

                ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);

                isFirstTime = true;
            }
            else
            {
                try
                {
                    _configuration.Load(out _settings);

                    _settings.MetadataSection.Attach(_settings);
                    _settings.NavigationSection.Attach(_settings);
                    _settings.RecurseFolders ??= true;
                    _settings.UseBuiltInViewer ??= true;
                    _settings.SortAlbumsBy ??= "Name";
                    _settings.Theme ??= "System";

                    UpdateTheme(_settings.Theme);

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
                catch (Exception exception)
                {
                    MessageBox.Show(this, "An error occured while loading configuration settings. The application will exit", "Startup failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }

            }


            _settings.SettingChanged += (s, args) =>
            {
                if (args.SettingName == nameof(Settings.AutoRefresh))
                {
                    _model.AutoRefresh = _settings.AutoRefresh;
                }
            };

            if (_settings.WindowState.HasValue)
            {
                this.WindowState = _settings.WindowState.Value;
            }

            if (_settings.WindowSize.HasValue)
            {
                this.Width = _settings.WindowSize.Value.Width;
                this.Height = _settings.WindowSize.Value.Height;
            }

            if (_settings.Top.HasValue)
            {
                this.Top = _settings.Top.Value;
            }

            if (_settings.Left.HasValue)
            {
                this.Left = _settings.Left.Value;
            }

            _settings.Culture ??= "default";

            if (_settings.Culture == "default")
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.CurrentCulture;
            }
            else
            {
                LocalizeDictionary.Instance.Culture = new CultureInfo(_settings.Culture);
            }

            _model.AutoRefresh = _settings.AutoRefresh;
            _model.HideNSFW = _settings.HideNSFW;
            _model.HideDeleted = _settings.HideDeleted;
            _model.HideUnavailable = _settings.HideUnavailable;

            QueryBuilder.HideNSFW = _model.HideNSFW;
            QueryBuilder.HideDeleted = _model.HideDeleted;
            QueryBuilder.HideUnavailable = _model.HideUnavailable;

            _model.NSFWBlur = _settings.NSFWBlur;
            _model.FitToPreview = _settings.FitToPreview;
            _model.ActualSize = _settings.ActualSize;
            _model.AutoAdvance = _settings.AutoAdvance;

            _model.Settings = _settings;

            Activated += OnActivated;
            StateChanged += OnStateChanged;
            SizeChanged += OnSizeChanged;
            LocationChanged += OnLocationChanged;

            ServiceLocator.SetSettings(_settings);

            Logger.Log($"Initializing pages");


            await dataStore.Create(
                () => Dispatcher.Invoke(() => _messagePopupManager.ShowMessage("Please wait while we update your database", "Updating Database")),
                (handle) => { Dispatcher.Invoke(() => { ((MessagePopupHandle)handle).CloseAsync(); }); }
            );

            _dataStoreOptions = new DataStoreOptions(dataStore);

            var total = _dataStore.GetTotal();

            var text = GetLocalizedText("Main.Status.ImagesInDatabase").Replace("{count}", $"{total:n0}");

            _model.Status = text;
            _model.TotalProgress = 100;

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

            _search.Toast = (message, caption) => Toast(message, caption);

            _search.MoveFiles = (files) =>
            {
                using var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
                {
                    var path = dialog.FileName;

                    var isInPath = false;

                    // Check if the target path is in the list of selected diffusion folders

                    if (_settings.RecurseFolders.GetValueOrDefault(true))
                    {
                        foreach (var imagePath in _settings.ImagePaths)
                        {
                            if (path.StartsWith(imagePath, true, CultureInfo.InvariantCulture))
                            {
                                isInPath = true;
                                break;
                            }
                        }

                        // Now check if the path falls under one of the excluded paths.

                        foreach (var imagePath in _settings.ExcludePaths)
                        {
                            if (path.StartsWith(imagePath, true, CultureInfo.InvariantCulture))
                            {
                                isInPath = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // If recursion is turned off, the path must specifically equal one of the diffusion folders

                        foreach (var imagePath in _settings.ImagePaths)
                        {
                            if (path.Equals(imagePath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                isInPath = true;
                                break;
                            }
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

            _prompts = new Prompts(_navigatorService, _dataStoreOptions, _messagePopupManager, _model, _settings);

            ThumbnailLoader.Instance.Size = _settings.ThumbnailSize;

            _model.ThumbnailSize = _settings.ThumbnailSize;

            _search.SetThumbnailSize(_settings.ThumbnailSize);
            _search.SetPageSize(_settings.PageSize);

            _prompts.SetThumbnailSize(_settings.ThumbnailSize);
            _prompts.SetPageSize(_settings.PageSize);

            _search.OnPopout = () => PopoutPreview(true, true, false);
            _search.OnCurrentImageOpen = OnCurrentImageOpen;

            _model.ShowFavorite = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _model.ActiveView = "Favorites";
                _search.ShowFavorite();
            });

            _model.ShowMarked = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _model.ActiveView = "Recycle Bin";
                _search.ShowMarked();
            });

            _model.ShowSearch = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _model.ActiveView = "Diffusions";
                _search.ShowSearch();
            });

            _model.ShowFolders = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _model.ActiveView = "Folders";
                _search.ShowFolders();
            });

            _model.ShowAlbums = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("search");
                _model.ActiveView = "Albums";
                _search.ShowAlbums();
            });

            _model.ShowModels = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("models");
                _model.ActiveView = "Models";
            });

            _model.ShowPromptsCommand = new RelayCommand<object>((o) =>
            {
                _navigatorService.Goto("prompts");
                _model.ActiveView = "Prompts";
            });



            if (_settings.WatchFolders)
            {
                CreateWatchers();
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
            _model.ActiveView = "Diffusions";

            Logger.Log($"Loading models");


            LoadAlbums();
            LoadModels();
            LoadImageModels();
            InitFolders();


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
                        var result = await _messagePopupManager.Show(GetLocalizedText("Main.Update.UpdateAvailable"), "Diffusion Toolkit", PopupButtons.YesNo);
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


                _ = Task.Run(async () =>
                {
                    if (await ServiceLocator.ProgressService.TryStartTask())
                    {
                        try
                        {
                            await ScanInternal(_settings, false, false, ServiceLocator.ProgressService.CancellationToken);
                        }
                        finally
                        {
                            ServiceLocator.ProgressService.CompleteTask();
                        }
                    }
                });
            }

            if (_settings.ImagePaths.Any())
            {
                _search.SearchImages(null);

                if (isFirstTime)
                {
                    await TryScanFolders();
                }
            }

            Logger.Log($"Init completed");

            //_previewWindow = new PreviewWindow();
            //_previewWindow.ShowInTaskbar = false;
            //_previewWindow.Owner = this;
            //_previewWindow.Show();
            InitScanningEvents();
        }


        private void OnCurrentImageOpen(ImageViewModel obj)
        {
            //if (obj == null) return;
            var p = obj.Path;

            if (_settings.UseBuiltInViewer.GetValueOrDefault(true))
            {
                PopoutPreview(false, true, _settings.OpenInFullScreen.GetValueOrDefault(true));
            }
            else if (_settings.UseSystemDefault.GetValueOrDefault(false))
            {
                var processInfo = new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{p}\"",
                    UseShellExecute = true
                };

                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else if (_settings.UseCustomViewer.GetValueOrDefault(false))
            {
                var args = _settings.CustomCommandLineArgs;

                if (string.IsNullOrWhiteSpace(args))
                {
                    args = "%1";
                }

                if (string.IsNullOrEmpty(_settings.CustomCommandLine))
                {
                    MessageBox.Show(this, "No custom viewer set. Please check Settings", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                if (!File.Exists(_settings.CustomCommandLine))
                {
                    MessageBox.Show(this, "The specified application does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var processInfo = new ProcessStartInfo()
                {
                    FileName = _settings.CustomCommandLine,
                    Arguments = args.Replace("%1", $"\"{p}\""),
                    UseShellExecute = true
                };

                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message + "\r\n\r\nPlease check that the path to your custom viewer is valid and that the arguments are correct", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }


            //PopoutPreview(false, true);

            //Process.Start("explorer.exe", $"/select,\"{p}\"");

        }

        private async void ShowSettings(object obj)
        {
            try
            {
                var oldImagePaths = _settings.ImagePaths.ToList();

                var settings = new SettingsWindow(_dataStore, _settings);
                settings.Owner = this;
                settings.ShowDialog();

                if (_settings.IsDirty())
                {
                    _configuration.Save(_settings);

                    _search.Settings = _settings;
                    _prompts.Settings = _settings;

                    if (_settings.IsPropertyDirty(nameof(Settings.PageSize)))
                    {
                        ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);
                        _search.SetPageSize(_settings.PageSize);
                        _prompts.SetPageSize(_settings.PageSize);
                        _search.SearchImages();
                    }

                    if (_settings.IsPropertyDirty(nameof(Settings.ModelRootPath)) || _settings.IsPropertyDirty(nameof(Settings.HashCache)))
                    {
                        LoadModels();
                    }

                    if (_settings.IsPropertyDirty(nameof(Settings.Theme)))
                    {
                        UpdateTheme(_settings.Theme);
                    }


                    if (_settings.IsPropertyDirty(nameof(Settings.ImagePaths)))
                    {
                        if (oldImagePaths.Except(_settings.ImagePaths).Any())
                        {
                            _dataStore.CreateBackup();
                            CleanRemovedFoldersInternal();
                        }

                        LoadFolders();

                        await TryScanFolders();

                        // Rebuild watchers in case paths were added or removed

                        if (_settings.WatchFolders)
                        {
                            RemoveWatchers();
                            CreateWatchers();
                        }
                    }


                    if (_settings.IsPropertyDirty(nameof(Settings.WatchFolders)) || _settings.IsPropertyDirty(nameof(Settings.RecurseFolders)))
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

                    if (_settings.IsPropertyDirty(nameof(Settings.ExcludePaths)))
                    {
                        CleanExcludedPaths(_settings.ExcludePaths);
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

                    if (_settings.IsPropertyDirty(nameof(Settings.Culture)))
                    {
                        if (_settings.Culture == "default")
                        {
                            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentCulture;
                        }
                        else
                        {
                            LocalizeDictionary.Instance.Culture = new CultureInfo(_settings.Culture);
                        }
                    }

                    _settings.SetPristine();

                    ServiceLocator.SetSettings(_settings);

                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }


        private void UpdateTheme(string theme)
        {
            ThemeManager.ChangeTheme(theme);
        }


        private void OnNavigate(object sender, Page page)
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
            }
            else
            {
                _modelsCollection = new List<Model>();
            }

            if (!string.IsNullOrEmpty(_settings.HashCache))
            {
                try
                {
                    string text = File.ReadAllText(_settings.HashCache);

                    // Fix unquoted "NaN" strings, which sometimes show up in SafeTensor metadata.
                    text = text.Replace("NaN", "null");

                    var hashes = JsonSerializer.Deserialize<Hashes>(text);
                    var modelLookup = _modelsCollection.ToDictionary(m => m.Path);

                    var index = "checkpoint/".Length;

                    foreach (var hash in hashes.hashes)
                    {
                        if (index < hash.Key.Length)
                        {
                            var path = hash.Key.Substring(index);

                            if (modelLookup.TryGetValue(path, out var model))
                            {
                                model.SHA256 = hash.Value.sha256;
                            }
                            else
                            {
                                _modelsCollection.Add(new Model()
                                {
                                    Filename = Path.GetFileNameWithoutExtension(path),
                                    Path = path,
                                    SHA256 = hash.Value.sha256,
                                    IsLocal = true
                                });

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error loading JSON file '{_settings.HashCache}':\n{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                //foreach (var model in _modelsCollection.ToList())
                //{
                //    if (hashes.hashes.TryGetValue("checkpoint/" + model.Path, out var hash))
                //    {
                //        model.SHA256 = hash.sha256;
                //    }
                //}
            }

            var otherModels = new List<Model>();

            if (File.Exists("models.json"))
            {
                var json = File.ReadAllText(Path.Combine(AppDir, "models.json"));

                var options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };

                var civitAiModels = JsonSerializer.Deserialize<LiteModelCollection>(json, options);

                foreach (var model in civitAiModels.Models)
                {
                    foreach (var modelVersion in model.ModelVersions)
                    {
                        foreach (var versionFile in modelVersion.Files)
                        {
                            otherModels.Add(new Model()
                            {
                                Filename = Path.GetFileNameWithoutExtension(versionFile.Name),
                                Hash = versionFile.Hashes.AutoV1,
                                SHA256 = versionFile.Hashes.SHA256,
                            });
                        }
                    }

                }

            }

            _allModels = _modelsCollection.Concat(otherModels).ToList();



            _search.SetModels(_allModels);
            _models.SetModels(_modelsCollection);

            QueryBuilder.SetModels(_allModels);
        }

        private ICollection<Model> _allModels = new List<Model>();

        private async Task TryScanFolders()
        {
            if (_settings.ImagePaths.Any())
            {
                if (await _messagePopupManager.Show("Do you want to scan your folders now?", "Setup", PopupButtons.YesNo) == PopupResult.Yes)
                {
                    await Scan();
                }
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

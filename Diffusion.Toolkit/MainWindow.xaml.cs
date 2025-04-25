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
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Diffusion.Civitai.Models;
using WPFLocalizeExtension.Engine;
using Diffusion.Toolkit.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using Diffusion.Toolkit.Services;
using Diffusion.Toolkit.Common;
using Settings = Diffusion.Toolkit.Configuration.Settings;
using Diffusion.Database.Models;
using Image = System.Windows.Controls.Image;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BorderlessWindow
    {
        private readonly MainModel _model;
        private NavigatorService _navigatorService;

        private DataStore _dataStore => ServiceLocator.DataStore;
        private Settings _settings;



        private Configuration<Settings> _configuration;
        //private Toolkit.Settings? _settings;


        private Search _search;
        private Pages.Settings _settingsPage;
        private Pages.Models _models;
        private bool _tipsOpen;
        private MessagePopupManager _messagePopupManager;

        public MainWindow()
        {
            try
            {
                Logger.Log("===========================================");
                Logger.Log($"Started Diffusion Toolkit {AppInfo.Version}");


                _configuration = new Configuration<Settings>(AppInfo.SettingsPath, AppInfo.IsPortable);


                //Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");

                InitializeComponent();

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

                QueryBuilder.Samplers = File.ReadAllLines("samplers.txt").ToList();

                Logger.Log($"Creating Thumbnail loader");



                _navigatorService = new NavigatorService(this);
                _navigatorService.OnNavigate += OnNavigate;

                ServiceLocator.SetNavigatorService(_navigatorService);

                SystemEvents.UserPreferenceChanged += SystemEventsOnUserPreferenceChanged;

                ServiceLocator.WindowService.SetWindow(this);

                _model = new MainModel();
                _model.CurrentProgress = 0;
                _model.TotalProgress = 100;

                ServiceLocator.MainModel = _model;
                ServiceLocator.Dispatcher = Dispatcher;

                _model.RenameFileCommand = new AsyncCommand<string>((o) => RenameImageEntry());
                _model.OpenWithCommand = new AsyncCommand<string>((o) => OpenWith(this, o));
                _model.Rescan = new AsyncCommand<object>(RescanTask);
                _model.Rebuild = new AsyncCommand<object>(RebuildTask);

                _model.ReloadHashes = new AsyncCommand<object>(async (o) =>
                {
                    LoadModels();
                    await _messagePopupManager.Show("Models have been reloaded", "Diffusion Toolkit", PopupButtons.OK);
                });

                _model.SettingsCommand = new RelayCommand<object>(ShowSettings);

                _model.CancelCommand = new AsyncCommand<object>((o) => CancelProgress());
                _model.AboutCommand = new RelayCommand<object>((o) => ShowAbout());

                InitEvents();
                InitAlbums();
                InitQueries();

                _model.Refresh = new RelayCommand<object>((o) => Refresh());

                // TODO: Remove
                _model.QuickCopy = new RelayCommand<object>((o) =>
                {
                    var win = new QuickCopy();
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

                ServiceLocator.MessageService = new MessageService(_messagePopupManager);
                ServiceLocator.ToastService = new ToastService(ToastPopup);

                PreviewMouseDown += (sender, args) =>
                {
                    QueryPopup.IsOpen = false;
                };

                Deactivated += (sender, args) =>
                {
                    //QueryPopup.IsOpen = false;
                    //ToastPopup.IsOpen = false;
                    //_messagePopupManager.Cancel();
                };


                StateChanged += (sender, args) =>
                {
                    if (WindowState == WindowState.Minimized)
                    {
                        QueryPopup.IsOpen = false;
                        ToastPopup.IsOpen = false;
                        _messagePopupManager.Cancel();
                    }
                };

                // TODO: Find a better eay that doesn't get called even when mouseover?
                // Maybe only when the language has changed?
                Menu.LayoutUpdated += (sender, args) =>
                {
                    Menu.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    MenuWidth = new GridLength(Menu.DesiredSize.Width);
                };

                //Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-PT");
                //Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-PT");
                //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                //    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

                //var control = Application.Current.FindResource(typeof(Slider));
                //using (XmlTextWriter writer = new XmlTextWriter(@"slider.xaml", System.Text.Encoding.UTF8))
                //{
                //    writer.Formatting = Formatting.Indented;
                //    XamlWriter.Save(control, writer);
                //}

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

        private async Task UnavailableFiles(object o)
        {
            var window = new UnavailableFilesWindow();
            window.Owner = this;
            window.ShowDialog();

            if (window.DialogResult is true)
            {
                if (window.Model.RemoveImmediately)
                {
                    var result = await ServiceLocator.MessageService.Show("Are you sure you want to remove all unavailable files?", "Scan for Unavailable Images", PopupButtons.YesNo);
                    if (result == PopupResult.No)
                    {
                        return;
                    }
                }

                Task.Run(async () =>
                {
                    if (await ServiceLocator.ProgressService.TryStartTask())
                    {
                        try
                        {
                            await ServiceLocator.ScanningService.ScanUnavailable(window.Model, ServiceLocator.ProgressService.CancellationToken);
                        }
                        finally
                        {
                            ServiceLocator.ProgressService.CompleteTask();
                            ServiceLocator.ProgressService.SetStatus(GetLocalizedText("Actions.Scanning.Completed"));
                        }
                    }
                });

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
                case "Navigation.Queries":
                    _model.Settings.NavigationSection.ShowQueries = !_model.Settings.NavigationSection.ShowQueries;
                    break;
            }
        }

        private void ToggleAutoRefresh()
        {
            _settings.AutoRefresh = !_settings.AutoRefresh;
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

            _ = ServiceLocator.FolderService.LoadFolders();
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

                _previewWindow = new PreviewWindow();

                _previewWindow.WindowState = maximized ? WindowState.Maximized : WindowState.Normal;

                _previewWindow.Owner = this;

                // TODO: better implementation for StartNavigation events
                _previewWindow.PreviewKeyUp += _search.ExtOnKeyUp;
                _previewWindow.PreviewKeyDown += _search.ExtOnKeyDown;

                _previewWindow.AdvanceSlideShow = _search.Advance;

                _previewWindow.OnDrop = (s) => _search.LoadPreviewImage(s);
                //_previewWindow.Changed = (id) => _search.Update(id);

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
            ServiceLocator.ThumbnailService.Size = _settings.ThumbnailSize;
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
                _search.UpdateResults();
            }
            else if (e.PropertyName == nameof(MainModel.CurrentAlbum))
            {
                //Debug.WriteLine(_model.CurrentAlbum.Name);
                //_search.SetView("albums");
                //_search.SearchImages(null);
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataStore = new DataStore(AppInfo.DatabasePath);
            var _showReleaseNotes = false;

            ServiceLocator.SetDataStore(dataStore);

            var isFirstTime = false;
            IReadOnlyList<string> newFolders = null;

            if (!_configuration.Exists())
            {
                Logger.Log($"Opening Settings for first time");

                _settings = new Settings();

                UpdateTheme(_settings.Theme);

                var welcome = new WelcomeWindow(_settings);
                welcome.Owner = this;
                welcome.ShowDialog();

                newFolders = welcome.SelectedPaths;

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
                    _settings.UseBuiltInViewer ??= true;
                    _settings.SortAlbumsBy ??= "Name";
                    _settings.Theme ??= "System";

                    UpdateTheme(_settings.Theme);

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

            if (!SemanticVersion.TryParse(_settings.Version, out var semVer))
            {
                semVer = SemanticVersion.Parse("v0.0.0");
            }





            // TODO: Find a better place to put this:
            // Set defaults for new version features
            if (semVer < SemanticVersion.Parse("v1.9.0"))
            {
                _settings.ShowTags = true;
                _settings.NavigationSection.ShowFolders = true;
                _settings.NavigationSection.ShowAlbums = true;
                _settings.NavigationSection.ShowQueries = true;
                _settings.ConfirmDeletion = true;
                _settings.PermanentlyDelete = false;
            }


            if (semVer < AppInfo.Version)
            {
                _showReleaseNotes = true;
                _settings.Version = AppInfo.Version.ToString();
            }


            _settings.PropertyChanged += (s, args) =>
            {
                if (!_isClosing)
                {
                    OnSettingsChanged(args);
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
            _model.PermanentlyDelete = _settings.PermanentlyDelete;

            // TODO: Get rid of globals
            QueryBuilder.HideNSFW = _model.HideNSFW;
            QueryBuilder.HideDeleted = _model.HideDeleted;
            QueryBuilder.HideUnavailable = _model.HideUnavailable;

            _model.NSFWBlur = _settings.NSFWBlur;
            _model.FitToPreview = _settings.FitToPreview;
            _model.ActualSize = _settings.ActualSize;
            _model.AutoAdvance = _settings.AutoAdvance;
            _model.ShowTags = _settings.ShowTags;
            _model.ShowNotifications = _settings.ShowNotifications;
            _model.ShowFilenames = _settings.ShowFilenames;

            _model.Settings = _settings;

            //KeyDown += (o, args) =>
            //{
            //    IInputElement focusedControl = Keyboard.FocusedElement;
            //    IInputElement focusedControl2 = FocusManager.GetFocusedElement(this);
            //};

            Activated += OnActivated;
            StateChanged += OnStateChanged;
            SizeChanged += OnSizeChanged;
            LocationChanged += OnLocationChanged;

            ServiceLocator.SetSettings(_settings);

            Logger.Log($"Initializing pages");


            await dataStore.Create(
                () => Dispatcher.Invoke(() => _messagePopupManager.ShowMessage("Please wait while we update your database", "Updating Database")),
                (handle) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ((MessagePopupHandle)handle).CloseAsync();
                        //ServiceLocator.MessageService.CloseHandle((MessagePopupHandle)handle);
                    });
                }
            );



            //var total = _dataStore.GetTotal();

            //var text = GetLocalizedText("Main.Status.ImagesInDatabase").Replace("{count}", $"{total:n0}");

            //_model.Status = text;
            //_model.TotalProgress = 100;

            _models = new Pages.Models();

            _models.OnModelUpdated = (model) =>
            {
                var updatedModel = _modelsCollection.FirstOrDefault(m => m.Path == model.Path);
                if (updatedModel != null)
                {
                    updatedModel.SHA256 = model.SHA256;
                    _search.SetModels(_modelsCollection);
                }
            };

            _search = new Search();

            _search.MoveFiles = (files) =>
            {
                using var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
                {
                    var path = dialog.FileName;

                    var isInPath = false;

                    // TODO: Fix
                    throw new NotImplementedException("Too Lazy to fix");

                    // Check if the target path is in the list of selected diffusion folders

                    //if (_settings.RecurseFolders.GetValueOrDefault(true))
                    //{
                    //    foreach (var folder in ServiceLocator.FolderService.RootFolders)
                    //    {
                    //        if (path.StartsWith(folder.Path, true, CultureInfo.InvariantCulture))
                    //        {
                    //            isInPath = true;
                    //            break;
                    //        }
                    //    }

                    //    // Now check if the path falls under one of the excluded paths.

                    //    foreach (var folder in ServiceLocator.FolderService.ExcludedFolders)
                    //    {
                    //        if (path.StartsWith(folder.Path, true, CultureInfo.InvariantCulture))
                    //        {
                    //            isInPath = false;
                    //            break;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    // If recursion is turned off, the path must specifically equal one of the diffusion folders

                    //    foreach (var imagePath in ServiceLocator.FolderService.RootFolders.Select(d => d.Path))
                    //    {
                    //        if (path.Equals(imagePath, StringComparison.InvariantCultureIgnoreCase))
                    //        {
                    //            isInPath = true;
                    //            break;
                    //        }
                    //    }
                    //}

                    Task.Run(async () =>
                    {
                        await ServiceLocator.FileService.MoveFiles(files.Select(d => new ImagePath() { Id = d.Id, Path = d.Path }).ToList(), path, !isInPath);

                        if (!isInPath)
                        {
                            _search.SearchImages();
                        }
                    });

                }

            };

            _prompts = new Prompts();
            _settingsPage = new Pages.Settings(this);


            _model.ThumbnailSize = _settings.ThumbnailSize;
            _model.ThumbnailViewMode = _settings.ThumbnailViewMode;

            _search.SetThumbnailSize(_settings.ThumbnailSize);
            _search.SetPageSize(_settings.PageSize);

            _prompts.SetThumbnailSize(_settings.ThumbnailSize);
            _prompts.SetPageSize(_settings.PageSize);

            _search.OnPopout = () => PopoutPreview(true, true, false);
            _search.OnCurrentImageOpen = OnCurrentImageOpen;

            _model.GotoUrl = new RelayCommand<string>((url) =>
            {
                _navigatorService.Goto(url);
            });

            _navigatorService.OnNavigate += (o, args) =>
            {
                _model.ActiveView = args.TargetUri.Path.ToLower() switch
                {
                    "search" when args.TargetUri.Fragment != null => args.TargetUri.Fragment.ToLower() switch
                    {
                        "favorites" => "Favorites",
                        "deleted" => "For Deletion",
                        "images" => "Diffusions",
                        "folders" => "Folders",
                        "albums" => "Albums",
                        _ => _model.ActiveView
                    },
                    "search" => "Diffusions",
                    "models" => "Models",
                    "prompts" => "Prompts",
                    "settings" => "Settings",
                    _ => _model.ActiveView
                };
            };


            ServiceLocator.FolderService.CreateWatchers();

            //_navigatorService.Goto("search");

            Logger.Log($"Loading models");

            LoadAlbums();
            LoadQueries();
            LoadModels();
            LoadImageModels();
            await InitFolders();

            ServiceLocator.ContextMenuService.Go();

            Logger.Log($"{_modelsCollection.Count} models loaded");

            Logger.Log($"Starting Services...");

            ServiceLocator.ThumbnailService.Size = _settings.ThumbnailSize;

            _ = ServiceLocator.ThumbnailService.StartAsync();

            if (_settings.CheckForUpdatesOnStartup)
            {
                _ = Task.Run(async () =>
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
                });
            }


            if (isFirstTime)
            {
                // Automatically scans images...
                await ServiceLocator.FolderService.ApplyFolderChanges(newFolders.Select(d => new FolderChange()
                {
                    ChangeType = ChangeType.Add,
                    FolderType = FolderType.Root,
                    Watched = true,
                    Recursive = true,
                    Path = d,
                }), confirmScan: false).ContinueWith(d =>
                {
                    ServiceLocator.NavigatorService.Goto("search");

                    if (ServiceLocator.FolderService.HasRootFolders)
                    {
                        // Wait for a bit, then show thumbnails
                        _ = Task.Delay(5000).ContinueWith(t =>
                        {
                            ServiceLocator.SearchService.ExecuteSearch();
                            ServiceLocator.MessageService.ShowMedium(GetLocalizedText("FirstScan.Message"),
                                GetLocalizedText("FirstScan.Title"), PopupButtons.OK);
                        });
                    }

                });
            }
            else
            {
                ServiceLocator.NavigatorService.Goto("search");

                if (ServiceLocator.FolderService.HasRootFolders)
                {
                    if (_settings.ScanForNewImagesOnStartup)
                    {
                        Logger.Log($"Scanning for new images");

                        _ = Task.Run(async () =>
                        {
                            if (await ServiceLocator.ProgressService.TryStartTask())
                            {
                                await ServiceLocator.ScanningService.ScanWatchedFolders(false, false, ServiceLocator.ProgressService.CancellationToken);
                            }
                        });
                    }
                }
            }

            Logger.Log($"Init completed");

            if (_showReleaseNotes)
            {
                ShowReleaseNotes();
            }
            // Cleanup();
        }

        private async Task Cleanup()
        {
            await CleanRemovedFoldersInternal();
        }

        //private void NavigatorServiceOnOnNavigate(object? sender, NavigateEventArgs e)
        //{
        //    if (e.CurrentUrl == "settings")
        //    {
        //        var changes = _settingsPage.ApplySettings();

        //        _ = ExecuteFolderChanges(changes);
        //    }
        //}


        private void OnCurrentImageOpen(ImageViewModel obj)
        {
            //if (obj == null) return;
            var p = obj.Path;

            ServiceLocator.FileService.UpdateViewed(obj.Id);

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
            _navigatorService.Goto("settings");
        }


        private void UpdateTheme(string theme)
        {
            ThemeManager.ChangeTheme(theme);
        }


        private void OnNavigate(object sender, NavigateEventArgs args)
        {
            _model.Page = args.TargetPage;
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

                try
                {
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
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
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
            if (ServiceLocator.FolderService.HasRootFolders)
            {
                if (await ServiceLocator.MessageService.Show("Do you want to scan your folders now?", "Setup", PopupButtons.YesNo) == PopupResult.Yes)
                {
                    await Scan();
                }
            }
            else
            {
                await ServiceLocator.MessageService.ShowMedium("You have not setup any image folders.\r\n\r\nAdd one or more folders first, then click the Scan Folders for new images icon in the toolbar.", "Setup", PopupButtons.OK);
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

using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using Diffusion.Toolkit.Classes;
using Search = Diffusion.Toolkit.Pages.Search;
using Diffusion.Toolkit.Thumbnails;
using Diffusion.IO;
using System.Threading;
using System.Threading.Tasks;
using Image = Diffusion.Database.Image;
using Diffusion.Toolkit.Themes;
using Microsoft.Win32;
using Diffusion.Toolkit.Pages;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Model = Diffusion.IO.Model;
using Timer = System.Threading.Timer;
using Diffusion.Updater;

namespace Diffusion.Toolkit
{
    public static class AppInfo
    {
        public static SemanticVersion Version => SemanticVersionHelper.GetLocalVersion();
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainModel _model;
        private readonly DataStore _dataStore;
        private NavigatorService _navigatorService;
        private string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiffusionToolkit");
        private Configuration<Settings> _configuration = new("DiffusionToolkit");
        private Settings? _settings = new Settings();
        private CancellationTokenSource _scanCancellationTokenSource = new CancellationTokenSource();

        private Search _search;
        private Pages.Models _models;
        private bool _tipsOpen;
        private MessagePopupManager _messagePopupManager;

        public MainWindow()
        {
            Logger.Log("===========================================");
            Logger.Log($"Started Diffusion Toolkit {AppInfo.Version}");

            InitializeComponent();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            QueryBuilder.Samplers = File.ReadAllLines("samplers.txt").ToList();

            Logger.Log($"Creating Thumbnail loader");

            ThumbnailLoader.CreateInstance(Dispatcher);

            _navigatorService = new NavigatorService(this)
            {
                OnNavigate = OnNavigate
            };

            SystemEvents.UserPreferenceChanged += SystemEventsOnUserPreferenceChanged;

            var dbPath = Path.Combine(AppDataPath, "diffusion-toolkit.db");

            Logger.Log($"Opening database at {dbPath}");

            _dataStore = new DataStore(dbPath);

            _model = new MainModel();
            _model.Rescan = new AsyncCommand(RescanTask);
            _model.Rebuild = new AsyncCommand(RebuildTask);
            _model.RemoveMarked = new RelayCommand<object>(RemoveMarked);
            _model.Settings = new RelayCommand<object>(ShowSettings);
            _model.CancelScan = new AsyncCommand(CancelScan);
            _model.About = new RelayCommand<object>((o) => ShowAbout());
            _model.Help = new RelayCommand<object>((o) => ShowTips());
            _model.ToggleInfo = new RelayCommand<object>((o) => ToggleInfo());
            _model.ToggleNSFWBlur = new RelayCommand<object>((o) => ToggleNSFWBlur());
            _model.ToggleHideNSFW = new RelayCommand<object>((o) => ToggleHideNSFW());

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

            //var str = new System.Text.StringBuilder();
            //using (var writer = new System.IO.StringWriter(str))
            //    System.Windows.Markup.XamlWriter.Save(EditMenu.Template, writer);
            //System.Diagnostics.Debug.Write(str);
        }

        private void ToggleHideNSFW()
        {
            _model.HideNSFW = !_model.HideNSFW;
            QueryBuilder.HideNFSW = _model.HideNSFW;
            _settings.HideNSFW = _model.HideNSFW;
            _search.SearchImages();
        }

        private void ToggleNSFWBlur()
        {
            _model.NSFWBlur = !_model.NSFWBlur;
            _settings.NSFWBlur = _model.NSFWBlur;
            _search.SetNSFWBlur(_model.NSFWBlur);
        }

        private void MyHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var message = ((Exception)e.ExceptionObject).Message;

            Logger.Log($"An unhandled exception occured: {message}");

            MessageBox.Show(this, message, "An unhandled exception occured", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void ToggleInfo()
        {
            _search.ToggleInfo();
        }

        private void SystemEventsOnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
            {
                UpdateTheme();
            }
        }

        private void ShowAbout()
        {
            var welcome = new WelcomeWindow(_settings!);
            welcome.Owner = this;
            welcome.ShowDialog();
        }

        private void ShowTips()
        {
            if (!_tipsOpen)
            {
                var tipsWindow = new TipsWindow();
                tipsWindow.Owner = this;
                tipsWindow.Show();
                _tipsOpen = true;
                tipsWindow.Closed += (sender, args) =>
                {
                    _tipsOpen = false;
                };
            }
        }

        private async Task CancelScan()
        {
            var dialogResult = await _messagePopupManager.Show("Are you sure you want to cancel the operation?", "Cancel", PopupButtons.YesNo);

            if (dialogResult == PopupResult.Yes)
            {
                _scanCancellationTokenSource.Cancel();
            }
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            _settings.WindowState = this.WindowState;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _settings.WindowSize = e.NewSize;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (_settings.IsDirty)
            {
                _configuration.Save(_settings);
            }
        }

        private void RemoveMarked(object obj)
        {
            if (_model.IsScanning)
            {
                return;
            }

            //var files = new List<ImagePath>();

            //for (var i = 1; i <= 100; i++)
            //{
            //    files.Add(new ImagePath() { Id = i, Path = $"File{i:000}.png" });
            //}

            var files = _dataStore.GetMarkedImagePaths().ToList();
            var count = 0;

            if (files.Count == 0)
            {
                _messagePopupManager.Show("There are no files to delete", "Empty recycle bin");
                return;
            }

            _messagePopupManager.Show("This will delete the files from your hard drive! Are you sure you want to continue?", "Empty recycle bin", PopupButtons.YesNo).ContinueWith(t =>
            {
                if (t.Result == PopupResult.Yes)
                {
                    Task.Run(async () =>
                    {
                        _model.IsScanning = true;

                        try
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _model.TotalFilesScan = files.Count;
                                _model.CurrentPositionScan = 0;
                            });

                            foreach (var imagePath in files)
                            {

                                if (_scanCancellationTokenSource.IsCancellationRequested) break;

                                try
                                {
                                    count++;

                                    var path = Path.GetFileName(imagePath.Path);

                                    Dispatcher.Invoke(() =>
                                    {
                                        _model.Status = $"Deleting {path}...";
                                        _model.CurrentPositionScan = count;
                                    });

                                    //await Task.Delay(50);

                                    File.Delete(imagePath.Path);
                                    var dir = Path.GetDirectoryName(imagePath.Path);
                                    var fileName = Path.GetFileNameWithoutExtension(imagePath.Path);
                                    var textFilePath = Path.Join(dir, $"{fileName}.txt");

                                    File.Delete(imagePath.Path);
                                    if (File.Exists(textFilePath))
                                    {
                                        File.Delete(textFilePath);
                                    }

                                    _dataStore.DeleteImage(imagePath.Id);

                                }
                                catch (Exception e)
                                {
                                    var result = await Dispatcher.Invoke(async () =>
                                    {
                                        return await _messagePopupManager.Show($"Failed to delete {imagePath.Path}. \n\n {e.Message}", "Error", PopupButtons.OkCancel);
                                    });

                                    if (result == PopupResult.Yes)
                                    {
                                        break;
                                    }

                                }
                            }

                            Dispatcher.Invoke(() =>
                            {
                                _model.TotalFilesScan = 100;
                                _model.CurrentPositionScan = 0;

                                _messagePopupManager.Show($"{count} images were deleted", "Delete images");
                            });

                        }
                        finally
                        {
                            _model.IsScanning = false;

                            SetTotalFilesStatus();

                            await _search.ReloadMatches();

                        }
                    });
                }
            });


            //if (MessageBox.Show(this, "This will delete the files from your hard drive! Are you sure you want to continue?", "Empty recycle bin", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            //{


            //}
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

                _settings = new Settings();

                UpdateTheme();

                var welcome = new WelcomeWindow(_settings);
                welcome.Owner = this;
                welcome.ShowDialog();

                var settings = new SettingsWindow(_dataStore, _settings);
                settings.Owner = this;
                settings.ShowDialog();

                if (_settings.IsDirty)
                {
                    _configuration.Save(_settings);
                    _settings.SetPristine();
                }

                ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);


                if (_settings.ImagePaths.Any())
                {
                    if (await _messagePopupManager.Show("Do you want Diffusion Toolkit to scan your configured folders now?", "Setup", PopupButtons.YesNo) == PopupResult.Yes)
                    {
                        await Scan();
                    };
                }
                else
                {
                    await _messagePopupManager.ShowMedium("You have not setup any image folders. You will not be able to search for anything yet.\r\n\r\nAdd folders first, then click the Rescan Folders icon in the toolbar to scan your images.", "Setup", PopupButtons.OK);
                }
            }
            else
            {
                UpdateTheme();

                if (!_settings.DontShowWelcomeOnStartup)
                {
                    var welcome = new WelcomeWindow(_settings);
                    welcome.Owner = this;
                    welcome.ShowDialog();
                }

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

            Activated += OnActivated;
            StateChanged += OnStateChanged;
            SizeChanged += OnSizeChanged;

            Logger.Log($"Initializing pages");

            _models = new Pages.Models(_dataStore, _settings);
            _search = new Search(_navigatorService, _dataStore, _settings);
            _search.SetNSFWBlur(_model.NSFWBlur);

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
            _model.ShowModels = new RelayCommand<object>((o) => _navigatorService.Goto("models"));

            if (_settings.WatchFolders)
            {
                foreach (var path in _settings.ImagePaths)
                {
                    var watcher = new FileSystemWatcher(path)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = true,
                    };
                    watcher.Created += WatcherOnCreated;
                    _watchers.Add(watcher);
                }
            }


            var pages = new Dictionary<string, Page>()
            {
                { "search", _search },
                { "models", _models },
                //{ "config", _configPage},
                //{ "setup", new SetupPage(_navigatorService) },
            };

            _navigatorService.SetPages(pages);

            _navigatorService.Goto("search");

            Logger.Log($"Loading models");

            LoadModels();

            Logger.Log($"{_modelsCollection.Count} models loaded");

            _search.SetModels(_modelsCollection);

            if (_settings.CheckForUpdatesOnStartup)
            {
                var checker = new UpdateChecker();

                Logger.Log($"Checking for latest version");

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

            if (_settings.ScanForNewImagesOnStartup)
            {
                Logger.Log($"Scanning for new images");

                await ScanInternal(_settings.ImagePaths, false, false);
            }

            Logger.Log($"Init completed");

        }

        private async void OnActivated(object? sender, EventArgs e)
        {

            if (addedTotal > 0)
            {
                await Report(addedTotal, 0, 0, false);
                lock (_lock)
                {
                    addedTotal = 0;
                }
            }

        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath)) > -1)
                {
                    AddFile(e.FullPath);
                }
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath)) > -1)
                {
                    AddFile(e.FullPath);
                }
            }
        }

        private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

        private List<string> detectedFiles;

        private Timer? t = null;
        private object _lock = new object();

        private void AddFile(string path)
        {
            lock (_lock)
            {
                if (t == null)
                {
                    detectedFiles = new List<string>();
                    t = new Timer(Callback, null, 2000, Timeout.Infinite);
                }
                else
                {
                    t.Change(2000, Timeout.Infinite);
                }
                detectedFiles.Add(path);
            }
        }

        private int addedTotal = 0;

        private async void Callback(object? state)
        {
            int added;
            float elapsed;

            lock (_lock)
            {
                t?.Dispose();
                t = null;
                (added, elapsed) = ScanFiles(detectedFiles.ToList(), false);
            }

            if (added > 0)
            {
                await Dispatcher.Invoke(async () =>
                {
                    var currentWindow = Application.Current.Windows.OfType<Window>().First();
                    if (currentWindow.IsActive)
                    {
                        await Report(added, 0, elapsed, false);
                    }
                    else
                    {
                        lock (_lock)
                        {
                            addedTotal += added;
                        }
                    }
                });


            }

        }


        private void ShowSettings(object obj)
        {
            try
            {
                var settings = new SettingsWindow(_dataStore, _settings);
                settings.Owner = this;
                settings.ShowDialog();

                if (_settings.IsDirty)
                {
                    _configuration.Save(_settings);

                    _search.Settings = _settings;

                    if (_settings.IsPropertyDirty(nameof(Settings.PageSize)))
                    {
                        ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);
                        _search.SearchImages();
                    }

                    if (_settings.IsPropertyDirty(nameof(Settings.ModelRootPath)))
                    {
                        LoadModels();
                        _search.SetModels(_modelsCollection);
                    }

                    if (_settings.IsPropertyDirty(nameof(Settings.Theme)))
                    {
                        UpdateTheme();
                    }

                    if (_settings.IsPropertyDirty(nameof(Settings.WatchFolders)))
                    {
                        if (_watchers != null)
                        {
                            foreach (var watcher in _watchers)
                            {
                                watcher.Dispose();
                            }
                            _watchers.Clear();
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

        private async Task RescanTask()
        {
            if (_settings.ImagePaths.Any())
            {
                await Scan();
            }
            else
            {
                await _messagePopupManager.Show("No image paths configured!", "Rescan Folders");
            }
        }


        private async Task RebuildTask()
        {
            if (_settings.ImagePaths.Any())
            {
                var message = "This will update the metadata in the database with newly scanned metadata from the files.\r\n\r\n" +
                              "You only need to do this if you think you're missing some metadata.\r\n\r\n" +
                              "Are you sure you want to continue?";

                var result = await _messagePopupManager.ShowMedium(message, "Rebuild Images", PopupButtons.YesNo);
                if (result == PopupResult.Yes)
                {
                    await Rebuild();
                }
            }
            else
            {
                await _messagePopupManager.Show("No image paths configured!", "Rebuild Images");
            }
        }

        private void OnNavigate(Page page)
        {
            _model.Page = page;
        }


        private async Task Scan()
        {
            await Task.Run(async () =>
            {
                var result = await ScanInternal(_settings.ImagePaths, false);
                if (result && _search != null)
                {
                    _search.SearchImages();
                }
            });
        }

        private async Task Rebuild()
        {
            await Task.Run(async () =>
            {
                var result = await ScanInternal(_settings.ImagePaths, true);
                if (result)
                {
                    _search.SearchImages();
                }
            });
        }

        private (int, float) ScanFiles(IList<string> filesToScan, bool updateImages)
        {
            var added = 0;
            var scanned = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var max = filesToScan.Count;

            Dispatcher.Invoke(() =>
            {
                _model.TotalFilesScan = max;
                _model.CurrentPositionScan = 0;
            });

            var newImages = new List<Image>();

            foreach (var file in Scanner.Scan(filesToScan))
            {
                if (_scanCancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                scanned++;

                if (file != null)
                {
                    var image = new Image()
                    {
                        Prompt = file.Prompt,
                        NegativePrompt = file.NegativePrompt,
                        Path = file.Path,
                        Width = file.Width,
                        Height = file.Height,
                        ModelHash = file.ModelHash,
                        Steps = file.Steps,
                        Sampler = file.Sampler,
                        CFGScale = file.CFGScale,
                        Seed = file.Seed,
                        BatchPos = file.BatchPos,
                        BatchSize = file.BatchSize,
                        CreatedDate = File.GetCreationTime(file.Path),
                        AestheticScore = file.AestheticScore,
                        HyperNetwork = file.HyperNetwork,
                        HyperNetworkStrength = file.HyperNetworkStrength,
                        ClipSkip = file.ClipSkip,
                        FileSize = file.FileSize,
                    };

                    if (!string.IsNullOrEmpty(file.HyperNetwork) && !file.HyperNetworkStrength.HasValue)
                    {
                        file.HyperNetworkStrength = 1;
                    }

                    newImages.Add(image);
                }

                if (newImages.Count == 100)
                {
                    if (updateImages)
                    {
                        _dataStore.UpdateImagesByPath(newImages);
                    }
                    else
                    {
                        _dataStore.AddImages(newImages);
                    }

                    added += newImages.Count;
                    newImages.Clear();
                }

                if (scanned % 33 == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _model.CurrentPositionScan = scanned;
                        _model.Status = $"Scanning {_model.CurrentPositionScan:#,###,###} of {_model.TotalFilesScan:#,###,###}...";
                    });
                }
            }

            if (newImages.Count > 0)
            {
                if (updateImages)
                {
                    _dataStore.UpdateImagesByPath(newImages);
                }
                else
                {
                    _dataStore.AddImages(newImages);
                }
                added += newImages.Count;
            }

            Dispatcher.Invoke(() =>
            {
                _model.Status = $"Scanning {_model.TotalFilesScan:#,###,###} of {_model.TotalFilesScan:#,###,###}...";
                _model.TotalFilesScan = Int32.MaxValue;
                _model.CurrentPositionScan = 0;
            });

            stopwatch.Stop();

            var elapsedTime = stopwatch.ElapsedMilliseconds / 1000f;


            return (added, elapsedTime);
        }


        private async Task<bool> ScanInternal(IEnumerable<string> paths, bool updateImages, bool reportIfNone = true)
        {
            if (_model.IsScanning) return false;

            _model.IsScanning = true;

            _scanCancellationTokenSource = new CancellationTokenSource();
            var removed = 0;
            var added = 0;

            try
            {
                var existingImages = _dataStore.GetImagePaths().ToList();

                var removedList = existingImages.Where(img => !File.Exists(img.Path)).ToList();

                if (removedList.Any())
                {
                    removed = removedList.Count;
                    _dataStore.DeleteImages(removedList.Select(i => i.Id));
                }

                var filesToScan = new List<string>();

                foreach (var path in paths)
                {
                    if (_scanCancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    var ignoreFiles = updateImages ? null : existingImages.Where(p => p.Path.StartsWith(path)).Select(p => p.Path).ToHashSet();

                    filesToScan.AddRange(Scanner.GetFiles(path, _settings.FileExtensions, ignoreFiles).ToList());
                }

                var (_added, elapsedTime) = ScanFiles(filesToScan, updateImages);

                added = _added;

                if ((added + removed == 0 && reportIfNone) || added + removed > 0)
                {
                    await Report(added, removed, elapsedTime, updateImages);
                }
            }
            catch (Exception ex)
            {
                await _messagePopupManager.ShowMedium(
                    ex.Message,
                    "Scan Error", PopupButtons.OK);
            }
            finally
            {
                _model.IsScanning = false;

            }


            return added + removed > 0;
        }

        private async Task Report(int added, int removed, float elapsedTime, bool updateImages)
        {
            await Dispatcher.Invoke(async () =>
            {
                if (added == 0 && removed == 0)
                {
                    await _messagePopupManager.Show($"No new images found", "Scan Complete");
                }
                else
                {
                    var newOrOpdated = updateImages ? $"{added:#,###,##0} images updated" : $"{added:#,###,##0} new images added";

                    var missing = removed > 0 ? $"{removed:#,###,##0} missing images removed" : string.Empty;

                    var messages = new[] { newOrOpdated, missing };

                    var message = string.Join("\n", messages.Where(m => !string.IsNullOrEmpty(m)));

                    message = $"{message}";

                    if (updateImages)
                    {
                        await _messagePopupManager.Show(message, "Rebuild Complete");
                    }
                    else
                    {
                        await _messagePopupManager.Show(message, "Scan Complete", 10);
                    }
                }

                SetTotalFilesStatus();
            });

        }

        private void SetTotalFilesStatus()
        {
            var total = _dataStore.GetTotal();
            _model.Status = $"{total:###,###,##0} images in database";
        }

        private ICollection<Model> _modelsCollection;

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

        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            CallUpdater();
        }

        private void FileCopy(string filename, string target)
        {
            File.Copy(filename, Path.Join(target, filename), true);
        }

        private void CallUpdater()
        {
            Logger.Log($"Calling updater...");

            var updaterExe = "Diffusion.Updater.exe";

            var path = "Updater";

            var temp = Path.Join(path, updaterExe);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            FileCopy(updaterExe, path);
            FileCopy("Diffusion.Updater.deps.json", path);
            FileCopy("Diffusion.Updater.dll", path);
            FileCopy("Diffusion.Updater.runtimeconfig.json", path);


            Process.Start(temp, System.AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}


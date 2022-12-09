using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using Diffusion.Toolkit.Controls;
using Image = Diffusion.Database.Image;
using Diffusion.Toolkit.Themes;
using Microsoft.Win32;
using Diffusion.Toolkit.Pages;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Panel = System.Windows.Controls.Panel;

namespace Diffusion.Toolkit
{
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

            InitializeComponent();

            QueryBuilder.Samplers = File.ReadAllLines("samplers.txt").ToList();

            ThumbnailLoader.CreateInstance(Dispatcher);

            _navigatorService = new NavigatorService(this)
            {
                OnNavigate = OnNavigate
            };

            SystemEvents.UserPreferenceChanged += SystemEventsOnUserPreferenceChanged;

            _dataStore = new DataStore(Path.Combine(AppDataPath, "diffusion-toolkit.db"));

            _model = new MainModel();
            _model.Rescan = new AsyncCommand(RescanTask);
            _model.Rebuild = new AsyncCommand(RebuildTask);
            _model.RemoveMarked = new RelayCommand<object>(RemoveMarked);
            _model.Settings = new RelayCommand<object>(ShowSettings);
            _model.CancelScan = new AsyncCommand(CancelScan);
            _model.About = new RelayCommand<object>((o) => ShowAbout());
            _model.Help = new RelayCommand<object>((o) => ShowTips());

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


            _messagePopupManager = new MessagePopupManager(PopupHost, Frame, Dispatcher);

            //var str = new System.Text.StringBuilder();
            //using (var writer = new System.IO.StringWriter(str))
            //    System.Windows.Markup.XamlWriter.Save(EditMenu.Template, writer);
            //System.Diagnostics.Debug.Write(str);
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
                    if (MessageBox.Show("Do you want Diffusion Toolkit to scan your configured folders now?", "Setup", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        await Scan();
                    };
                }
                else
                {
                    MessageBox.Show("You have not setup any image folders. You will not be able to search anything yet. Add folders, then click the Rescan Folders icon after you have set them up.", "Setup", MessageBoxButton.OK, MessageBoxImage.Information);
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



            StateChanged += OnStateChanged;
            SizeChanged += OnSizeChanged;

            _models = new Pages.Models(_dataStore, _settings);
            _search = new Search(_navigatorService, _dataStore, _settings);

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



            var pages = new Dictionary<string, Page>()
            {
                { "search", _search },
                { "models", _models },
                //{ "config", _configPage},
                //{ "setup", new SetupPage(_navigatorService) },
            };

            _navigatorService.SetPages(pages);

            _navigatorService.Goto("search");



        }

        private void ShowSettings(object obj)
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
                    _search.LoadModels();
                }

                if (_settings.IsPropertyDirty(nameof(Settings.Theme)))
                {
                    UpdateTheme();
                }

                _settings.SetPristine();

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
                if (result)
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

        private async Task<bool> ScanInternal(IEnumerable<string> paths, bool updateImages)
        {
            if (_model.IsScanning) return false;

            _model.IsScanning = true;

            _scanCancellationTokenSource = new CancellationTokenSource();
            var added = 0;
            var removed = 0;
            try
            {
                var scanned = 0;

                var scanner = new Scanner(_settings.FileExtensions);

                var existingImages = _dataStore.GetImagePaths().ToList();

                HashSet<string> ignoreFiles = updateImages ? new HashSet<string>() : existingImages.Select(p => p.Path).ToHashSet();

                var removedList = existingImages.Where(img => !File.Exists(img.Path)).ToList();

                if (removedList.Any())
                {
                    removed = removedList.Count;
                    _dataStore.DeleteImages(removedList.Select(i => i.Id));
                }

                foreach (var path in paths)
                {
                    if (_scanCancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    var max = scanner.Count(path);

                    Dispatcher.Invoke(() =>
                    {
                        _model.TotalFilesScan = max;
                        _model.CurrentPositionScan = 0;
                    });


                    //scanned += images.Count();

                    var files = scanner.Scan(path, ignoreFiles);

                    var newImages = new List<Image>();

                    foreach (var file in files)
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
                            };

                            if (!string.IsNullOrEmpty(file.HyperNetwork) && !file.HyperNetworkStrength.HasValue)
                            {
                                file.HyperNetworkStrength = 1;
                            }

                            newImages.Add(image);



                        }

                        if (newImages.Count == 50)
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

                        if (scanned % 51 == 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _model.CurrentPositionScan += 51;
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
                }

                await Dispatcher.Invoke(async () =>
                {
                    if (added == 0 && removed == 0)
                    {
                        await _messagePopupManager.Show("No new images found", "Scan Complete");
                    }
                    else
                    {
                        var newOrOpdated = updateImages ? $"{added:#,###,##0} images updated" : $"{added:#,###,##0} new images added";

                        var missing = removed > 0 ? $"{removed:#,###,##0} missing images removed" : string.Empty;

                        var messages = new[] { newOrOpdated, missing };

                        var message = string.Join("\n", messages.Where(m => !string.IsNullOrEmpty(m)));

                        await _messagePopupManager.Show(message, updateImages ? "Rebuild Complete" : "Scan Complete");
                    }

                    SetTotalFilesStatus();
                });
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

        private void SetTotalFilesStatus()
        {
            var total = _dataStore.GetTotal();
            _model.Status = $"{total:###,###,##0} images in database";
        }


    }

    public class MessagePopupManager
    {
        private readonly Panel _host;
        private readonly UIElement _placementTarget;
        private readonly Dispatcher _dispatcher;

        public MessagePopupManager(Panel host, UIElement placementTarget, Dispatcher dispatcher)
        {
            _host = host;
            _placementTarget = placementTarget;
            _dispatcher = dispatcher;
        }

        public Task<PopupResult> Show(string message, string title)
        {
            _host.Visibility = Visibility.Visible;
            var popup = new MessagePopup(_placementTarget);
            _host.Children.Add(popup);
            return popup.Show(message, title)
                .ContinueWith(t =>
                {
                    _dispatcher.Invoke(() =>
                    {
                        _host.Visibility = Visibility.Hidden;
                    });
                return t.Result;
            });
        }

        public Task<PopupResult> Show(string message, string title, PopupButtons buttons)
        {
            _host.Visibility = Visibility.Visible;
            var popup = new MessagePopup(_placementTarget);
            _host.Children.Add(popup);
            return popup.Show(message, title, buttons)
                .ContinueWith(t =>
                {
                    _dispatcher.Invoke(() =>
                    {
                        _host.Visibility = Visibility.Hidden;
                    });
                    return t.Result;
                });
        }

        public Task<PopupResult> ShowMedium(string message, string title, PopupButtons buttons)
        {
            _host.Visibility = Visibility.Visible;
            var popup = new MessagePopup(_placementTarget);
            _host.Children.Add(popup);
            return popup.ShowMedium(message, title, buttons)
                .ContinueWith(t =>
                {
                    _dispatcher.Invoke(() =>
                    {
                        _host.Visibility = Visibility.Hidden;
                    });
                    return t.Result;
                });
        }
    }

}

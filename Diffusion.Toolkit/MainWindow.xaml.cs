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
using System.Configuration;
using Accessibility;
using Diffusion.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;
using Image = Diffusion.Database.Image;
using System.Windows.Shapes;

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

        public MainWindow()
        {
        
            InitializeComponent();

            QueryBuilder.Samplers = File.ReadAllLines("samplers.txt").ToList();

            ThumbnailLoader.CreateInstance(Dispatcher);

            _navigatorService = new NavigatorService(this)
            {
                OnNavigate = OnNavigate
            };



            _dataStore = new DataStore(Path.Combine(AppDataPath, "diffusion-toolkit.db"));

            _model = new MainModel();
            _model.Rescan = new RelayCommand<object>(Rescan);
            _model.Rebuild = new RelayCommand<object>(Rebuild);
            _model.RemoveMarked = new RelayCommand<object>(RemoveMarked);
            _model.Settings = new RelayCommand<object>(ShowSettings);
            _model.CancelScan = new RelayCommand<object>((o) => CancelScan());
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
        }

        private void ShowAbout()
        {
            var welcome = new WelcomeWindow(_settings);
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

        private void CancelScan()
        {
            var dialogResult = MessageBox.Show(_navigatorService.Host, "Are you sure you want to cancel the operation?", "Cancel", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (dialogResult == MessageBoxResult.Yes)
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

            var files = new List<ImagePath>();

            for (var i = 1; i <= 100; i++)
            {
                files.Add(new ImagePath() { Id = i, Path = $"File{i:000}.png" });
            }

            //var files = _dataStore.GetMarkedImagePaths().ToList();
            var count = 0;

            if (files.Count == 0)
            {
                MessageBox.Show(this, "There are no files to delete", "Empty recycle bin", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show(this, "This will delete the files from your hard drive! Are you sure you want to continue?", "Empty recycle bin", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
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
                                await Task.Delay(50);
                                //File.Delete(imagePath.Path);
                                //var dir = Path.GetDirectoryName(imagePath.Path);
                                //var fileName = Path.GetFileNameWithoutExtension(imagePath.Path);
                                //var textFilePath = Path.Join(dir, $"{fileName}.txt");

                                //File.Delete(imagePath.Path);
                                //if (File.Exists(textFilePath))
                                //{
                                //    File.Delete(textFilePath);
                                //}

                                //_dataStore.DeleteImage(imagePath.Id);

                            }
                            catch (Exception e)
                            {
                                var result = Dispatcher.Invoke(() => MessageBox.Show(this, $"Failed to delete {imagePath.Path}. \n\n {e.Message}", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error));

                                if (result == MessageBoxResult.Cancel)
                                {
                                    break;
                                }

                            }
                        }

                        Dispatcher.Invoke(() =>
                        {
                            _model.TotalFilesScan = 100;
                            _model.CurrentPositionScan = 0;

                            MessageBox.Show($"{count} images were deleted", "Delete images", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_configuration.TryLoad(out _settings))
            {
                _settings = new Settings();

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
                    Scan();
                }
            }
            else
            {
                ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);
            }


            this.WindowState = _settings.WindowState;
            this.Width = _settings.WindowSize.Width;
            this.Height = _settings.WindowSize.Height;



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

            if (!_settings.DontShowWelcomeOnStartup)
            {
                var welcome = new WelcomeWindow(_settings);
                welcome.Owner = this;
                welcome.ShowDialog();
            }

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

                _settings.SetPristine();

            }
        }

        private void Rescan(object obj)
        {
            if (_settings.ImagePaths.Any())
            {
                Scan();
            }
            else
            {
                MessageBox.Show("No image paths configured!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void Rebuild(object obj)
        {
            if (_settings.ImagePaths.Any())
            {
                Rebuild();
            }
            else
            {
                MessageBox.Show("No image paths configured!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnNavigate(Page page)
        {
            _model.Page = page;
        }


        private void Scan()
        {
            Task.Run(() => ScanInternal(_settings.ImagePaths, false)).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully && t.Result)
                {
                    _search.ReloadMatches();
                }
            });
        }

        private void Rebuild()
        {
            Task.Run(() => ScanInternal(_settings.ImagePaths, true)).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully && t.Result)
                {
                    _search.ReloadMatches();
                }
            });
        }

        private bool ScanInternal(IEnumerable<string> paths, bool updateImages)
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

                Dispatcher.Invoke(() =>
                {
                    if (added == 0)
                    {
                        MessageBox.Show(_navigatorService.Host,
                            "No new images found",
                            "Scan Complete",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var newOrOpdated = updateImages ? $"{added:#,###,##0} images updated" : $"{added:#,###,##0} new images added";

                        var missing = removed > 0 ? $"{removed:#,###,##0} missing images removed" : string.Empty;

                        var messages = new[] { newOrOpdated, missing };

                        var message = string.Join("\n", messages.Where(m => !string.IsNullOrEmpty(m)));

                        MessageBox.Show(_navigatorService.Host,
                            message,
                            updateImages ? "Rebuild Complete" : "Scan Complete",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    SetTotalFilesStatus();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(_navigatorService.Host,
                    ex.Message,
                    "Scan Error",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
}

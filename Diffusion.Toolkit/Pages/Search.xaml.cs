using Diffusion.Database;
using Diffusion.IO;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Diffusion.Toolkit.Thumbnails;
using File = System.IO.File;
using Image = Diffusion.Database.Image;
using Path = System.IO.Path;
using static System.Net.WebRequestMethods;
using Diffusion.Toolkit.Classes;
using System.Reflection;

namespace Diffusion.Toolkit.Pages
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Search : Page
    {
        private readonly SearchModel _model;
        private NavigatorService _navigatorService;
        private DataStore _dataStore;
        private Settings _settings;

        private CancellationTokenSource _thumbnailLoaderCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

        public Search()
        {
            InitializeComponent();

            ThumbnailCache.CreateInstance();
            ThumbnailLoader.CreateInstance(Dispatcher);

            Task.Run(() =>
            {
                _ = ThumbnailLoader.Instance.StartRun(
                    _thumbnailLoaderCancellationTokenSource.Token);
            });

            PrevPage.IsEnabled = false;
            NextPage.IsEnabled = false;
        }

        public Search(NavigatorService navigatorService, DataStore dataStore, Settings settings) : this()
        {
            this._navigatorService = navigatorService;
            this._dataStore = dataStore;
            _settings = settings;

            navigatorService.Host.Closed += (sender, args) =>
            {
                _thumbnailLoaderCancellationTokenSource.Cancel();
                _searchCancellationTokenSource.Cancel();
            };

            var total = _dataStore.GetTotal();


            _model = new SearchModel();
            _model.Status = $"{total:###,###,##0} images in database";
            _model.Page = 0;
            _model.Pages = 0;
            _model.TotalFiles = 100;
            _model.TotalFilesScan = 100;

            _model.PropertyChanged += ModelOnPropertyChanged;
            _model.SearchCommand = new RelayCommand<object>(SearchImages);
            _model.CopyPathCommand = new RelayCommand<object>(CopyPath);
            _model.CopyPromptCommand = new RelayCommand<object>(CopyPrompt);
            _model.CopyNegativePromptCommand = new RelayCommand<object>(CopyNegative);
            _model.CopyParameters = new RelayCommand<object>(CopyParameters);
            _model.OpenInExplorerCommand = new RelayCommand<object>(OpenInExplorer);
            DataContext = _model;
        }

        private void OpenInExplorer(object obj)
        {
            if (_model.SelectedImageEntry == null) return;
            var p = _model.SelectedImageEntry.FileParameters.Path;
            Process.Start("explorer.exe", $"/select,\"{p}\"");
        }

        public Settings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        private void CopyPath(object obj)
        {
            if (_model.SelectedImageEntry == null) return;
            var p = _model.SelectedImageEntry.FileParameters.Path;
            Clipboard.SetText(p);
        }

        private void CopyPrompt(object obj)
        {
            if (_model.SelectedImageEntry == null) return;
            var p = _model.SelectedImageEntry.FileParameters.Prompt;
            Clipboard.SetText(p);
        }

        private void CopyNegative(object obj)
        {
            if (_model.SelectedImageEntry == null) return;
            var p = _model.SelectedImageEntry.FileParameters.NegativePrompt;
            Clipboard.SetText(p);
        }


        private void CopyParameters(object obj)
        {
            if (_model.SelectedImageEntry == null) return;

            var p = _model.SelectedImageEntry.FileParameters.Prompt;
            var n = _model.SelectedImageEntry.FileParameters.NegativePrompt;
            var o = _model.SelectedImageEntry.FileParameters.OtherParameters;
            var parameters = $"{p}\r\n\r\nNegative prompt: {n}\r\n{o}";

            Clipboard.SetText(parameters);
        }

        public void SearchImages()
        {
            SearchImages(null);
        }

        public void SearchImages(object obj)
        {
            if (!_settings.ImagePaths.Any())
            {
                MessageBox.Show("No image paths configured!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _model.Images!.Clear();
            
            try
            {
                var count = _dataStore.Count(_model.SearchText);

                if (count == 0)
                {
                    MessageBox.Show(_navigatorService.Host, "The search term yielded no results", "No items found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                _model.Pages = count / _settings.PageSize + (count % _settings.PageSize > 1 ? 1 : 0);
                PrevPage.IsEnabled = false;
                NextPage.IsEnabled = _model.Pages > 1;
                _model.Results = $"{count:###,###,##0} results found";

                setPage = true;
                _model.Page = 1;
                setPage = false;

                ReloadMatches((string)obj != "ManualSearch");
            }
            catch (Exception e)
            {
                MessageBox.Show(_navigatorService.Host, e.Message, "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool setPage = false;

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchModel.SelectedImageEntry))
            {
                _model.SelectedImage = _model.SelectedImageEntry == null ? null : GetBitmapImage(_model.SelectedImageEntry.FileParameters.Path);
            }
            else if (e.PropertyName == nameof(SearchModel.Page))
            {
                if (setPage) return;

                setPage = true;

                if (_model.Page > _model.Pages)
                {
                    _model.Page = _model.Pages;
                }
                if (_model.Page < 1)
                {
                    _model.Page = 1;
                }

                PrevPage.IsEnabled = _model.Page > 1;
                NextPage.IsEnabled = _model.Pages > 1;

                setPage = false;

                //ReloadMatches();
            }
        }

        public static BitmapImage GetBitmapImage(string path)
        {
            BitmapImage bitmap = null;
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
            bitmap.CacheOption = BitmapCacheOption.None;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private bool isScanning = false;

        public void Scan()
        {
            if (isScanning)
            {
                return;
            }

            isScanning = true;
            Task.Run(() => ScanInternal(_settings.ImagePaths));
        }

        private void ScanInternal(IEnumerable<string> paths)
        {
            try
            {
                var added = 0;
                var scanned = 0;

                var scanner = new Scanner(_settings.FileExtensions);

                var images = _dataStore.GetImagePaths();
                var ignoreFiles = images.ToHashSet();

                foreach (var path in paths)
                {
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
                        scanned++;

                        newImages.Add(new Image()
                        {
                            Width = file.Width,
                            Height = file.Height,
                            ModelHash = file.ModelHash,
                            Path = file.Path,
                            Steps = file.Steps,
                            Sampler = file.Sampler,
                            CFGScale = file.CFGScale,
                            Seed = file.Seed,
                            BatchPos = file.BatchPos,
                            BatchSize = file.BatchSize,
                            CreatedDate = File.GetCreationTime(file.Path),
                            NegativePrompt = file.NegativePrompt,
                            //OtherParameters = file.OtherParameters,
                            //Parameters = file.Parameters,
                            Prompt = file.Prompt
                        });

                        added++;

                        if (newImages.Count == 50)
                        {
                            _dataStore.AddImages(newImages);
                            newImages.Clear();
                        }

                        if (scanned % 51 == 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _model.CurrentPositionScan += 51;
                                _model.Status = $"Scanning {_model.CurrentPositionScan} of {_model.TotalFilesScan}...";
                            });
                        }
                    }

                    if (newImages.Count > 0)
                    {
                        _dataStore.AddImages(newImages);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        _model.Status = $"Scanning {_model.TotalFilesScan} of {_model.TotalFilesScan}...";
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
                        MessageBox.Show(_navigatorService.Host,
                            $"New images added: {added}",
                            "Scan Complete",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    var total = _dataStore.GetTotal();
                    _model.Status = $"{total:###,###,##0} images in database";
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
                isScanning = false;

            }

        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelected();
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OpenSelected();
            }
        }

        private void OpenSelected()
        {
            using Process fileopener = new Process();

            if (_model.SelectedImageEntry != null)
            {
                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + _model.SelectedImageEntry.FileParameters.Path + "\"";
                fileopener.Start();
            }
        }

        private void PrevPage_OnClick(object sender, RoutedEventArgs e)
        {
            setPage = true;
            _model.Page--;
            setPage = false;

            NextPage.IsEnabled = true;

            if (_model.Page == 1)
            {
                PrevPage.IsEnabled = false;
            }

            ReloadMatches();
        }

        private Task ReloadMatches(bool focus = true)
        {
            _searchCancellationTokenSource.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            return Task.Run(() => LoadMatchesOnThread(_searchCancellationTokenSource.Token), _searchCancellationTokenSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ResetView(focus);
                        });
                    }
                });


        }

        private void NextPage_OnClick(object sender, RoutedEventArgs e)
        {
            setPage = true;
            _model.Page++;
            setPage = false;

            PrevPage.IsEnabled = true;
            if (_model.Page == _model.Pages)
            {
                NextPage.IsEnabled = false;
            }

            ReloadMatches();
        }

        private void LoadMatchesOnThread(CancellationToken token)
        {
            var matches = _dataStore
                .Search(_model.SearchText, _settings.PageSize,
                    _settings.PageSize * (_model.Page - 1)).Select(file =>
                    new FileParameters()
                    {
                        Width = file.Width,
                        Height = file.Height,
                        ModelHash = file.ModelHash,
                        Path = file.Path,
                        Steps = file.Steps,
                        Sampler = file.Sampler,
                        CFGScale = file.CFGScale,
                        Seed = file.Seed,
                        BatchPos = file.BatchPos,
                        BatchSize = file.BatchSize,
                        //CreatedDate = File.GetCreationTime(file.Path),
                        NegativePrompt = file.NegativePrompt,
                        OtherParameters =
                            $"Steps: {file.Steps}, Sampler: {file.Sampler}, CFG scale: {file.CFGScale}, Seed: {file.Seed}, Size: {file.Width}x{file.Height}, Model hash: {file.ModelHash}",
                        //Parameters = file.Parameters,
                        Prompt = file.Prompt
                    });

            var images = new List<ImageEntry>();

            Dispatcher.Invoke(() =>
            {
                _model.TotalFiles = _settings.PageSize;
                _model.CurrentPosition = 0;
            });

            _model.Images = new ObservableCollection<ImageEntry>();

            var count = 0;
            foreach (var file in matches)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                images.Add(new ImageEntry()
                {
                    FileParameters = file,
                    FileName = Path.GetFileName(file.Path),
                });

                if (count % 50 == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var image in images)
                        {
                            _model.Images.Add(image);
                        }
                        _model.CurrentPosition += images.Count;
                        images.Clear();
                    });
                }


                //Dispatcher.Invoke(() =>
                //{
                //    _model.CurrentPosition++;
                //});

                count++;
            }


            Dispatcher.Invoke(() =>
            {
                foreach (var image in images)
                {
                    _model.Images.Add(image);
                }

                _model.IsEmpty = _model.Images.Count == 0;
                _model.TotalFiles = Int32.MaxValue;
                _model.CurrentPosition = 0;
            });

        }


        private void LoadMatches(CancellationToken token)
        {
            var matches = _dataStore
                .Search(_model.SearchText, _settings.PageSize,
                    _settings.PageSize * (_model.Page - 1)).Select(file =>
                    new FileParameters()
                    {
                        Width = file.Width,
                        Height = file.Height,
                        ModelHash = file.ModelHash,
                        Path = file.Path,
                        Steps = file.Steps,
                        Sampler = file.Sampler,
                        CFGScale = file.CFGScale,
                        Seed = file.Seed,
                        BatchPos = file.BatchPos,
                        BatchSize = file.BatchSize,
                        //CreatedDate = File.GetCreationTime(file.Path),
                        NegativePrompt = file.NegativePrompt,
                        OtherParameters =
                            $"Steps: {file.Steps}, Sampler: {file.Sampler}, CFG scale: {file.CFGScale}, Seed: {file.Seed}, Size: {file.Width}x{file.Height}, Model hash: {file.ModelHash}",
                        //Parameters = file.Parameters,
                        Prompt = file.Prompt
                    });

            _model.Images = new ObservableCollection<ImageEntry>();
            _model.TotalFiles = _settings.PageSize;
            _model.CurrentPosition = 0;

            var images = new List<ImageEntry>();
            foreach (var file in matches)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                images.Add(new ImageEntry()
                {
                    FileParameters = file,
                    FileName = Path.GetFileName(file.Path),
                });


                _model.CurrentPosition++;
            }

            _model.TotalFiles = Int32.MaxValue;
            _model.CurrentPosition = 0;
        }

        private void Page_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReloadMatches();
                e.Handled = true;
            }
        }

        private void ResetView(bool focus)
        {
            if (_model.Images is { Count: > 0 })
            {
                ThumbnailListView.ScrollIntoView(_model.Images[0]);
                ThumbnailListView.SelectedItem = _model.Images[0];

                if (focus)
                {
                    if (ThumbnailListView.ItemContainerGenerator.ContainerFromIndex(0) is ListViewItem item)
                    {
                        item.Focus();
                    }

                }
            }
        }
    }
}

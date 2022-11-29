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
using File = System.IO.File;
using Image = Diffusion.Database.Image;
using Path = System.IO.Path;

namespace Diffusion.Toolkit.Pages
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Search : Page
    {
        private readonly SearchModel _model;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private NavigatorService _navigatorService;
        private DataStore _dataStore;

        private CancellationTokenSource _thumbnailLoaderCancellationTokenSource = new CancellationTokenSource();

        public Search()
        {
            InitializeComponent();

            ThumbnailLoader.CreateInstance(Dispatcher);

            Task.Run(() =>
            {
                ThumbnailLoader.Instance.Start(_thumbnailLoaderCancellationTokenSource.Token);
            });

            PrevPage.IsEnabled = false;
            NextPage.IsEnabled = false;
        }

        public Search(NavigatorService navigatorService, DataStore dataStore) : this()
        {
            this._navigatorService = navigatorService;
            this._dataStore = dataStore;

            var total = _dataStore.GetTotal();


            _model = new SearchModel();
            _model.Status = $"{total} images in database";
            _model.Page = 1;
            _model.TotalFiles = 100;
            _model.TotalFilesScan = 100;

            _model.PropertyChanged += ModelOnPropertyChanged;
            _model.SearchCommand = new RelayCommand<object>(SearchImages);
            _model.CopyPathCommand = new RelayCommand<object>(CopyPath);
            _model.CopyPromptCommand = new RelayCommand<object>(CopyPrompt);
            _model.CopyNegativePromptCommand = new RelayCommand<object>(CopyNegative);
            _model.CopyParameters = new RelayCommand<object>(CopyParameters);
            DataContext = _model;
        }

        private void CopyPath(object obj)
        {
            var p = _model.SelectedImageEntry.FileParameters.Path;
            Clipboard.SetText(p);
        }

        private void CopyPrompt(object obj)
        {
            var p = _model.SelectedImageEntry.FileParameters.Prompt;
            Clipboard.SetText(p);
        }

        private void CopyNegative(object obj)
        {
            var p = _model.SelectedImageEntry.FileParameters.NegativePrompt;
            Clipboard.SetText(p);
        }


        private void CopyParameters(object obj)
        {
            var p = _model.SelectedImageEntry.FileParameters.Prompt;
            var n = _model.SelectedImageEntry.FileParameters.NegativePrompt;
            var o = _model.SelectedImageEntry.FileParameters.OtherParameters;
            var parameters = $"{p}\r\n\r\n{n}\r\n{o}";

            Clipboard.SetText(parameters);
        }

        private List<FileParameters>? _matches;
        private int _page = 1;
        private int _pageSize = 250;
        private int _pages = 0;

        private void SearchImages(object obj)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            var token = cancellationTokenSource.Token;

            Task.Run(async () =>
            {

                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        _model.Images!.Clear();
                    });


                    _matches = _dataStore.Search(_model.SearchText).Select(file => new FileParameters()
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
                        OtherParameters = $"Steps: {file.Steps}, Sampler: {file.Sampler}, CFG scale: {file.CFGScale}, Seed: {file.Seed}, Size: {file.Width}x{file.Height}, Model hash: {file.ModelHash}",
                        //Parameters = file.Parameters,
                        Prompt = file.Prompt
                    }).ToList();

                    _pages = _matches.Count / _pageSize + (_matches.Count % _pageSize > 1 ? 1 : 0);
                    _model.Page = 1;
                    
                    Dispatcher.Invoke(() =>
                    {
                        PrevPage.IsEnabled = false;
                        NextPage.IsEnabled = _pages > 1;
                    });

                    await LoadMatchesAsync(cancellationTokenSource.Token);

                }
                catch (Exception e)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(e.Message, "Error!",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    });
                    throw;
                }

            }, token).ContinueWith(t =>
            {
                if (!t.IsCompletedSuccessfully)
                {

                }
            });
        }

        private bool setPage = false;

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchModel.SelectedImageEntry))
            {
                _model.SelectedImage = _model.SelectedImageEntry == null ? null : GetBitmapImage(_model.SelectedImageEntry.FileParameters.Path);
            }
            if (e.PropertyName == nameof(SearchModel.Page))
            {
                if (setPage) return;

                setPage = true;
                
                if (_model.Page > _pages)
                {
                    _model.Page = _pages;
                }
                if (_model.Page < 1)
                {
                    _model.Page = 1;
                }

                PrevPage.IsEnabled = _model.Page > 1;
                NextPage.IsEnabled = _pages > 1;
                
                setPage = false;

                cancellationTokenSource.Cancel();
                cancellationTokenSource = new CancellationTokenSource();

                Task.Run(async () => await LoadMatchesAsync(cancellationTokenSource.Token));
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

        public void Scan(IEnumerable<string> paths)
        {
            if (isScanning)
            {
                return;
            }

            isScanning = true;
            Task.Run(() => ScanInternal(paths));
        }

        private void ScanInternal(IEnumerable<string> paths)
        {
            try
            {
                var added = 0;
                var scanned = 0;

                var scanner = new Scanner();

                foreach (var path in paths)
                {
                    var max = scanner.Count(path);

                    Dispatcher.Invoke(() =>
                    {
                        _model.TotalFilesScan = max;
                        _model.CurrentPositionScan = 0;
                    });

                    var files = scanner.Scan(path);

                    var images = _dataStore.GetImagePaths();


                    var imgHash = images.ToHashSet();

                    foreach (var file in files)
                    {
                        scanned++;

                        if (!imgHash.Contains(file.Path))
                        {
                            //_model.FileParameters.Add(file);
                            _dataStore.AddImage(new Image()
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
                        }

                        Dispatcher.Invoke(() =>
                        {
                            _model.CurrentPositionScan++;
                            _model.Status = $"Scanning {_model.CurrentPositionScan} of {_model.TotalFilesScan}...";
                        });
                    }


                    Dispatcher.Invoke(() =>
                    {
                        _model.TotalFilesScan = Int32.MaxValue;
                        _model.CurrentPositionScan = 0;

                    });
                }

                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Files Scanned: {scanned}\r\nFiles Added: {added}", "Scan Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    var total = _dataStore.GetTotal();
                    _model.Status = $"{total} images in database";
                });
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
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            _model.Page--;
            NextPage.IsEnabled = true;

            if (_model.Page == 1)
            {
                PrevPage.IsEnabled = false;
            }

            Task.Run(async () => await LoadMatchesAsync(cancellationTokenSource.Token));
        }

        private void NextPage_OnClick(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            _model.Page++;
            PrevPage.IsEnabled = true;
            if (_model.Page == _pages)
            {
                NextPage.IsEnabled = false;
            }
            Task.Run(async () => await LoadMatchesAsync(cancellationTokenSource.Token));
        }

        private async Task LoadMatchesAsync(CancellationToken token)
        {
            if (_matches == null) return;

            var images = new List<ImageEntry>();

            Dispatcher.Invoke(() =>
            {
                _model.TotalFiles = _pageSize;
                _model.CurrentPosition = 0;
            });

            await Task.Yield();

            foreach (var file in _matches.Skip(_pageSize * (_model.Page - 1)).Take(_pageSize))
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

                
                Dispatcher.Invoke(() =>
                {
                    _model.CurrentPosition++;
                });
            }

            Dispatcher.Invoke(() =>
            {
                _model.Images = new ObservableCollection<ImageEntry>(images);
                _model.TotalFiles = Int32.MaxValue;
                _model.CurrentPosition = 0;
            });

        }

        private void FrameworkElement_OnTargetUpdated(object? sender, DataTransferEventArgs e)
        {
            var listview = ((ListView)sender!);
            if (listview.Items.Count > 0)
            {
                listview.ScrollIntoView(listview.Items[0]);
            }


        }
    }
}

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
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Diffusion.Toolkit.Thumbnails;
using File = System.IO.File;
using Image = Diffusion.Database.Image;
using Path = System.IO.Path;
using Diffusion.Toolkit.Classes;

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

        private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

        public Search()
        {
            InitializeComponent();

            Task.Run(() =>
            {
                _ = ThumbnailLoader.Instance.StartRun();
            });

            PrevPage.IsEnabled = false;
            NextPage.IsEnabled = false;
        }


        private Random r = new Random();
        private readonly string[] _searchHints = File.ReadAllLines("hints.txt").Where(s => !string.IsNullOrEmpty(s.Trim())).ToArray();

        private void GetRandomHint()
        {
            var randomHint = _searchHints[r.Next(_searchHints.Length)];
            _model.SearchHint = $"Search for {randomHint}";
        }

        public Search(NavigatorService navigatorService, DataStore dataStore, Settings settings) : this()
        {
            this._navigatorService = navigatorService;
            this._dataStore = dataStore;
            _settings = settings;

            navigatorService.Host.Closed += (sender, args) =>
            {
                ThumbnailLoader.Instance.Flush();
                ThumbnailLoader.Instance.Stop();
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
            _model.CurrentImage.CopyPathCommand = new RelayCommand<object>(CopyPath);
            _model.CurrentImage.CopyPromptCommand = new RelayCommand<object>(CopyPrompt);
            _model.CurrentImage.CopyNegativePromptCommand = new RelayCommand<object>(CopyNegative);
            _model.CurrentImage.CopyParameters = new RelayCommand<object>(CopyParameters);
            _model.CurrentImage.OpenInExplorerCommand = new RelayCommand<object>(OpenInExplorer);
            DataContext = _model;
        }

        public Settings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        private void OpenInExplorer(object obj)
        {
            if (_model.CurrentImage == null) return;
            var p = _model.CurrentImage.Path;
            Process.Start("explorer.exe", $"/select,\"{p}\"");
        }


        private void CopyPath(object obj)
        {
            if (_model.CurrentImage == null) return;
            var p = _model.CurrentImage.Path;
            Clipboard.SetText(p);
        }

        private void CopyPrompt(object obj)
        {
            if (_model.CurrentImage == null) return;
            var p = _model.CurrentImage.Prompt;
            Clipboard.SetText(p);
        }

        private void CopyNegative(object obj)
        {
            if (_model.CurrentImage == null) return;
            var p = _model.CurrentImage.NegativePrompt;
            Clipboard.SetText(p);
        }


        private void CopyParameters(object obj)
        {
            if (_model.CurrentImage == null) return;

            var p = _model.CurrentImage.Prompt;
            var n = _model.CurrentImage.NegativePrompt;
            var o = _model.CurrentImage.OtherParameters;
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
                if (_model.SearchHistory.Count + 1 > 25)
                {
                    _model.SearchHistory.RemoveAt(_model.SearchHistory.Count - 1);
                }

                _model.SearchHistory.Insert(0, _model.SearchText);

                var count = _dataStore.Count(_model.SearchText);

                if (count == 0)
                {
                    _model.ResultStatus = "No results found";
                    MessageBox.Show(_navigatorService.Host, "The search term yielded no results", "No results found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
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
                if (_model.SelectedImageEntry != null)
                {
                    var parameters = Metadata.ReadFromFile(_model.SelectedImageEntry.Path);

                    _model.CurrentImage.Image = _model.SelectedImageEntry == null ? null : GetBitmapImage(_model.SelectedImageEntry.Path);
                    _model.CurrentImage.Path = parameters.Path;
                    _model.CurrentImage.Prompt = parameters.Prompt;
                    _model.CurrentImage.NegativePrompt = parameters.NegativePrompt;
                    _model.CurrentImage.OtherParameters = parameters.OtherParameters;
                    _model.CurrentImage.Favorite = _model.SelectedImageEntry.Favorite;
                }
            }
            else if (e.PropertyName == nameof(SearchModel.SearchText))
            {
                if (string.IsNullOrEmpty(_model.SearchText))
                {
                    GetRandomHint();
                }
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
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            stream.Close();
            stream.Dispose();
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
            Task.Run(() => ScanInternal(_settings.ImagePaths, false));
        }

        public void Rebuild()
        {
            if (isScanning)
            {
                return;
            }

            isScanning = true;
            Task.Run(() => ScanInternal(_settings.ImagePaths, true));
        }

        private void ScanInternal(IEnumerable<string> paths, bool updateImages)
        {
            try
            {
                var added = 0;
                var scanned = 0;

                var scanner = new Scanner(_settings.FileExtensions);

                var existingImages = _dataStore.GetImagePaths().ToList();

                HashSet<string> ignoreFiles = updateImages ? new HashSet<string>() : existingImages.Select(p => p.Path).ToHashSet();

                var removed = existingImages.Where(img => !File.Exists(img.Path)).ToList();

                if (removed.Any())
                {
                    _dataStore.DeleteImages(removed.Select(i => i.Id));
                }

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

                            added++;

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
                        if (updateImages)
                        {
                            _dataStore.UpdateImagesByPath(newImages);
                        }
                        else
                        {
                            _dataStore.AddImages(newImages);
                        }
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
                        var newOrOpdated = updateImages ? $"{added} images updated" : $"{added} new images added";

                        var missing = removed.Count >  0 ? $"{removed.Count} missing images removed" : string.Empty;

                        var messages = new[] { newOrOpdated, missing };

                        var message = string.Join("\n", messages.Where(m=> !string.IsNullOrEmpty(m)));

                        MessageBox.Show(_navigatorService.Host,
                            message,
                            updateImages ? "Rebuild Complete" : "Scan Complete",
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
            var ratings = new[]
            {
                Key.D1,
                Key.D2,
                Key.D3,
                Key.D4,
                Key.D5,
            };

            if (e.Key == Key.Enter)
            {
                OpenSelected();
            }
            else if (e.Key == Key.Delete || e.Key == Key.X)
            {
                if (ThumbnailListView.SelectedItems != null)
                {
                    foreach (ImageEntry entry in ThumbnailListView.SelectedItems)
                    {
                        entry.ForDeletion = !entry.ForDeletion;
                        _dataStore.SetDeleted(entry.Id, entry.ForDeletion);
                    }
                }
            }
            else if (e.Key == Key.F)
            {
                if (ThumbnailListView.SelectedItems != null)
                {
                    foreach (ImageEntry entry in ThumbnailListView.SelectedItems)
                    {
                        entry.Favorite = !entry.Favorite;
                        if (_model.CurrentImage != null && _model.CurrentImage.Path == entry.Path)
                        {
                            _model.CurrentImage.Favorite = entry.Favorite;
                        }
                        _dataStore.SetFavorite(entry.Id, entry.Favorite);
                    }
                }
            }
            else if (ratings.Contains(e.Key))
            {
                var rating = e.Key switch
                {
                    Key.D1 => 1,
                    Key.D2 => 2,
                    Key.D3 => 3,
                    Key.D4 => 4,
                    Key.D5 => 5,
                };

                if (ThumbnailListView.SelectedItems != null)
                {
                    foreach (ImageEntry entry in ThumbnailListView.SelectedItems)
                    {

                        if (entry.Rating == rating)
                        {
                            entry.Rating = null;
                        }
                        else
                        {
                            entry.Rating = rating;
                        }
                        _dataStore.SetRating(entry.Id, entry.Rating);
                    }
                }

            }
        }

        public void SetOpacityView(bool value)
        {
            _model.ImageOpacity = value ? 0.35f : 1.0f;
        }

        public void SetIconVisibility(bool value)
        {
            _model.HideIcons = value;
        }

        private void OpenSelected()
        {
            using Process fileopener = new Process();

            if (_model.SelectedImageEntry != null)
            {
                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + _model.SelectedImageEntry.Path + "\"";
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

        public Task ReloadMatches(bool focus = true)
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
            ThumbnailLoader.Instance.Flush();

            var matches = _dataStore
                .Search(_model.SearchText, _settings.PageSize,
                    _settings.PageSize * (_model.Page - 1));

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
                    Id = file.Id,
                    Favorite = file.Favorite,
                    ForDeletion = file.ForDeletion,
                    Rating = file.Rating,
                    Path = file.Path,
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

        public void ShowFavorite()
        {
            _model.SearchText = "favorite: true";
            SearchImages(null);
        }

        public void ShowMarked()
        {
            _model.SearchText = "delete: true";
            SearchImages(null);
        }
    }
}

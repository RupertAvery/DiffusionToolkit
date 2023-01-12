using Diffusion.Database;
using Diffusion.IO;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Diffusion.Toolkit.Thumbnails;
using File = System.IO.File;
using Path = System.IO.Path;
using Diffusion.Toolkit.Classes;
using Model = Diffusion.IO.Model;
using Task = System.Threading.Tasks.Task;
using System.Windows.Media;

namespace Diffusion.Toolkit.Pages
{
    public class ModeSettings
    {
        public ModeSettings()
        {
            History = new List<string?>();
        }

        public string LastQuery { get; set; }
        public List<string?> History { get; set; }
        public int LastPage { get; set; }
        public string ExtraQuery { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Search : Page
    {
        private readonly SearchModel _model;
        private NavigatorService _navigatorService;
        private DataStore _dataStore;
        private Settings _settings;

        private ModeSettings _currentModeSettings;

        private ICollection<Model>? _modelLookup;

        public Search()
        {
            InitializeComponent();


            Task.Run(() =>
            {
                _ = ThumbnailLoader.Instance.StartRun();
            });



            //var str = new System.Text.StringBuilder();
            //using (var writer = new System.IO.StringWriter(str))
            //    System.Windows.Markup.XamlWriter.Save(MyContextMenu.Template, writer);
            //System.Diagnostics.Debug.Write(str);

        }


        private Random r = new Random();
        private readonly string[] _searchHints = File.ReadAllLines("hints.txt").Where(s => !string.IsNullOrEmpty(s.Trim())).ToArray();

        private void GetRandomHint()
        {
            var randomHint = _searchHints[r.Next(_searchHints.Length)];
            _model.SearchHint = $"Search for {randomHint}";
        }

        private Regex _gridLengthRegex = new Regex("Auto|(?<value>\\d+(?:\\.\\d+)?)(?<star>\\*)?");

        public GridLength GetGridLength(string? value)
        {
            if (string.IsNullOrEmpty(value)) return new GridLength(0, GridUnitType.Auto);

            if (value == "*") return new GridLength(0, GridUnitType.Star);

            var match = _gridLengthRegex.Match(value);

            if (match.Groups[0].Value == "Auto")
            {
                return new GridLength();
            }
            else if (match.Groups["star"].Success)
            {
                return new GridLength(double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture), GridUnitType.Star);
            }
            else
            {
                return new GridLength(double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture), GridUnitType.Pixel);
            }
        }

        public Action OnPopout
        {
            set
            {
                PreviewPane.OnPopout = value;
            }
        }

        public Search(NavigatorService navigatorService, DataStore dataStore, MessagePopupManager messagePopupManager, Settings settings) : this()
        {
            this._navigatorService = navigatorService;
            this._dataStore = dataStore;
            _settings = settings;

            navigatorService.Host.Closed += async (sender, args) =>
            {
                ThumbnailLoader.Instance.Stop();
            };

            _modeSettings = new Dictionary<string, ModeSettings>()
            {
                { "search", new ModeSettings() { Name="Diffusions", ExtraQuery = "" } },
                { "favorites", new ModeSettings() { Name="Favorites", ExtraQuery = "favorite: true" } },
                { "deleted", new ModeSettings() { Name="Recycle Bin", ExtraQuery = "delete: true" } },
            };

            if (_settings.MainGridWidth != null)
            {
                MainGrid.ColumnDefinitions[0].Width = GetGridLength(_settings.MainGridWidth);
                MainGrid.ColumnDefinitions[2].Width = GetGridLength(_settings.MainGridWidth2);
            }
            //if (_settings.PreviewGridHeight != null)
            //{
            //    PreviewGrid.RowDefinitions[0].Height = GetGridLength(_settings.PreviewGridHeight);
            //    PreviewGrid.RowDefinitions[2].Height = GetGridLength(_settings.PreviewGridHeight2);
            //}

            var widthDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
            widthDescriptor.AddValueChanged(MainGrid.ColumnDefinitions[0], WidthChanged);
            widthDescriptor.AddValueChanged(MainGrid.ColumnDefinitions[2], WidthChanged2);

            //var heightDescriptor = DependencyPropertyDescriptor.FromProperty(RowDefinition.HeightProperty, typeof(ItemsControl));
            //heightDescriptor.AddValueChanged(PreviewGrid.RowDefinitions[0], HeightChanged);
            //heightDescriptor.AddValueChanged(PreviewGrid.RowDefinitions[2], HeightChanged2);

            _model = new SearchModel();

            _model.DataStore = _dataStore;
            _model.Page = 0;
            _model.Pages = 0;
            _model.TotalFiles = 100;
            _model.Images = new ObservableCollection<ImageEntry>();
            _model.PropertyChanged += ModelOnPropertyChanged;
            _model.SearchCommand = new RelayCommand<object>(SearchImages);

            _model.Refresh = new RelayCommand<object>((o) => ReloadMatches());
            _model.CurrentImage.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
            _model.CopyFiles = new RelayCommand<object>((o) => CopyFiles());

            _model.FocusSearch = new RelayCommand<object>((o) => SearchTermTextBox.Focus());
            _model.ShowDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = true);
            _model.HideDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = false);
            SetMode("search");

            DataContext = _model;

            ThumbnailListView.DataStore = dataStore;
            ThumbnailListView.MessagePopupManager = messagePopupManager;

            PreviewPane.NSFW = (id, b) =>
            {
                _dataStore.SetNSFW(id, b);
                Update(id);
            };
            PreviewPane.Favorite = (id, b) =>
            {
                _dataStore.SetFavorite(id, b);
                Update(id);
            };
            PreviewPane.Rate = (id, b) =>
            {
                _dataStore.SetRating(id, b);
                Update(id);
            };
            PreviewPane.Delete = (id, b) =>
            {
                _dataStore.SetDeleted(id, b);
                Update(id);
            };
            PreviewPane.OnNext = Next;
            PreviewPane.OnPrev = Prev;
        }

        private void CopyFiles()
        {
            //foreach (ImageEntry selectedItem in ThumbnailListView.SelectedItems)
            //{

            //}

            //DataObject dataObject = new DataObject();
            //dataObject.SetData(DataFormats.Bitmap, _selItems.Select(t => t.Path).ToArray());
            //DragDrop.DoDragDrop(source, dataObject, DragDropEffects.Copy);
        }



        private void WidthChanged(object? sender, EventArgs e)
        {
            _settings.MainGridWidth = MainGrid.ColumnDefinitions[0].Width.ToString();
        }

        private void WidthChanged2(object? sender, EventArgs e)
        {
            _settings.MainGridWidth2 = MainGrid.ColumnDefinitions[2].Width.ToString();
        }

        public Settings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public Action<IList<ImageEntry>> MoveFiles
        {
            get => ThumbnailListView.MoveFiles;
            set => ThumbnailListView.MoveFiles = value;

        }

        public Action<ImageViewModel> OnCurrentImageChange { get; set; }

        public ImageViewModel? CurrentImage => _model.CurrentImage;

        private void ShowInExplorer(object obj)
        {
            if (_model.CurrentImage == null) return;
            var p = _model.CurrentImage.Path;
            Process.Start("explorer.exe", $"/select,\"{p}\"");
        }

        public void SearchImages()
        {
            SearchImages(null);
        }

        static T Time<T>(Func<T> action)
        {
            Stopwatch t = new Stopwatch();
            T result = action();
            t.Start();
            Debug.WriteLine($"{t.ElapsedMilliseconds}ms");
            t.Stop();
            return result;
        }

        public void SearchImages(object obj)
        {
            if (!_settings.ImagePaths.Any())
            {
                MessageBox.Show("No image paths configured!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            try
            {
                Dispatcher.Invoke(() =>
                {
                    //_model.Images!.Clear();

                    if (!string.IsNullOrEmpty(_model.SearchText))
                    {
                        if (_model.SearchHistory.Count == 0 || (_model.SearchHistory.Count > 0 && _model.SearchHistory[0] != _model.SearchText))
                        {
                            if (_model.SearchHistory.Count + 1 > 25)
                            {
                                _model.SearchHistory.RemoveAt(_model.SearchHistory.Count - 1);
                            }
                            _model.SearchHistory.Insert(0, _model.SearchText);

                            _currentModeSettings.History = _model.SearchHistory.ToList();
                        }
                    }

                    _currentModeSettings.LastQuery = _model.SearchText;

                    // need a better way to do this... property?
                    var query = _model.SearchText + " " + _currentModeSettings.ExtraQuery;

                    var count = _dataStore.Count(query);
                    var size = _dataStore.CountFileSize(query);

                    //_model.FileSize = size;

                    _model.IsEmpty = count == 0;

                    _model.Pages = count / _settings.PageSize + (count % _settings.PageSize > 1 ? 1 : 0);

                    float fsize = size;

                    var ssize = $"{fsize:#,##0} B";

                    if (fsize > 1073741824)
                    {
                        fsize /= 1073741824;
                        ssize = $"{fsize:#,##0.00} GiB";
                    }
                    else if (fsize > 1048576)
                    {
                        fsize /= 1048576;
                        ssize = $"{fsize:#,##0.00} MiB";
                    }
                    else if (fsize > 1024)
                    {
                        fsize /= 1024;
                        ssize = $"{fsize:#,##0.00} KiB";
                    }

                    _model.Results = $"{count:###,###,##0} results found ({ssize})";

                    if (_model.IsEmpty)
                    {
                        _model.Page = 0;
                        _model.ResultStatus = "No results found";
                        //MessageBox.Show(_navigatorService.Host, "The search term yielded no results", "No results found",
                        //    MessageBoxButton.OK,
                        //    MessageBoxImage.Information);
                        return;
                    }
                    _model.Page = 1;

                    ThumbnailListView.SetPagingEnabled();
                });



                ReloadMatches((string)obj != "ManualSearch");
            }
            catch (Exception e)
            {
                MessageBox.Show(_navigatorService.Host, e.Message, "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchModel.SelectedImageEntry))
            {
                if (_model.SelectedImageEntry != null)
                {
                    var parameters = Metadata.ReadFromFile(_model.SelectedImageEntry.Path);

                    try
                    {
                        PreviewPane.ResetZoom();

                        _model.CurrentImage.Id = _model.SelectedImageEntry.Id;
                        _model.CurrentImage.Image = _model.SelectedImageEntry == null ? null : GetBitmapImage(_model.SelectedImageEntry.Path);
                        _model.CurrentImage.Path = parameters.Path;
                        _model.CurrentImage.Prompt = parameters.Prompt;
                        _model.CurrentImage.NegativePrompt = parameters.NegativePrompt;
                        _model.CurrentImage.OtherParameters = parameters.OtherParameters;
                        _model.CurrentImage.Favorite = _model.SelectedImageEntry.Favorite;
                        _model.CurrentImage.Date = _model.SelectedImageEntry.CreatedDate.ToString();
                        _model.CurrentImage.Rating = _model.SelectedImageEntry.Rating;
                        _model.CurrentImage.NSFW = _model.SelectedImageEntry.NSFW;
                        _model.CurrentImage.ForDeletion = _model.SelectedImageEntry.ForDeletion;
                        _model.CurrentImage.ModelHash = parameters.ModelHash;
                        _model.CurrentImage.Seed = parameters.Seed;

                        if (_modelLookup != null)
                        {
                            var models = _modelLookup.Where(m => String.Equals(m.Hash, parameters.ModelHash, StringComparison.CurrentCultureIgnoreCase));

                            if (models.Any())
                            {
                                _model.CurrentImage.ModelName = string.Join(", ", models.Select(m => m.Filename)) + $" ({parameters.ModelHash})";
                            }
                            else
                            {
                                _model.CurrentImage.ModelName = $"Not found ({parameters.ModelHash})";
                            }
                        }
                        else
                        {
                            _model.CurrentImage.ModelName = $"Not found ({parameters.ModelHash})";
                        }

                        PreviewPane.ZoomPreview();

                        OnCurrentImageChange?.Invoke(_model.CurrentImage);
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show(_navigatorService.Host, "The source image could not be located. This can happen when you move or rename the file outside of Diffusion Toolkit.", "Load image failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(_navigatorService.Host, $"{ex.Message}", "An error occured", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            //else if (e.PropertyName == nameof(SearchModel.Page))
            //{
            //    ReloadMatches(true);
            //}
            else if (e.PropertyName == nameof(SearchModel.SearchText))
            {
                if (string.IsNullOrEmpty(_model.SearchText))
                {
                    GetRandomHint();
                }

            }
        }


        public static BitmapImage GetBitmapImage(string path)
        {
            BitmapImage bitmap;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            bitmap.Freeze();
            return bitmap;
        }

        public void SetOpacityView(bool value)
        {
            _model.ImageOpacity = value ? 0.35f : 1.0f;
        }

        public void SetIconVisibility(bool value)
        {
            _model.HideIcons = value;
        }

        public Task ReloadMatches(bool focus = true)
        {
            //await LoadMatchesAsync();

            //ThumbnailListView.ResetView(focus);
            return Task.Run(LoadMatchesOnThread)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ThumbnailListView.ResetView(focus);
                        });
                    }
                });
        }

        public void Next()
        {
            if (_model.Images == null) return;
            int currentIndex = 0;
            if (_model.SelectedImageEntry != null)
            {
                currentIndex = _model.Images.IndexOf(_model.SelectedImageEntry);
            }

            if (currentIndex < _model.Images.Count - 1)
            {
                _model.SelectedImageEntry = _model.Images[currentIndex + 1];

                ThumbnailListView.SelectItem(currentIndex + 1);
            }

        }

        public void Prev()
        {
            if (_model.Images == null) return;
            int currentIndex = 0;
            if (_model.SelectedImageEntry != null)
            {
                currentIndex = _model.Images.IndexOf(_model.SelectedImageEntry);
            }

            if (currentIndex > 0)
            {
                _model.SelectedImageEntry = _model.Images[currentIndex - 1];

                ThumbnailListView.SelectItem(currentIndex - 1);
            }
        }

        private async Task LoadMatchesAsync()
        {
            var rId = r.NextInt64();
            ThumbnailLoader.Instance.SetCurrentRequestId(rId);

            var query = _model.SearchText + " " + _currentModeSettings.ExtraQuery;

            var matches = Time(() => _dataStore
                .Search(query, _settings.PageSize,
                    _settings.PageSize * (_model.Page - 1)));

            Dispatcher.Invoke(() =>
            {
                _model.Images.Clear();
            });

            var images = new List<ImageEntry>();

            var sw = new Stopwatch();
            sw.Start();


            var count = 0;
            foreach (var file in matches)
            {
                images.Add(new ImageEntry(rId)
                {
                    Id = file.Id,
                    Favorite = file.Favorite,
                    ForDeletion = file.ForDeletion,
                    Rating = file.Rating,
                    Path = file.Path,
                    CreatedDate = file.CreatedDate,
                    FileName = Path.GetFileName(file.Path),
                    NSFW = file.NSFW
                });


                count++;
            }

            Dispatcher.Invoke(() =>
            {
                _model.Images = new ObservableCollection<ImageEntry>(images);
            });

            sw.Stop();

            Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds:#,###,##0}ms");

            foreach (var image in _model.Images)
            {
                await image.LoadThumbnail();
            }
        }

        private async Task LoadMatchesOnThread()
        {
            var rId = r.NextInt64();
            ThumbnailLoader.Instance.SetCurrentRequestId(rId);

            var query = _model.SearchText + " " + _currentModeSettings.ExtraQuery;

            var matches = Time(() => _dataStore
                .Search(query, _settings.PageSize,
                    _settings.PageSize * (_model.Page - 1)));

            Dispatcher.Invoke(() =>
            {
                _model.Images.Clear();
            });

            var images = new List<ImageEntry>();

            var sw = new Stopwatch();
            sw.Start();


            var count = 0;
            foreach (var file in matches)
            {
                var imageEntry = new ImageEntry(rId)
                {
                    Id = file.Id,
                    Favorite = file.Favorite,
                    ForDeletion = file.ForDeletion,
                    Rating = file.Rating,
                    Path = file.Path,
                    CreatedDate = file.CreatedDate,
                    FileName = Path.GetFileName(file.Path),
                    NSFW = file.NSFW
                };

                Dispatcher.Invoke(() =>
                {
                    _model.Images.Add(imageEntry);
                });

                //images.Add(imageEntry);


                count++;
            }

            //Dispatcher.Invoke(() =>
            //{
            //    _model.Images = new ObservableCollection<ImageEntry>(images);
            //});


            sw.Stop();


            Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds:#,###,##0}ms");


            await RefreshThumbnails();

        }

        public async Task RefreshThumbnails()
        {
            if (_model.Images != null)
            {
                foreach (var image in _model.Images)
                {
                    await image.LoadThumbnail();
                }
            }
        }

        private void Page_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReloadMatches();
                e.Handled = true;
            }
        }

        private Dictionary<string, ModeSettings> _modeSettings = new Dictionary<string, ModeSettings>();

        private ModeSettings GetModeSettings(string mode)
        {
            if (!_modeSettings.TryGetValue(mode, out var settings))
            {
                settings = new ModeSettings();
            }
            return settings;
        }

        private void SetMode(string mode)
        {
            _currentModeSettings = GetModeSettings(mode);
            _model.SearchText = _currentModeSettings.LastQuery;
            _model.SearchHistory = new ObservableCollection<string?>(_currentModeSettings.History);
            _model.ModeName = _currentModeSettings.Name;
        }

        public void ShowSearch()
        {
            SetMode("search");
            SearchImages(null);
        }

        public void ShowFavorite()
        {
            SetMode("favorites");
            SearchImages(null);
        }

        public void ShowMarked()
        {
            SetMode("deleted");
            SearchImages(null);
        }

        private void SearchTermTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    SearchTermTextBox.IsDropDownOpen = true;
                    e.Handled = true;
                    break;
            }
        }

        public void SetModels(ICollection<Model> modelsCollection)
        {
            _modelLookup = modelsCollection;
        }

        public void ToggleInfo()
        {
            _model.CurrentImage.IsParametersVisible = !_model.CurrentImage.IsParametersVisible;
        }

        public void SetNSFWBlur(bool value)
        {
            _model.NSFWBlur = value;
        }

        public void SetFitToPreview(bool value)
        {
            _model.CurrentImage.FitToPreview = value;
        }

        private void ThumbnailListView_OnPageChangedEvent(object? sender, int e)
        {
            ReloadMatches(true);
        }

        public void SetThumbnailSize(int thumbnailSize)
        {
            ThumbnailListView.SetThumbnailSize(thumbnailSize);
            _ = RefreshThumbnails();
        }

        public void SetPreviewVisible(bool visible)
        {
            PreviewPane.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            GridSplitter.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

            if (visible)
            {
                MainGrid.ColumnDefinitions[0].Width = GetGridLength(_settings.MainGridWidth);
                MainGrid.ColumnDefinitions[2].Width = GetGridLength(_settings.MainGridWidth2);

                var widthDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
                widthDescriptor.AddValueChanged(MainGrid.ColumnDefinitions[0], WidthChanged);
                widthDescriptor.AddValueChanged(MainGrid.ColumnDefinitions[2], WidthChanged2);
            }
            else
            {

                var widthDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
                widthDescriptor.RemoveValueChanged(MainGrid.ColumnDefinitions[0], WidthChanged);
                widthDescriptor.RemoveValueChanged(MainGrid.ColumnDefinitions[2], WidthChanged2);

                MainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions[2].Width = new GridLength(0);
            }
        }

        public void Update(int id)
        {
            var imageData = _dataStore.GetImage(id);
            var image = _model.Images.FirstOrDefault(i => i.Id == id);

            image.NSFW = imageData.NSFW;
            image.Favorite = imageData.Favorite;
            image.Rating = imageData.Rating;
            image.ForDeletion = imageData.ForDeletion;
        }
    }
}

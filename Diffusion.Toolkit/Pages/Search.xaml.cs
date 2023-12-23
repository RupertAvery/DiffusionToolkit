using Diffusion.Database;
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
using Model = Diffusion.Common.Model;
using Task = System.Threading.Tasks.Task;
using static System.String;
using Diffusion.Toolkit.Controls;
using System.Collections.Specialized;
using Diffusion.IO;
using Image = Diffusion.Database.Image;
using Diffusion.Toolkit.Common;
using Microsoft.Extensions.Options;

namespace Diffusion.Toolkit.Pages
{
    public class ReloadOptions
    {
        public bool Focus { get; set; }
        public Action? OnCompleted { get; set; }
        public bool GotoEnd { get; set; }
    }

    public class ModeSettings
    {
        public ModeSettings()
        {
            History = new List<string?>();
        }

        public string LastQuery { get; set; }
        public List<string?> History { get; set; }
        public int LastPage { get; set; }
        //public string ExtraQuery { get; set; }
        public string Name { get; set; }

        public bool IsMarkedForDeletion { get; set; }
        public bool IsFavorite { get; set; }
        public ViewMode ViewMode { get; set; }
        public string CurrentFolder { get; set; }
        //public Album CurrentAlbum { get; set; }
    }

    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Search : Page
    {
        private readonly SearchModel _model;
        private NavigatorService _navigatorService;
        private IOptions<DataStore> _dataStoreOptions;


        private DataStore DataStore => _dataStoreOptions.Value;

        private Settings _settings;
        //private readonly MainModel _mainModel;

        private ModeSettings _currentModeSettings;

        private ICollection<Model>? _modelLookup;
        public Action<string, string> Toast { get; set; }

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
        private readonly string[] _searchHints = File.ReadAllLines("hints.txt").Where(s => !IsNullOrEmpty(s.Trim())).ToArray();

        private void GetRandomHint()
        {
            var randomHint = _searchHints[r.Next(_searchHints.Length)];
            _model.SearchHint = $"Search for {randomHint}";
        }

        private Regex _gridLengthRegex = new Regex("Auto|(?<value>\\d+(?:\\.\\d+)?)(?<star>\\*)?");

        public GridLength GetGridLength(string? value)
        {
            if (IsNullOrEmpty(value)) return new GridLength(0, GridUnitType.Auto);

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

        public Search(NavigatorService navigatorService, IOptions<DataStore> dataStoreOptions, MessagePopupManager messagePopupManager, Settings settings, MainModel mainModel) : this()
        {
            this._navigatorService = navigatorService;
            this._dataStoreOptions = dataStoreOptions;

            _settings = settings;

            navigatorService.Host.Closed += async (sender, args) =>
            {
                ThumbnailLoader.Instance.Stop();
            };

            _modeSettings = new Dictionary<string, ModeSettings>()
            {
                { "search", new ModeSettings() { Name="Diffusions", ViewMode = ViewMode.Search } },
                { "folders", new ModeSettings() { Name="Folders", ViewMode = ViewMode.Folder, CurrentFolder = "$" } },
                { "albums", new ModeSettings() { Name="Albums", ViewMode = ViewMode.Album } },
                { "favorites", new ModeSettings() { Name="Favorites", ViewMode = ViewMode.Search, IsFavorite = true } },
                { "deleted", new ModeSettings() { Name="Recycle Bin", ViewMode = ViewMode.Search, IsMarkedForDeletion = true } },
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

            _model = new SearchModel(mainModel);
            //_model.DataStore = _dataStoreOptions;
            _model.Page = 0;
            _model.Pages = 0;
            _model.TotalFiles = 100;
            _model.Images = new ObservableCollection<ImageEntry>();
            _model.ShowAlbumPanel = settings.ShowAlbumPanel.GetValueOrDefault(true);
            _model.PropertyChanged += ModelOnPropertyChanged;
            _model.SearchCommand = new RelayCommand<object>((o) =>
            {
                _model.IsFilterVisible = false;
                _model.Filter.Clear();

                UseFilter = false;
                SearchImages(null);
            });

            _model.Refresh = new RelayCommand<object>((o) => ReloadMatches(null));
            _model.CurrentImage.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
            _model.CopyFiles = new RelayCommand<object>((o) => CopyFiles());

            _model.FocusSearch = new RelayCommand<object>((o) => SearchTermTextBox.Focus());
            _model.ShowDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = true);
            _model.HideDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = false);

            _model.ShowFilter = new RelayCommand<object>((o) => _model.IsFilterVisible = true);
            _model.HideFilter = new RelayCommand<object>((o) => _model.IsFilterVisible = false);
            _model.ClearSearch = new RelayCommand<object>((o) => ClearQueryFilter());

            _model.FilterCommand = new RelayCommand<object>((o) =>
            {
                _model.IsFilterVisible = false;
                UseFilter = true;
                SearchImages(null);
            });

            _model.ClearCommand = new RelayCommand<object>((o) =>
            {
                _model.Filter.Clear();

            });

            _model.OpenCommand = new RelayCommand<object>(async (o) =>
            {
                if (_currentModeSettings.ViewMode == ViewMode.Folder && _model.SelectedImageEntry.EntryType == EntryType.Folder)
                {
                    _currentModeSettings.CurrentFolder = Path.GetFullPath(_model.SelectedImageEntry.Path);

                    _model.FolderPath = _currentModeSettings.CurrentFolder;

                    SearchImages(null);
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Album && _model.SelectedImageEntry.EntryType == EntryType.Album)
                {
                    _model.MainModel.CurrentAlbum = new AlbumModel()
                    {
                        Name = _model.SelectedImageEntry.Name,
                        Id = _model.SelectedImageEntry.Id
                    };


                    SearchImages(null);
                }
                else
                {
                    OpenImage(o);
                }

            });

            _model.GoHome = new RelayCommand<object>((o) =>
            {
                if (_currentModeSettings.ViewMode == ViewMode.Folder)
                {
                    _model.FolderPath = "Watched Folders";
                    _currentModeSettings.CurrentFolder = "$";
                    SearchImages(null);
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Album)
                {
                    _model.Album = "Albums";

                    _model.MainModel.CurrentAlbum = null;

                    //_currentModeSettings.CurrentAlbum = new Album() { Id = -1, Name = "$" };
                    SearchImages(null);
                }
            });

            _model.GoUp = new RelayCommand<object>((o) =>
            {
                _currentModeSettings.CurrentFolder = Path.GetFullPath(Path.Combine(_currentModeSettings.CurrentFolder, ".."));
                _model.FolderPath = _currentModeSettings.CurrentFolder;
                SearchImages(null);
            });



            _model.PageChangedCommand = new RelayCommand<PageChangedEventArgs>((o) =>
            {
                ReloadMatches(new ReloadOptions() { Focus = true, GotoEnd = o.GotoEnd, OnCompleted = o.OnCompleted });
            });

            //var albums = DataStore.GetAlbums();

            //_model.Albums = new ObservableCollection<Album>(albums);


            SetMode("search");

            DataContext = _model;

            ThumbnailListView.DataStoreOptions = _dataStoreOptions;

            ThumbnailListView.MessagePopupManager = messagePopupManager;

            PreviewPane.MainModel = mainModel;

            PreviewPane.NSFW = (id, b) =>
            {
                DataStore.SetNSFW(id, b);
                Update(id);
            };

            PreviewPane.Favorite = (id, b) =>
            {
                DataStore.SetFavorite(id, b);
                Update(id);
            };

            PreviewPane.Rate = (id, b) =>
            {
                DataStore.SetRating(id, b);
                Update(id);
            };

            PreviewPane.Delete = (id, b) =>
            {
                DataStore.SetDeleted(id, b);
                Update(id);
            };

            FilterPopup.Closed += (sender, args) =>
            {
                ThumbnailListView.Focus();
            };

            //PreviewPane.OnNext = Next;
            //PreviewPane.OnPrev = Prev;
            GetRandomHint();
        }

        private void ClearQueryFilter()
        {
            _model.Filter.Clear();
            SearchTermTextBox.Text = "";
            SearchImages(null);
        }


        private void CopyFiles()
        {
            StringCollection paths = new StringCollection();
            foreach (var path in ThumbnailListView.SelectedImages.Select(i => i.Path))
            {
                paths.Add(path);
            }
            Clipboard.SetFileDropList(paths);
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

        public Action<ImageViewModel> OnCurrentImageOpen { get; set; }

        private void OpenImage(object obj)
        {
            OnCurrentImageOpen?.Invoke(_model.CurrentImage);
        }

        public void SearchImages()
        {
            SearchImages(null);
        }

        static T Time<T>(Func<T> action)
        {
            Stopwatch t = new Stopwatch();
            t.Start();
            T result = action();
            t.Stop();
            Debug.WriteLine($"{t.ElapsedMilliseconds}ms");
            return result;
        }

        public bool UseFilter { get; private set; }

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
                    int count = 0;
                    long size = 0;

                    if (UseFilter)
                    {
                        var filter = _model.Filter.AsFilter();

                        if (_currentModeSettings.IsFavorite)
                        {
                            filter.UseFavorite = true;
                            filter.Favorite = true;
                        }
                        else if (_currentModeSettings.IsMarkedForDeletion)
                        {
                            filter.ForDeletion = true;
                            filter.UseForDeletion = true;
                        }
                        else if (_currentModeSettings.ViewMode == ViewMode.Folder)
                        {
                            if (_currentModeSettings.CurrentFolder != "$")
                            {
                                filter.Folder = _currentModeSettings.CurrentFolder;
                            }
                        }
                        else if (_currentModeSettings.ViewMode == ViewMode.Album)
                        {
                            if (_model.MainModel.CurrentAlbum != null)
                            {
                                filter.Album = _model.MainModel.CurrentAlbum.Name;
                            }
                        }

                        count = DataStore.Count(filter);
                        size = DataStore.CountFileSize(filter);
                    }
                    else
                    {
                        if (!IsNullOrEmpty(_model.SearchText))
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
                        var query = _model.SearchText;

                        if (_currentModeSettings.IsFavorite)
                        {
                            query = $"{query} favorite: true";
                        }
                        else if (_currentModeSettings.IsMarkedForDeletion)
                        {
                            query = $"{query} delete: true";
                        }
                        else if (_currentModeSettings.ViewMode == ViewMode.Folder)
                        {
                            if (_currentModeSettings.CurrentFolder != "$")
                            {
                                query = $"{query} folder: \"{_currentModeSettings.CurrentFolder}\"";
                            }
                        }
                        else if (_currentModeSettings.ViewMode == ViewMode.Album)
                        {
                            if (_model.MainModel.CurrentAlbum != null)
                            {
                                query = $"{query} album: \"{_model.MainModel.CurrentAlbum.Name}\"";
                            }
                        }

                        count = DataStore.Count(query);
                        size = DataStore.CountFileSize(query);

                    }

                    //_model.FileSize = size;

                    _model.IsEmpty = count == 0;

                    if (_model.IsEmpty)
                    {
                        //_model.CurrentImage.;
                    }

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

                    if (_currentModeSettings.ViewMode == ViewMode.Folder)
                    {
                        if (_model.Pages == 0)
                        {
                            _model.Pages = 1;
                        }
                    }
                    else
                    {
                        if (_model.IsEmpty)
                        {
                            _model.Page = 0;
                            _model.ResultStatus = "No results found";
                            //MessageBox.Show(_navigatorService.Host, "The search term yielded no results", "No results found",
                            //    MessageBoxButton.OK,
                            //    MessageBoxImage.Information);
                            return;
                        }
                    }
                    _model.Page = 1;

                    ThumbnailListView.SetPagingEnabled();
                });



                ReloadMatches(new ReloadOptions() { Focus = (string)obj != "ManualSearch" });
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
                    LoadPreviewImage(_model.SelectedImageEntry.Path, _model.SelectedImageEntry);
                }
                else
                {
                    _model.CurrentImage = new ImageViewModel();
                    foreach (var album in _model.MainModel.Albums)
                    {
                        album.IsTicked = false;
                    }
                }
            }
            else if (e.PropertyName == nameof(SearchModel.SortBy))
            {
                ReloadMatches(new ReloadOptions() { Focus = true });
            }
            else if (e.PropertyName == nameof(SearchModel.SortDirection))
            {
                ReloadMatches(new ReloadOptions() { Focus = true });
            }

            //else if (e.PropertyName == nameof(SearchModel.Page))
            //{
            //    ReloadMatches(true);
            //}
            else if (e.PropertyName == nameof(SearchModel.SearchText))
            {
                if (IsNullOrEmpty(_model.SearchText))
                {
                    GetRandomHint();
                }

            }
        }

        public void LoadPreviewImage(string path, ImageEntry? image = null)
        {
            if (image != null && image.EntryType != EntryType.File) return;


            try
            {
                var parameters = Metadata.ReadFromFile(path);

                PreviewPane.ResetZoom();

                var old = _model.CurrentImage.IsParametersVisible;

                _model.CurrentImage = new ImageViewModel();
                _model.CurrentImage.IsParametersVisible = old;
                _model.CurrentImage.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());

                if (image != null)
                {
                    _model.CurrentImage.Id = image.Id;
                    _model.CurrentImage.Favorite = image.Favorite;
                    _model.CurrentImage.Date = image.CreatedDate.ToString();
                    _model.CurrentImage.Rating = image.Rating;
                    _model.CurrentImage.NSFW = image.NSFW;
                    _model.CurrentImage.ForDeletion = image.ForDeletion;
                    _model.CurrentImage.Albums = _dataStoreOptions.Value.GetImageAlbums(image.Id);
                    var albumLookup = _model.CurrentImage.Albums.ToDictionary(x => x.Id);

                    foreach (var album in _model.MainModel.Albums)
                    {
                        album.IsTicked = albumLookup.ContainsKey(album.Id);
                    }

                }

                Task.Run(() =>
                {
                    _model.CurrentImage.Image = GetBitmapImage(path);
                });

                _model.CurrentImage.Path = parameters.Path;
                _model.CurrentImage.Prompt = parameters.Prompt;
                _model.CurrentImage.NegativePrompt = parameters.NegativePrompt;
                _model.CurrentImage.OtherParameters = parameters.OtherParameters;

                _model.CurrentImage.ModelHash = parameters.ModelHash;
                _model.CurrentImage.Seed = parameters.Seed;
                _model.CurrentImage.AestheticScore = $"{parameters.AestheticScore}";


                if (_modelLookup != null)
                {
                    var models = _modelLookup.Where(m =>
                        !IsNullOrEmpty(parameters.ModelHash) &&
                        (String.Equals(m.Hash, parameters.ModelHash, StringComparison.CurrentCultureIgnoreCase)
                         ||
                         (m.SHA256 != null && string.Equals(m.SHA256.Substring(0, parameters.ModelHash.Length), parameters.ModelHash, StringComparison.CurrentCultureIgnoreCase))
                    ));

                    if (models.Any())
                    {
                        _model.CurrentImage.ModelName = Join(", ", models.Select(m => m.Filename));
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

                PreviewPane.ResetZoom();

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


        public void ReloadMatches(ReloadOptions? options)
        {
            Task.Run(() =>
            {
                LoadMatches();
                ThumbnailListView.ResetView(options?.Focus ?? true, options?.GotoEnd ?? false);
                Dispatcher.Invoke(() => { options?.OnCompleted?.Invoke(); });
            });
        }


        public Task ReloadMatchesAsync(bool focus = true, bool gotoEnd = false)
        {
            //await LoadMatchesAsync();

            //ThumbnailListView.ResetView(focus);
            return Task.Run(LoadMatchesAsync)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ThumbnailListView.ResetView(focus, gotoEnd);
                        });
                    }
                });
        }

        private int _startIndex = -1;

        public void StartNavigateCursor()
        {
            if (_startIndex == -1 && _model.SelectedImageEntry != null)
            {
                _startIndex = _model.Images.IndexOf(_model.SelectedImageEntry);
            }
        }

        public void EndNavigateCursor()
        {
            _startIndex = -1;
        }


        public void NavigateCursorNext()
        {
            if (_model.Images == null) return;

            int currentIndex = 0;

            if (_model.SelectedImageEntry != null)
            {
                currentIndex = _model.Images.IndexOf(_model.SelectedImageEntry);
            }

            if (currentIndex < _model.Images.Count - 1)
            {
                ThumbnailListView.ShowItem(currentIndex + 1);
                _model.SelectedImageEntry = _model.Images[currentIndex + 1];
            }
            else
            {
                if (_startIndex == _model.Images.Count - 1)
                {
                    ThumbnailListView.GoNextPage(() => _model.SelectedImageEntry = _model.Images[0]);
                    _startIndex = 0;

                }
            }

        }

        public void NavigateCursorPrevious()
        {
            if (_model.Images == null) return;
            int currentIndex = 0;
            if (_model.SelectedImageEntry != null)
            {
                currentIndex = _model.Images.IndexOf(_model.SelectedImageEntry);
            }

            if (currentIndex > 0)
            {
                ThumbnailListView.ShowItem(currentIndex - 1);
                _model.SelectedImageEntry = _model.Images[currentIndex - 1];
            }
            else
            {
                if (_startIndex == 0)
                {
                    ThumbnailListView.GoPrevPage(() => _model.SelectedImageEntry = _model.Images[^1], true);
                    _startIndex = _model.Images.Count - 1;

                }
            }

        }

        //private async Task LoadMatchesAsync()
        //{
        //    var rId = r.NextInt64();
        //    ThumbnailLoader.Instance.SetCurrentRequestId(rId);

        //    var query = _model.SearchText;

        //    if (_currentModeSettings.IsFavorite)
        //    {
        //        query = $"{query} favorite: true";
        //    }
        //    else if (_currentModeSettings.IsMarkedForDeletion)
        //    {
        //        query = $"{query} delete: true";
        //    }


        //    var matches = Time(() => DataStore
        //        .Search(query, _settings.PageSize,
        //            _settings.PageSize * (_model.Page - 1),
        //            _model.SortBy,
        //            _model.SortDirection
        //            ));

        //    Dispatcher.Invoke(() =>
        //    {
        //        _model.Images.Clear();
        //    });

        //    var images = new List<ImageEntry>();

        //    var sw = new Stopwatch();
        //    sw.Start();


        //    var count = 0;
        //    foreach (var file in matches)
        //    {
        //        images.Add(new ImageEntry(rId)
        //        {
        //            Id = file.Id,
        //            Favorite = file.Favorite,
        //            ForDeletion = file.ForDeletion,
        //            Rating = file.Rating,
        //            Path = file.Path,
        //            CreatedDate = file.CreatedDate,
        //            FileName = Path.GetFileName(file.Path),
        //            NSFW = file.NSFW
        //        });


        //        count++;
        //    }

        //    Dispatcher.Invoke(() =>
        //    {
        //        _model.Images = new ObservableCollection<ImageEntry>(images);
        //    });

        //    sw.Stop();

        //    Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds:#,###,##0}ms");

        //    foreach (var image in _model.Images)
        //    {
        //        await image.LoadThumbnail();
        //    }
        //}

        private void LoadMatches()
        {
            Dispatcher.Invoke(() =>
            {
                _model.Images?.Clear();
            });

            var images = new List<ImageEntry>();

            var rId = r.NextInt64();
            ThumbnailLoader.Instance.SetCurrentRequestId(rId);

            if (_currentModeSettings.ViewMode == ViewMode.Folder && _model.Page == 1)
            {
                IEnumerable<string> folders = Enumerable.Empty<string>();

                if (_currentModeSettings.CurrentFolder == "$")
                {
                    folders = _settings.ImagePaths;
                }
                else
                {
                    folders = new[] { Path.Combine(_currentModeSettings.CurrentFolder, "..") }.Concat(Directory.GetDirectories(_currentModeSettings.CurrentFolder));
                }

                foreach (var folder in folders)
                {
                    var imageEntry = new ImageEntry(rId)
                    {
                        Id = 0,
                        Path = folder,
                        FileName = Path.GetFileName(folder),
                        Name = Path.GetFileName(folder),
                        EntryType = EntryType.Folder
                    };

                    images.Add(imageEntry);
                    //Dispatcher.Invoke(() =>
                    //{
                    //    _model.Images.Add(imageEntry);
                    //});

                }
            }

            if (_currentModeSettings.ViewMode == ViewMode.Album)
            {

                if (_model.MainModel.CurrentAlbum == null)
                {
                    IEnumerable<Album> albums = DataStore.GetAlbums();

                    foreach (var album in albums)
                    {
                        var imageEntry = new ImageEntry(rId)
                        {
                            Id = album.Id,
                            Path = "",
                            FileName = "",
                            Name = album.Name,
                            EntryType = EntryType.Album
                        };

                        images.Add(imageEntry);

                        //Dispatcher.Invoke(() =>
                        //{
                        //    _model.Images.Add(imageEntry);
                        //});

                    }
                }

            }


            IEnumerable<Image> matches = Enumerable.Empty<Image>();

            if (UseFilter)
            {
                var filter = _model.Filter.AsFilter();
                bool showImages = true;

                if (_currentModeSettings.IsFavorite)
                {
                    filter.UseFavorite = true;
                    filter.Favorite = true;
                }
                else if (_currentModeSettings.IsMarkedForDeletion)
                {
                    filter.ForDeletion = true;
                    filter.UseForDeletion = true;
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Folder)
                {
                    if (_currentModeSettings.CurrentFolder != "$")
                    {
                        filter.Folder = _currentModeSettings.CurrentFolder;
                    }
                    else
                    {
                        showImages = false;
                    }
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Album)
                {
                    if (_model.MainModel.CurrentAlbum != null)
                    {
                        filter.Album = _model.MainModel.CurrentAlbum.Name;
                    }
                    else
                    {
                        showImages = false;
                    }
                }

                if (showImages)
                {
                    matches = Time(() => DataStore
                        .Search(filter, _settings.PageSize,
                            _settings.PageSize * (_model.Page - 1),
                            _model.SortBy,
                            _model.SortDirection));
                }
            }
            else
            {
                var query = _model.SearchText;
                bool showImages = true;

                if (_currentModeSettings.IsFavorite)
                {
                    query = $"{query} favorite: true";
                }
                else if (_currentModeSettings.IsMarkedForDeletion)
                {
                    query = $"{query} delete: true";
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Folder)
                {
                    if (_currentModeSettings.CurrentFolder != "$")
                    {
                        query = $"{query} folder: \"{_currentModeSettings.CurrentFolder}\"";
                    }
                    else
                    {
                        showImages = false;
                    }
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Album)
                {
                    if (_model.MainModel.CurrentAlbum != null)
                    {
                        query = $"{query} album: \"{_model.MainModel.CurrentAlbum.Name}\"";
                    }
                    else
                    {
                        showImages = false;
                    }
                }

                if (showImages)
                {
                    matches = Time(() => DataStore
                        .Search(query, _settings.PageSize,
                            _settings.PageSize * (_model.Page - 1),
                            _model.SortBy,
                            _model.SortDirection
                        ));
                }

            }

            //var images = new List<ImageEntry>();

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
                    Score = $"{file.AestheticScore:0.0}",
                    Path = file.Path,
                    CreatedDate = file.CreatedDate,
                    FileName = Path.GetFileName(file.Path),
                    NSFW = file.NSFW,
                    EntryType = EntryType.File,
                    AlbumCount = file.AlbumCount
                };

                images.Add(imageEntry);

                //Dispatcher.Invoke(() =>
                //{
                //    _model.Images.Add(imageEntry);
                //});

                //images.Add(imageEntry);


                count++;
            }

            //_model.Images = new ObservableCollection<ImageEntry>(images);
            //RefreshThumbnails();


            Dispatcher.Invoke(() =>
            {
                _model.Images = new ObservableCollection<ImageEntry>(images);
                RefreshThumbnails();

            });

            sw.Stop();


            Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds:#,###,##0}ms");



        }

        private async Task LoadMatchesAsync()
        {
            Dispatcher.Invoke(() =>
            {
                _model.Images.Clear();
            });

            var images = new List<ImageEntry>();

            var rId = r.NextInt64();
            ThumbnailLoader.Instance.SetCurrentRequestId(rId);

            if (_currentModeSettings.ViewMode == ViewMode.Folder && _model.Page == 1)
            {
                IEnumerable<string> folders = Enumerable.Empty<string>();

                if (_currentModeSettings.CurrentFolder == "$")
                {
                    folders = _settings.ImagePaths;
                }
                else
                {
                    folders = new[] { Path.Combine(_currentModeSettings.CurrentFolder, "..") }.Concat(Directory.GetDirectories(_currentModeSettings.CurrentFolder));
                }

                foreach (var folder in folders)
                {
                    var imageEntry = new ImageEntry(rId)
                    {
                        Id = 0,
                        Path = folder,
                        FileName = Path.GetFileName(folder),
                        Name = Path.GetFileName(folder),
                        EntryType = EntryType.Folder
                    };

                    images.Add(imageEntry);
                    //Dispatcher.Invoke(() =>
                    //{
                    //    _model.Images.Add(imageEntry);
                    //});

                }
            }

            if (_currentModeSettings.ViewMode == ViewMode.Album)
            {

                if (_model.MainModel.CurrentAlbum != null)
                {
                    IEnumerable<Album> albums = DataStore.GetAlbums();

                    foreach (var album in albums)
                    {
                        var imageEntry = new ImageEntry(rId)
                        {
                            Id = album.Id,
                            Path = "",
                            FileName = "",
                            Name = album.Name,
                            EntryType = EntryType.Album
                        };

                        images.Add(imageEntry);

                        //Dispatcher.Invoke(() =>
                        //{
                        //    _model.Images.Add(imageEntry);
                        //});

                    }
                }

            }


            IEnumerable<Image> matches = Enumerable.Empty<Image>();

            if (UseFilter)
            {
                var filter = _model.Filter.AsFilter();
                bool showImages = true;

                if (_currentModeSettings.IsFavorite)
                {
                    filter.UseFavorite = true;
                    filter.Favorite = true;
                }
                else if (_currentModeSettings.IsMarkedForDeletion)
                {
                    filter.ForDeletion = true;
                    filter.UseForDeletion = true;
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Folder)
                {
                    if (_currentModeSettings.CurrentFolder != "$")
                    {
                        filter.Folder = _currentModeSettings.CurrentFolder;
                    }
                    else
                    {
                        showImages = false;
                    }
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Album)
                {
                    if (_model.MainModel.CurrentAlbum != null)
                    {
                        filter.Album = _model.MainModel.CurrentAlbum.Name;
                    }
                    else
                    {
                        showImages = false;
                    }
                }

                if (showImages)
                {
                    matches = Time(() => DataStore
                        .Search(filter, _settings.PageSize,
                            _settings.PageSize * (_model.Page - 1),
                            _model.SortBy,
                            _model.SortDirection));
                }
            }
            else
            {
                var query = _model.SearchText;
                bool showImages = true;

                if (_currentModeSettings.IsFavorite)
                {
                    query = $"{query} favorite: true";
                }
                else if (_currentModeSettings.IsMarkedForDeletion)
                {
                    query = $"{query} delete: true";
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Folder)
                {
                    if (_currentModeSettings.CurrentFolder != "$")
                    {
                        query = $"{query} folder: \"{_currentModeSettings.CurrentFolder}\"";
                    }
                    else
                    {
                        showImages = false;
                    }
                }
                else if (_currentModeSettings.ViewMode == ViewMode.Album)
                {
                    if (_model.MainModel.CurrentAlbum != null)
                    {
                        query = $"{query} album: \"{_model.MainModel.CurrentAlbum.Name}\"";
                    }
                    else
                    {
                        showImages = false;
                    }
                }

                if (showImages)
                {
                    matches = Time(() => DataStore
                        .Search(query, _settings.PageSize,
                            _settings.PageSize * (_model.Page - 1),
                            _model.SortBy,
                            _model.SortDirection
                        ));
                }

            }

            //var images = new List<ImageEntry>();

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
                    Score = $"{file.AestheticScore:0.0}",
                    Path = file.Path,
                    CreatedDate = file.CreatedDate,
                    FileName = Path.GetFileName(file.Path),
                    NSFW = file.NSFW,
                    EntryType = EntryType.File
                };

                images.Add(imageEntry);

                //Dispatcher.Invoke(() =>
                //{
                //    _model.Images.Add(imageEntry);
                //});

                //images.Add(imageEntry);


                count++;
            }

            Dispatcher.Invoke(() =>
            {
                _model.Images = new ObservableCollection<ImageEntry>(images);
                RefreshThumbnails();
            });


            sw.Stop();


            Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds:#,###,##0}ms");



        }

        public void RefreshThumbnails()
        {
            if (_model.Images != null)
            {
                foreach (var image in _model.Images)
                {
                    image.LoadThumbnail();
                }
            }
        }

        private void Page_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReloadMatches(null);
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

        public void SetMode(string mode)
        {
            _currentModeSettings = GetModeSettings(mode);
            _model.IsFilterVisible = false;


            if (_currentModeSettings.IsFavorite)
            {
                _model.Filter.UseFavorite = true;
                _model.Filter.Favorite = true;
                _model.Filter.UseForDeletion = false;
                _model.Filter.ForDeletion = false;
            }
            else if (_currentModeSettings.IsMarkedForDeletion)
            {
                _model.Filter.UseFavorite = false;
                _model.Filter.Favorite = false;
                _model.Filter.UseForDeletion = true;
                _model.Filter.ForDeletion = true;
            }
            else
            {
                _model.Filter.UseFavorite = false;
                _model.Filter.Favorite = false;
                _model.Filter.UseForDeletion = false;
                _model.Filter.ForDeletion = false;
            }

            _model.CurrentViewMode = _currentModeSettings.ViewMode;
            _model.SearchText = _currentModeSettings.LastQuery;
            _model.SearchHistory = new ObservableCollection<string?>(_currentModeSettings.History);
            _model.ModeName = _currentModeSettings.Name;
        }

        public void ShowSearch()
        {
            SetMode("search");
            SearchImages(null);
        }

        public void ShowFolders()
        {
            SetMode("folders");
            SearchImages(null);
        }

        public void ShowAlbums()
        {
            SetMode("albums");
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

        public void SetThumbnailSize(int thumbnailSize)
        {
            ThumbnailListView.SetThumbnailSize(thumbnailSize);
            RefreshThumbnails();
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
            var imageData = DataStore.GetImage(id);
            var image = _model.Images.FirstOrDefault(i => i.Id == id);

            if (image != null)
            {
                image.NSFW = imageData.NSFW;
                image.Favorite = imageData.Favorite;
                image.Rating = imageData.Rating;
                image.ForDeletion = imageData.ForDeletion;
            }
        }

        private void PreviewPane_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (!e.Data.GetDataPresent("DTCustomDragSource"))
                {
                    // Note that you can have more than one file.
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    // Assuming you have one file that you care about, pass it off to whatever
                    // handling code you have defined.
                    LoadPreviewImage(files[0]);
                }
            }

        }

        public string? Prompt => _model.SearchText;

        public Filter Filter => _model.Filter.AsFilter();

        public bool IsQueryEmpty()
        {
            if (UseFilter)
            {
                return _model.Filter.AsFilter().IsEmpty;
            }
            else
            {
                return _model.SearchText == null || _model.SearchText.Trim().Length == 0;
            }
        }

        private void FolderPath_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_model.FolderPath != null && Directory.Exists(_model.FolderPath))
                {
                    _currentModeSettings.CurrentFolder = _model.FolderPath;

                    SearchImages(null);
                }
            }
        }

        public void SetShowAlbumPanel(bool showAlbumPanel)
        {
            _model.ShowAlbumPanel = showAlbumPanel;
        }

        public void ExtOnKeyUp(object sender, KeyEventArgs e)
        {
            EndNavigateCursor();
        }

        public void ExtOnKeyDown(object sender, KeyEventArgs e)
        {
            StartNavigateCursor();
            if (e.Key == Key.Left)
            {
                NavigateCursorPrevious();
            }
            else if (e.Key == Key.Right)
            {
                NavigateCursorNext();
            }
        }

        private void PreviewPane_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            ExtOnKeyUp(this, e);
        }

        private void PreviewPane_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ExtOnKeyDown(this, e);
        }

        public void SetPageSize(int pageSize)
        {
            ThumbnailListView.PageSize = pageSize;
        }

        private void PreviewPane_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void RenameAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            _model.MainModel.RemoveAlbumCommand.Execute(null);
        }

        private void DropImagesOnAlbum(object sender, DragEventArgs e)
        {
            var album = (AlbumModel)((FrameworkElement)sender).DataContext;
            _model.MainModel.AddSelectedImagesToAlbum(album);
        }

        private void RemoveAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            _model.MainModel.RemoveAlbumCommand.Execute(null);
        }

        private void ListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //SetMode("albums");
            //SearchImages(null);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _model.MainModel.CurrentAlbum = ((AlbumModel)((Button)sender).DataContext);
            SetMode("albums");
            SearchImages(null);
        }

        private void FilterPopup_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _model.IsFilterVisible = false;
            }
        }
    }
}

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
using System.Text.Json;
using System.Threading;
using Diffusion.IO;
using Image = Diffusion.Database.Image;
using Diffusion.Toolkit.Common;
using Microsoft.Extensions.Options;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Themes;
using static System.Net.WebRequestMethods;
using WPFLocalizeExtension.Engine;
using System.Windows.Documents;
using System.Windows.Media;
using Diffusion.Toolkit.Services;

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
        private Dictionary<string, ModeSettings> _modeSettings = new Dictionary<string, ModeSettings>();
        private readonly MessagePopupManager _messagePopupManager;

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

            ServiceLocator.ThumbnailNavigationService.Next += ThumbnailNavigationServiceOnNext;
            ServiceLocator.ThumbnailNavigationService.Previous +=  ThumbnailNavigationServiceOnPrevious;
            ServiceLocator.ThumbnailNavigationService.NextPage += ThumbnailNavigationServiceOnNextPage;
            ServiceLocator.ThumbnailNavigationService.PreviousPage += ThumbnailNavigationServiceOnPreviousPage;

            //var str = new System.Text.StringBuilder();
            //using (var writer = new System.IO.StringWriter(str))
            //    System.Windows.Markup.XamlWriter.Save(MyContextMenu.Template, writer);
            //System.Diagnostics.Debug.Write(str);
            ServiceLocator.SearchService.Search += (obj, args) =>
            {
                SearchImages(null);
            };
        }

        private void ThumbnailNavigationServiceOnPreviousPage(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ThumbnailNavigationServiceOnNextPage(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ThumbnailNavigationServiceOnPrevious(object? sender, EventArgs e)
        {
            StartNavigateCursor();
            NavigateCursorPrevious();
            EndNavigateCursor();
        }

        private void ThumbnailNavigationServiceOnNext(object? sender, EventArgs e)
        {
            StartNavigateCursor();
            NavigateCursorNext();
            EndNavigateCursor();
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
            if (IsNullOrEmpty(value)) return new GridLength(1, GridUnitType.Star);

            if (value == "*") return new GridLength(1, GridUnitType.Star);

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

            if (_settings.MainGridWidth != null)
            {
                MainGrid.ColumnDefinitions[0].Width = GetGridLength(_settings.MainGridWidth);
                MainGrid.ColumnDefinitions[2].Width = GetGridLength(_settings.MainGridWidth2);
            }

            if (_settings.NavigationThumbnailGridWidth != null)
            {
                NavigationThumbnailGrid.ColumnDefinitions[0].Width = GetGridLength(_settings.NavigationThumbnailGridWidth);
                NavigationThumbnailGrid.ColumnDefinitions[2].Width = GetGridLength(_settings.NavigationThumbnailGridWidth2);
            }

            if (_settings.PreviewGridHeight != null)
            {
                PreviewGrid.RowDefinitions[0].Height = GetGridLength(_settings.PreviewGridHeight);
                PreviewGrid.RowDefinitions[2].Height = GetGridLength(_settings.PreviewGridHeight2);
            }

            var widthDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
            widthDescriptor.AddValueChanged(MainGrid.ColumnDefinitions[0], WidthChanged);
            widthDescriptor.AddValueChanged(MainGrid.ColumnDefinitions[2], WidthChanged2);

            var navThumbWidthDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
            navThumbWidthDescriptor.AddValueChanged(NavigationThumbnailGrid.ColumnDefinitions[0], NavThumbWidthChanged);
            navThumbWidthDescriptor.AddValueChanged(NavigationThumbnailGrid.ColumnDefinitions[2], NavThumbWidthChanged2);

            var heightDescriptor = DependencyPropertyDescriptor.FromProperty(RowDefinition.HeightProperty, typeof(ItemsControl));
            heightDescriptor.AddValueChanged(PreviewGrid.RowDefinitions[0], HeightChanged);
            heightDescriptor.AddValueChanged(PreviewGrid.RowDefinitions[2], HeightChanged2);

            _model = new SearchModel(mainModel);
            //_model.DataStore = _dataStoreOptions;
            _model.Page = 0;
            _model.Pages = 0;
            _model.TotalFiles = 100;
            _model.Images = new ObservableCollection<ImageEntry>();
            _model.PropertyChanged += ModelOnPropertyChanged;
            _model.SearchCommand = new RelayCommand<object>((o) =>
            {
                _model.IsFilterVisible = false;
                _model.Filter.Clear();

                UseFilter = false;
                SearchImages(null);
            });

            _model.Refresh = new RelayCommand<object>((o) =>
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    SearchImages(null);
                }
                else
                {
                    ReloadMatches(null);
                }
            });
            _model.CurrentImage.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
            _model.CopyFiles = new RelayCommand<object>((o) => CopyFiles(ThumbnailListView.SelectedImages));

            ThumbnailListView.CopyFiles = CopyFiles;


            _model.FocusSearch = new RelayCommand<object>((o) => SearchTermTextBox.Focus());
            _model.ShowDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = true);
            _model.HideDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = false);

            _model.ShowFilter = new RelayCommand<object>((o) => ShowFilter());
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

                    ExpandToPath(_model.FolderPath);

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

            void PopulateSortOptions()
            {
                _modeSettings = new Dictionary<string, ModeSettings>()
                {
                    { "search", new ModeSettings() { Name = GetLocalizedText("Search.Diffusions"), ViewMode = ViewMode.Search } },
                    { "models", new ModeSettings() { Name = GetLocalizedText("Search.Models"), ViewMode = ViewMode.Model } },
                    { "folders", new ModeSettings() { Name = GetLocalizedText("Search.Folders"), ViewMode = ViewMode.Folder, CurrentFolder = "$" } },
                    { "albums", new ModeSettings() { Name = GetLocalizedText("Search.Albums"), ViewMode = ViewMode.Album } },
                    { "favorites", new ModeSettings() { Name = GetLocalizedText("Search.Favorites"), ViewMode = ViewMode.Search, IsFavorite = true } },
                    { "deleted", new ModeSettings() { Name = GetLocalizedText("Search.RecycleBin"), ViewMode = ViewMode.Search, IsMarkedForDeletion = true } },
                };


                _model.SortOptions = new List<OptionValue>()
                {
                    new(GetLocalizedText("Search.SortBy.DateCreated"), "Date Created"),
                    new(GetLocalizedText("Search.SortBy.DateModified"), "Date Modified"),
                    new(GetLocalizedText("Search.SortBy.Rating"), "Rating"),
                    new(GetLocalizedText("Search.SortBy.AestheticScore"), "Aesthetic Score"),
                    new(GetLocalizedText("Search.SortBy.Name"), "Name"),
                    new(GetLocalizedText("Search.SortBy.Prompt"), "Prompt"),
                    new(GetLocalizedText("Search.SortBy.Random"), "Random"),
                };

                _model.SortOrderOptions = new List<OptionValue>()
                {
                    new(GetLocalizedText("Search.SortBy.Ascending"), "A-Z"),
                    new(GetLocalizedText("Search.SortBy.Descending"), "Z-A")
                };

            }

            LocalizeDictionary.Instance.PropertyChanged += (sender, args) =>
            {
                var sortBy = _model.SortBy;
                var sortDirection = _model.SortDirection;
                PopulateSortOptions();
                _model.SortBy = sortBy;
                _model.SortDirection = sortDirection;

                SearchImages(null);
                SetMode(_model.CurrentMode);
                _model.MainModel.Status = "";
            };

            PopulateSortOptions();

            _model.SortBy = _settings.SortBy;
            _model.SortDirection = _settings.SortDirection;


            _model.MetadataSection.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(MetadataSection.PromptState):
                        _settings.MetadataSection.PromptState = _model.MetadataSection.PromptState;
                        break;
                    case nameof(MetadataSection.NegativePromptState):
                        _settings.MetadataSection.NegativePromptState = _model.MetadataSection.NegativePromptState;
                        break;
                    case nameof(MetadataSection.SeedState):
                        _settings.MetadataSection.SeedState = _model.MetadataSection.SeedState;
                        break;
                    case nameof(MetadataSection.SamplerState):
                        _settings.MetadataSection.SamplerState = _model.MetadataSection.SamplerState;
                        break;
                    case nameof(MetadataSection.OthersState):
                        _settings.MetadataSection.OthersState = _model.MetadataSection.OthersState;
                        break;
                    case nameof(MetadataSection.ModelState):
                        _settings.MetadataSection.ModelState = _model.MetadataSection.ModelState;
                        break;
                    case nameof(MetadataSection.PathState):
                        _settings.MetadataSection.PathState = _model.MetadataSection.PathState;
                        break;
                    case nameof(MetadataSection.DateState):
                        _settings.MetadataSection.DateState = _model.MetadataSection.DateState;
                        break;
                    case nameof(MetadataSection.AlbumState):
                        _settings.MetadataSection.AlbumState = _model.MetadataSection.AlbumState;
                        break;
                }
            };

            _model.MetadataSection.PromptState = _settings.MetadataSection.PromptState;
            _model.MetadataSection.NegativePromptState = _settings.MetadataSection.NegativePromptState;
            _model.MetadataSection.SeedState = _settings.MetadataSection.SeedState;
            _model.MetadataSection.SamplerState = _settings.MetadataSection.SamplerState;
            _model.MetadataSection.OthersState = _settings.MetadataSection.OthersState;
            _model.MetadataSection.ModelState = _settings.MetadataSection.ModelState;
            _model.MetadataSection.PathState = _settings.MetadataSection.PathState;
            _model.MetadataSection.DateState = _settings.MetadataSection.DateState;
            _model.MetadataSection.AlbumState = _settings.MetadataSection.AlbumState;


            _model.MainModel.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(MainModel.Albums):
                        ThumbnailListView.ReloadAlbums();
                        break;
                }
            };

            _model.MainModel.Settings.NavigationSection.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(NavigationSectionSettings.ShowFolders):
                    case nameof(NavigationSectionSettings.ShowModels):
                    case nameof(NavigationSectionSettings.ShowAlbums):
                        SetAccordionResizeableState();
                        break;

                    case nameof(NavigationSectionSettings.ShowSection):
                        SetNavigationVisible(_model.MainModel.Settings.NavigationSection.ShowSection);
                        SetAccordionResizeableState();
                        break;
                }
            };

            _model.NavigationSection.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(NavigationSection.FolderState):
                        _settings.NavigationSection.FolderState = _model.NavigationSection.FolderState;
                        break;
                    case nameof(NavigationSection.ModelState):
                        _settings.NavigationSection.ModelState = _model.NavigationSection.ModelState;
                        break;
                    case nameof(NavigationSection.AlbumState):
                        _settings.NavigationSection.AlbumState = _model.NavigationSection.AlbumState;
                        break;

                    case nameof(NavigationSection.FolderHeight):
                        _settings.NavigationSection.FolderHeight = _model.NavigationSection.FolderHeight;
                        break;
                    case nameof(NavigationSection.ModelHeight):
                        _settings.NavigationSection.ModelHeight = _model.NavigationSection.ModelHeight;
                        break;
                    case nameof(NavigationSection.AlbumHeight):
                        _settings.NavigationSection.AlbumHeight = _model.NavigationSection.AlbumHeight;
                        break;
                        //case nameof(NavigationSection.ShowFolders):
                        //    _settings.NavigationSection.ShowFolders = _model.NavigationSection.ShowFolders;
                        //    break;
                        //case nameof(NavigationSection.ShowModels):
                        //    _settings.NavigationSection.ShowModels = _model.NavigationSection.ShowModels;
                        //    break;
                        //case nameof(NavigationSection.ShowAlbums):
                        //    _settings.NavigationSection.ShowAlbums = _model.NavigationSection.ShowAlbums;
                        //    break;
                }
            };

            _model.NavigationSection.FolderState = _settings.NavigationSection.FolderState;
            _model.NavigationSection.ModelState = _settings.NavigationSection.ModelState;
            _model.NavigationSection.AlbumState = _settings.NavigationSection.AlbumState;

            _model.NavigationSection.FolderHeight = _settings.NavigationSection.FolderHeight;
            _model.NavigationSection.ModelHeight = _settings.NavigationSection.ModelHeight;
            _model.NavigationSection.AlbumHeight = _settings.NavigationSection.AlbumHeight;

            //_model.NavigationSection.ShowFolders = _settings.NavigationSection.ShowFolders;
            //_model.NavigationSection.ShowModels = _settings.NavigationSection.ShowModels;
            //_model.NavigationSection.ShowAlbums = _settings.NavigationSection.ShowAlbums;
            this.Loaded += (sender, args) =>
            {
                SetAccordionResizeableState();
            };

            SetNavigationVisible(_model.MainModel.Settings.NavigationSection.ShowSection);

            SetMode("search");

            DataContext = _model;

            ThumbnailListView.DataStoreOptions = _dataStoreOptions;

            ThumbnailListView.MessagePopupManager = messagePopupManager;

            _messagePopupManager = messagePopupManager;

            ThumbnailListView.OnExpandToFolder = entry =>
            {
                ExpandToPath(Path.GetDirectoryName(entry.Path));
            };

            PreviewPane.MainModel = mainModel;

            PreviewPane.NSFW = (id, b) =>
            {
                DataStore.SetNSFW(id, b);
                Update(id);

                AdvanceOnTag();
            };

            PreviewPane.Favorite = (id, b) =>
            {
                DataStore.SetFavorite(id, b);
                Update(id);

                AdvanceOnTag();
            };

            PreviewPane.Rate = (id, b) =>
            {
                DataStore.SetRating(id, b);
                Update(id);

                AdvanceOnTag();
            };

            PreviewPane.Delete = (id, b) =>
            {
                DataStore.SetDeleted(id, b);
                Update(id);

                AdvanceOnTag();
            };

            FilterPopup.Closed += (sender, args) =>
            {
                ThumbnailListView.Focus();
            };

            //PreviewPane.OnNext = Next;
            //PreviewPane.OnPrev = Prev;
            GetRandomHint();
        }

        private void AdvanceOnTag()
        {
            if (ServiceLocator.Settings.AutoAdvance)
            {
                ServiceLocator.ThumbnailNavigationService.MoveNext();
            }
        }

        public void ShowFilter()
        {
            _model.IsFilterVisible = true;
        }

        private void ClearQueryFilter()
        {
            _model.Filter.Clear();
            SearchTermTextBox.Text = "";
            SearchImages(null);
        }


        private void CopyFiles(IEnumerable<ImageEntry> images)
        {
            StringCollection paths = new StringCollection();
            foreach (var path in images.Select(i => i.Path))
            {
                paths.Add(path);
            }
            Clipboard.SetFileDropList(paths);
        }

        private string GetLocalizedText(string key)
        {
            return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
        }

        private void WidthChanged(object? sender, EventArgs e)
        {
            _settings.MainGridWidth = MainGrid.ColumnDefinitions[0].Width.ToString();
        }

        private void WidthChanged2(object? sender, EventArgs e)
        {
            _settings.MainGridWidth2 = MainGrid.ColumnDefinitions[2].Width.ToString();
        }

        private void NavThumbWidthChanged(object? sender, EventArgs e)
        {
            _settings.NavigationThumbnailGridWidth = NavigationThumbnailGrid.ColumnDefinitions[0].Width.ToString();
        }

        private void NavThumbWidthChanged2(object? sender, EventArgs e)
        {
            _settings.NavigationThumbnailGridWidth2 = NavigationThumbnailGrid.ColumnDefinitions[2].Width.ToString();
        }

        private void HeightChanged(object? sender, EventArgs e)
        {
            _settings.PreviewGridHeight = PreviewGrid.RowDefinitions[0].Height.ToString();
        }

        private void HeightChanged2(object? sender, EventArgs e)
        {
            _settings.PreviewGridHeight2 = PreviewGrid.RowDefinitions[2].Height.ToString();
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
        
        //public Action<IList<ImageEntry>> CopyFiles
        //{
        //    get => ThumbnailListView.CopyFiles;
        //    set => ThumbnailListView.CopyFiles = value;
        //}

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
                MessageBox.Show(GetLocalizedText("Messages.Errors.NoImagePaths"), GetLocalizedText("Messages.Captions.Error"),
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
                        else if (_currentModeSettings.ViewMode == ViewMode.Model)
                        {
                            if (_model.MainModel.CurrentAlbum != null)
                            {
                                filter.ModelHash = _model.MainModel.CurrentModel.Hash;
                                filter.ModelName = _model.MainModel.CurrentModel.Name;
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
                        else if (_currentModeSettings.ViewMode == ViewMode.Model)
                        {
                            if (_model.MainModel.CurrentModel != null)
                            {
                                query = $"{query} model_or_hash: \"{_model.MainModel.CurrentModel.Name}\"|{_model.MainModel.CurrentModel.Hash}";
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

                    var ssize = $"{fsize:n} B";

                    if (fsize > 1073741824)
                    {
                        fsize /= 1073741824;
                        ssize = $"{fsize:n2} GiB";
                    }
                    else if (fsize > 1048576)
                    {
                        fsize /= 1048576;
                        ssize = $"{fsize:n2} MiB";
                    }
                    else if (fsize > 1024)
                    {
                        fsize /= 1024;
                        ssize = $"{fsize:n2} KiB";
                    }

                    var text = GetLocalizedText("Search.Results");

                    text = text.Replace("{count}", $"{count:n0}")
                        .Replace("{size}", $"{ssize}");

                    _model.Results = text;

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
                            var noResults = GetLocalizedText("Search.NoResults");

                            _model.Page = 0;
                            _model.ResultStatus = noResults;
                            //MessageBox.Show(_navigatorService.Host, "The search term yielded no results", "No results found",
                            //    MessageBoxButton.OK,
                            //    MessageBoxImage.Information);
                            return;
                        }
                    }
                    _model.Page = 1;
                    ThumbnailListView.Model.Pages = _model.Pages;
                    ThumbnailListView.Model.Page = _model.Page;

                    ThumbnailListView.SetPagingEnabled();
                });



                ReloadMatches(new ReloadOptions() { Focus = (string)obj != "ManualSearch" });
            }
            catch (Exception e)
            {
                MessageBox.Show(_navigatorService.Host, e.Message, GetLocalizedText("Messages.Captions.Error"),
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
                _settings.SortBy = _model.SortBy;

                ReloadMatches(new ReloadOptions() { Focus = true });
            }
            else if (e.PropertyName == nameof(SearchModel.SortDirection))
            {
                _settings.SortDirection = _model.SortDirection;

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

        private CancellationTokenSource? _loadPreviewBitmapCts;

        public void LoadPreviewImage(string path, ImageEntry? image = null)
        {
            if (image != null && image.EntryType != EntryType.File) return;


            try
            {
                if (_loadPreviewBitmapCts != null)
                {
                    _loadPreviewBitmapCts.Cancel();
                }

                _loadPreviewBitmapCts = new CancellationTokenSource();

                if (!File.Exists(path))
                {
                    var emptyModel = new ImageViewModel();
                    emptyModel.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
                    emptyModel.Path = path;
                    emptyModel.IsMessageVisible = true;
                    emptyModel.Message = GetLocalizedText("Search.LoadPreview.MediaUnavailable");

                    _model.CurrentImage = emptyModel;

                    PreviewPane.ResetZoom();

                    return;
                }

                var parameters = Metadata.ReadFromFile(path);

                var old = _model.CurrentImage.IsParametersVisible;

                var imageViewModel = new ImageViewModel();
                imageViewModel.IsParametersVisible = old;
                imageViewModel.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
                imageViewModel.OpenAlbumCommand = new RelayCommand<Album>(OpenAlbum);
                imageViewModel.RemoveFromAlbumCommand = new RelayCommand<Album>(RemoveFromAlbum);

                if (image != null)
                {
                    imageViewModel.Id = image.Id;
                    imageViewModel.Favorite = image.Favorite;
                    imageViewModel.Date = image.CreatedDate.ToString("G", LocalizeDictionary.CurrentCulture);
                    imageViewModel.Rating = image.Rating;
                    imageViewModel.NSFW = image.NSFW;
                    imageViewModel.ForDeletion = image.ForDeletion;
                    imageViewModel.Albums = _dataStoreOptions.Value.GetImageAlbums(image.Id);
                    var albumLookup = imageViewModel.Albums.ToDictionary(x => x.Id);

                    foreach (var album in _model.MainModel.Albums)
                    {
                        album.IsTicked = albumLookup.ContainsKey(album.Id);
                    }

                }

                Task.Run(() =>
                {
                    imageViewModel.IsLoading = true;

                    var sourceImage = GetBitmapImage(path);

                    Dispatcher.Invoke(() =>
                    {
                        _model.CurrentImage.Image = sourceImage;
                        imageViewModel.IsLoading = false;
                    });

                }, _loadPreviewBitmapCts.Token);

                if (parameters != null)
                {
                    imageViewModel.HasError = parameters.HasError;
                    imageViewModel.ErrorMessage = parameters.ErrorMessage;

                    imageViewModel.Path = parameters.Path;
                    imageViewModel.Prompt = parameters.Prompt?.Trim();
                    imageViewModel.NegativePrompt = parameters.NegativePrompt?.Trim();
                    imageViewModel.OtherParameters = parameters.OtherParameters?.Trim();
                    imageViewModel.CFGScale = parameters.CFGScale;
                    imageViewModel.Steps = parameters.Steps;
                    imageViewModel.Sampler = parameters.Sampler;

                    imageViewModel.Width = parameters.Width;
                    imageViewModel.Height = parameters.Height;

                    imageViewModel.ModelHash = parameters.ModelHash;
                    imageViewModel.Seed = parameters.Seed;
                    imageViewModel.AestheticScore = $"{parameters.AestheticScore}";

                    imageViewModel.Workflow = parameters.Workflow;

                    var parser = new ComfyUIParser();
                    imageViewModel.Nodes = parser.Parse(parameters.WorkflowId, parameters.Workflow);


                    var notFound = GetLocalizedText("Metadata.Modelname.NotFound");

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
                            imageViewModel.ModelName = Join(", ", models.Select(m => m.Filename).Distinct());
                        }
                        else
                        {
                            imageViewModel.ModelName = notFound;
                        }
                    }
                    else
                    {
                        imageViewModel.ModelName = notFound;
                    }

                }

                _model.CurrentImage = imageViewModel;

                //PreviewPane.ResetZoom();

                OnCurrentImageChange?.Invoke(_model.CurrentImage);
            }
            catch (FileNotFoundException)
            {
                var notFound = GetLocalizedText("Search.LoadPreview.ImageNotFound");
                var caption = GetLocalizedText("Search.LoadPreview.ImageNotFound.Caption");

                MessageBox.Show(_navigatorService.Host, notFound, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
                Dispatcher.Invoke(() =>
                {
                    options?.OnCompleted?.Invoke();

                    if (_model.Images is { Count: > 0 })
                    {
                        if (options?.GotoEnd ?? false)
                        {
                            _model.SelectedImageEntry = _model.Images[^1];
                        }
                        else
                        {
                            _model.SelectedImageEntry = _model.Images[0];
                        }
                    }
                    else
                    {
                        _model.SelectedImageEntry = null;
                    }

                });


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


        public void Advance()
        {
            StartNavigateCursor();
            NavigateCursorNext();
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
                ThumbnailListView.ThumbnailListView.SelectedItem = _model.SelectedImageEntry;
            }
            else
            {
                if (_startIndex == _model.Images.Count - 1)
                {
                    ThumbnailListView.GoNextPage(() =>
                    {
                        _model.SelectedImageEntry = _model.Images[0];
                        ThumbnailListView.ThumbnailListView.SelectedItem = _model.SelectedImageEntry;
                        NavigationCompleted?.Invoke(this, new EventArgs());
                    });
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
                ThumbnailListView.ThumbnailListView.SelectedItem = _model.SelectedImageEntry;
            }
            else
            {
                if (_startIndex == 0)
                {
                    ThumbnailListView.GoPrevPage(() =>
                    {
                        _model.SelectedImageEntry = _model.Images[^1];
                        ThumbnailListView.ThumbnailListView.SelectedItem = _model.SelectedImageEntry;
                        NavigationCompleted?.Invoke(this, new EventArgs());
                    }, true);
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
            //Dispatcher.Invoke(() =>
            //{
            //    _model.Images?.Clear();
            //});

            var images = new List<ImageEntry>();

            var rId = r.NextInt64();
            ThumbnailLoader.Instance.SetCurrentRequestId(rId);

            if (_currentModeSettings != null && _currentModeSettings.ViewMode == ViewMode.Folder && _model.Page == 1)
            {
                IEnumerable<string> folders = Enumerable.Empty<string>();

                if (_currentModeSettings.CurrentFolder == "$")
                {
                    folders = _settings.ImagePaths;
                }
                else
                {
                    if (!Directory.Exists(_currentModeSettings.CurrentFolder))
                    {
                        MessageBox.Show(GetLocalizedText("Search.Folders.Unavailable"), GetLocalizedText("Search.Folders.Unavailable.TItle"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!_settings.ImagePaths.Contains(_currentModeSettings.CurrentFolder))
                    {
                        folders = folders.Concat(new[] { Path.Combine(_currentModeSettings.CurrentFolder, "..") });
                    }

                    folders = folders.Concat(Directory.GetDirectories(_currentModeSettings.CurrentFolder));
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

            if (_currentModeSettings != null && _currentModeSettings.ViewMode == ViewMode.Album)
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


            IEnumerable<ImageView> matches = Enumerable.Empty<ImageView>();

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
                else if (_currentModeSettings.ViewMode == ViewMode.Model)
                {
                    if (_model.MainModel.CurrentModel != null)
                    {
                        filter.ModelHash = _model.MainModel.CurrentModel.Hash;
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

                if (_currentModeSettings != null)
                {
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
                    else if (_currentModeSettings.ViewMode == ViewMode.Model)
                    {
                        if (_model.MainModel.CurrentModel != null)
                        {
                            query = $"{query} model_or_hash: \"{_model.MainModel.CurrentModel.Name}\"|{_model.MainModel.CurrentModel.Hash}";
                        }
                        else
                        {
                            showImages = false;
                        }
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
                    AlbumCount = file.AlbumCount,
                    Dispatcher = Dispatcher,
                    HasError = file.HasError
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
                if (_model.Images == null || _model.Images.Count != images.Count)
                {
                    _model.Images = new ObservableCollection<ImageEntry>(images);
                }
                else
                {
                    for (var i = 0; i < images.Count; i++)
                    {
                        _model.Images[i] = images[i];
                    }
                }

                ThumbnailListView.ReloadThumbnailsView(0);

                //RefreshThumbnails();

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
                else if (_currentModeSettings.ViewMode == ViewMode.Model)
                {
                    if (_model.MainModel.CurrentModel != null)
                    {
                        filter.ModelHash = _model.MainModel.CurrentModel.Hash;
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
                else if (_currentModeSettings.ViewMode == ViewMode.Model)
                {
                    if (_model.MainModel.CurrentModel != null)
                    {
                        query = $"{query} model_or_hash: \"{_model.MainModel.CurrentModel.Name}\"|{_model.MainModel.CurrentModel.Hash}";
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
                    Dispatcher = Dispatcher,
                    HasError = file.HasError
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
                if (_model.Images == null || _model.Images.Count != images.Count)
                {
                    _model.Images = new ObservableCollection<ImageEntry>(images);
                }
                else
                {
                    for (var i = 0; i < images.Count; i++)
                    {
                        _model.Images[i] = images[i];
                    }
                }
                //RefreshThumbnails();
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

        private ModeSettings GetModeSettings(string mode)
        {
            if (!_modeSettings.TryGetValue(mode, out var settings))
            {
                settings = new ModeSettings();
            }
            return settings;
        }

        public void SetMode(string mode, string? context = null)
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

            _model.CurrentMode = mode;
            _model.CurrentViewMode = _currentModeSettings.ViewMode;
            _model.SearchText = _currentModeSettings.LastQuery;

            _model.SearchHistory = new ObservableCollection<string?>(_currentModeSettings.History);

            if (context != null)
            {
                _model.ModeName = $"{_currentModeSettings.Name} - {context}";
            }
            else
            {
                _model.ModeName = _currentModeSettings.Name;
            }
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

        private void SetAccordionResizeableState()
        {
            var stackPanel = GetChildOfType<StackPanel>(NavigationScrollViewer);
            if (stackPanel != null)
            {
                var children = GetChildrenOfType<AccordionControl>(stackPanel);
                var items = children.Where(c => c.Visibility == Visibility.Visible).ToList();
                for (var i = 0; i < items.Count; i++)
                {
                    items[i].CanResize = i < items.Count - 1;
                }
            }
        }

        private static T? GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private static IEnumerable<T> GetChildrenOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T result) yield return result;
            }
        }

        public void SetNavigationVisible(bool visible)
        {
            var old = NavigationScrollViewer.Visibility;

            NavigationScrollViewer.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            GridSplitter2.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

            if (NavigationScrollViewer.Visibility == old) return;

            if (visible)
            {
                //NavigationThumbnailGrid.ColumnDefinitions[0].Width = GetGridLength(_settings.NavigationThumbnailGridWidth);
                //NavigationThumbnailGrid.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Auto);

                NavigationThumbnailGrid.ColumnDefinitions[0].Width = GetGridLength(_settings.NavigationThumbnailGridWidth);
                NavigationThumbnailGrid.ColumnDefinitions[2].Width = GetGridLength(_settings.NavigationThumbnailGridWidth2);

                var widthDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
                widthDescriptor.AddValueChanged(NavigationThumbnailGrid.ColumnDefinitions[0], NavThumbWidthChanged);
                widthDescriptor.AddValueChanged(NavigationThumbnailGrid.ColumnDefinitions[2], NavThumbWidthChanged2);
            }
            else
            {
                var widthDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
                widthDescriptor.RemoveValueChanged(NavigationThumbnailGrid.ColumnDefinitions[0], NavThumbWidthChanged);
                widthDescriptor.RemoveValueChanged(NavigationThumbnailGrid.ColumnDefinitions[2], NavThumbWidthChanged2);

                NavigationThumbnailGrid.ColumnDefinitions[0].Width = new GridLength(0);
                NavigationThumbnailGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            }
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
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
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

        public void ExtOnKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Left)
                {
                    EndNavigateCursor();
                }
                else if (e.Key == Key.Right)
                {
                    EndNavigateCursor();
                }
                else if (e.Key == Key.Delete)
                {
                    AdvanceOnTag();
                }
                else if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    AdvanceOnTag();
                }
                else if (e.Key == Key.F)
                {
                    AdvanceOnTag();
                }
                else if (e.Key == Key.F)
                {
                    AdvanceOnTag();
                }
            }
        }

        public event EventHandler NavigationCompleted;

        public void ExtOnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Left)
                {
                    StartNavigateCursor();
                    NavigateCursorPrevious();
                }
                else if (e.Key == Key.Right)
                {
                    StartNavigateCursor();
                    NavigateCursorNext();
                }
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

        private void Album_OnClick(object sender, RoutedEventArgs e)
        {
            var model = ((AlbumModel)((Button)sender).DataContext);

            if (_model.MainModel.CurrentAlbum != null)
            {
                _model.MainModel.CurrentAlbum.IsSelected = false;
            }

            _model.MainModel.CurrentAlbum = model;

            _model.MainModel.CurrentAlbum.IsSelected = true;

            _model.MainModel.ActiveView = "Albums";
            SetMode("albums", _model.MainModel.CurrentAlbum.Name);
            SearchImages(null);
        }


        private void Model_OnClick(object sender, RoutedEventArgs e)
        {
            _model.MainModel.CurrentModel = ((Toolkit.Models.ModelViewModel)((Button)sender).DataContext);
            SetMode("models", _model.MainModel.CurrentModel.Name);
            SearchImages(null);
        }

        private void FilterPopup_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _model.IsFilterVisible = false;
            }
        }

        private void OpenAlbum(Album  album)
        {
            var albumModel = new AlbumModel()
            {
                Id = album.Id,
                Name = album.Name,
            };

            _model.MainModel.CurrentAlbum = albumModel;

            SetMode("albums", _model.MainModel.CurrentAlbum.Name);
            SearchImages(null);
        }

        private void RemoveFromAlbum(Album albumModel)
        {
            ServiceLocator.DataStore.RemoveImagesFromAlbum(albumModel.Id,  new [] { _model.CurrentImage.Id });
            SearchImages(null);
        }
        
        private void DropImagesOnFolder(object sender, DragEventArgs e)
        {
            var folder = (FolderViewModel)((FrameworkElement)sender).DataContext;
            _model.MainModel.MoveSelectedImagesToFolder(folder);

        }

        public void ResetLayout()
        {

            MainGrid.ColumnDefinitions[0].Width = new GridLength(5, GridUnitType.Star);
            MainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);

            NavigationThumbnailGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            NavigationThumbnailGrid.ColumnDefinitions[2].Width = new GridLength(3, GridUnitType.Star);

            PreviewGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            PreviewGrid.RowDefinitions[2].Height = new GridLength(3, GridUnitType.Star);

            _settings.MainGridWidth = "5*";
            _settings.MainGridWidth2 = "*";
            _settings.NavigationThumbnailGridWidth = "*";
            _settings.NavigationThumbnailGridWidth2 = "3*";
            _settings.PreviewGridHeight = "*";
            _settings.PreviewGridHeight2 = "3*";
        }

        private void PreviewPane_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnCurrentImageOpen?.Invoke(_model.CurrentImage);
        }
    }
}

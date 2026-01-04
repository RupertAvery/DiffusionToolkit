using Diffusion.Civitai.Models;
using Diffusion.Common;
using Diffusion.Common.Query;
using Diffusion.Database;
using Diffusion.Database.Models;
using Diffusion.IO;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Controls;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using Diffusion.Video;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFLocalizeExtension.Engine;
using XmpCore.Impl;
using Metadata = Diffusion.IO.Metadata;
using Model = Diffusion.Common.Model;
using SearchView = Diffusion.Common.SearchView;

namespace Diffusion.Toolkit.Pages
{
    public enum CursorPosition
    {
        Unspecified,
        Start,
        End
    }


    public class ReloadOptions
    {
        public bool Focus { get; set; }
        public Action? OnCompleted { get; set; }
        public CursorPosition CursorPosition { get; set; }
        public bool IsEmpty { get; set; }
    }

    public class ModeSettings
    {
        public ModeSettings()
        {
            History = new List<string?>();
        }

        public string LastQuery { get; set; }
        public List<string?> History { get; set; }
        public string Name { get; set; }
        public ViewMode ViewMode { get; set; }
        public string Key { get; set; }
    }

    public abstract class NavigationPage : Page
    {
        protected string NavigationPath;
        protected NavigationPage(string path)
        {
            NavigationPath = path;
            ServiceLocator.NavigatorService.RegisterRoute(path, this);
            ServiceLocator.NavigatorService.OnNavigate += NavigatorServiceOnOnNavigate;
        }

        private void NavigatorServiceOnOnNavigate(object? sender, NavigateEventArgs e)
        {
            Navigate(e);
        }

        protected virtual void Navigate(NavigateEventArgs navigate)
        {

        }
    }

    public partial class Search : NavigationPage
    {
        const string RootFolders = "Root Folders";

        private readonly SearchModel _model;
        private Dictionary<string, ModeSettings> _modeSettings = new Dictionary<string, ModeSettings>();

        private ModeSettings _currentModeSettings;

        private ICollection<Model>? _modelLookup;

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
        private readonly string[] _searchHints = File.ReadAllLines("hints.txt").Where(s => !string.IsNullOrEmpty(s.Trim())).ToArray();

        private void GetRandomHint()
        {
            var randomHint = _searchHints[r.Next(_searchHints.Length)];
            _model.SearchHint = $"Search for {randomHint}";
        }

        private Regex _gridLengthRegex = new Regex("Auto|(?<value>\\d+(?:\\.\\d+)?)(?<star>\\*)?");

        public GridLength GetGridLength(string? value)
        {
            if (string.IsNullOrEmpty(value)) return new GridLength(1, GridUnitType.Star);

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

        protected override void Navigate(NavigateEventArgs navigate)
        {
            base.Navigate(navigate);

            if (navigate.TargetUri.Path.ToLower() == "search")
            {
                var fragment = navigate.TargetUri.Fragment;

                if (!string.IsNullOrEmpty(fragment))
                {
                    SetView(fragment.ToLower());
                }
                else
                {
                    SetView("images");
                }

                SearchImages(null);
            }

        }

        public Search() : base("search")
        {
            InitializeComponent();

            ServiceLocator.ThumbnailNavigationService.Next += ThumbnailNavigationServiceOnNext;
            ServiceLocator.ThumbnailNavigationService.Previous += ThumbnailNavigationServiceOnPrevious;
            ServiceLocator.ThumbnailNavigationService.NextPage += ThumbnailNavigationServiceOnNextPage;
            ServiceLocator.ThumbnailNavigationService.PreviousPage += ThumbnailNavigationServiceOnPreviousPage;

            var _settings = ServiceLocator.Settings;

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

            _model = new SearchModel();
            //_model.DataStore = _dataStoreOptions;

            ServiceLocator.SearchService = new SearchService(_model.Filter, _model.SearchSettings);
            ServiceLocator.SearchService.Search += (obj, args) =>
            {
                SearchImages(null);
            };

            ServiceLocator.SearchService.Refresh += (obj, args) =>
            {
                ReloadMatches(null);
            };

            ServiceLocator.SearchService.OpenFolder += (obj, args) =>
            {
                OpenFolder(args);
            };


            ServiceLocator.SearchService.OpenPath += (obj, args) =>
            {
                OpenPath(args);
            };

            _model.Page = 0;
            _model.Pages = 0;
            _model.TotalFiles = 100;
            _model.Images = new ObservableCollection<ImageEntry>();
            _model.PropertyChanged += ModelOnPropertyChanged;
            _model.SearchCommand = new RelayCommand<object>((o) =>
            {
                _model.IsFilterVisible = false;
                _model.Filter.Clear();
                SearchImages(null);
            });

            _model.Refresh = new RelayCommand<object>((o) =>
            {
                Refresh();
            });

            _model.CurrentImage.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
            _model.CopyFiles = new RelayCommand<object>((o) => CopyFiles(ThumbnailListView.SelectedImages));

            ThumbnailListView.CopyFiles = CopyFiles;


            _model.FocusSearch = new RelayCommand<object>((o) => SearchTermTextBox.Focus());
            _model.ShowDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = true);
            _model.HideDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = false);

            _model.ShowFilter = new RelayCommand<object>((o) => ShowFilter());
            _model.ShowSearchHelp = new RelayCommand<object>((o) => OpenSearchHelp());
            _model.ShowSearchSettings = new RelayCommand<object>((o) => OpenSearchSettings());
            _model.HideFilter = new RelayCommand<object>((o) => _model.IsFilterVisible = false);
            _model.ClearSearch = new RelayCommand<object>((o) => ClearQueryFilter());

            var settings = ServiceLocator.Settings;

            _model.SearchSettings.SearchNodes = settings.SearchNodes;
            _model.SearchSettings.SearchAllProperties = settings.SearchAllProperties;
            _model.SearchSettings.SearchRawData = settings.SearchRawData;

            _model.SearchSettings.IncludeNodeProperties = string.Join("\n", settings.IncludeNodeProperties);
            _model.SearchSettings.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(SearchSettings.SearchNodes):
                        settings.SearchNodes = _model.SearchSettings.SearchNodes;
                        break;
                    case nameof(SearchSettings.SearchAllProperties):
                        settings.SearchNodes = _model.SearchSettings.SearchAllProperties;
                        break;
                    case nameof(SearchSettings.SearchRawData):
                        settings.SearchNodes = _model.SearchSettings.SearchRawData;
                        break;
                    case nameof(SearchSettings.IncludeNodeProperties):
                        settings.IncludeNodeProperties = _model.SearchSettings.GetNodePropertiesList().ToList();
                        break;
                }
            };

            _model.FilterCommand = new RelayCommand<object>((o) =>
            {
                _model.IsFilterVisible = false;
                _model.SearchText = "";
                ServiceLocator.MainModel.QueryText = "";
                SearchImages(null);
            });

            _model.ClearCommand = new RelayCommand<object>((o) =>
            {
                _model.Filter.Clear();

            });

            _model.OpenCommand = new RelayCommand<object>(async (o) =>
            {
                if (_currentModeSettings.ViewMode == ViewMode.Folder &&
                    (_model.SelectedImageEntry.EntryType is EntryType.Folder or EntryType.RootFolder))
                {
                    _model.FolderPath = _model.SelectedImageEntry.Path;

                    await ExpandToPath(_model.FolderPath);

                    SearchImages(null);
                }
                else
                {
                    OpenImage(null);
                }
            });

            _model.GoHome = new RelayCommand<object>((o) =>
            {
                if (_currentModeSettings.ViewMode == ViewMode.Folder)
                {
                    OpenFolder(FolderViewModel.Home);
                }
            });

            _model.GoUp = new RelayCommand<object>((o) =>
            {
                ServiceLocator.FolderService.NavigateToParentFolder();
            });

            _model.PageChangedCommand = new RelayCommand<PageChangedEventArgs>((o) =>
            {
                ReloadMatches(new ReloadOptions() { Focus = true, CursorPosition = o.CursorPosition, OnCompleted = o.OnCompleted });
            });

            void PopulateViews()
            {
                _modeSettings = new Dictionary<string, ModeSettings>()
                {
                    { "images", new ModeSettings() { Key = "images", Name = GetLocalizedText("Search.Diffusions"), ViewMode = ViewMode.Search } },
                    //{ "models", new ModeSettings() { Name = GetLocalizedText("Search.Models"), ViewMode = ViewMode.Model } },
                    { "folders", new ModeSettings() { Key = "folders", Name = GetLocalizedText("Search.Folders"), ViewMode = ViewMode.Folder  } },
                    // { "albums", new ModeSettings() { Name = GetLocalizedText("Search.Albums"), ViewMode = ViewMode.Album } },
                    { "favorites", new ModeSettings() { Key = "favorites", Name = GetLocalizedText("Search.Favorites"), ViewMode = ViewMode.Search } },
                    { "deleted", new ModeSettings() { Key = "deleted", Name = GetLocalizedText("Search.ForDeletion"), ViewMode = ViewMode.Search } },
                };
            }


            void PopulateSortOptions()
            {
                _model.SortOptions = new List<OptionValue>()
                {
                    new(GetLocalizedText("Search.SortBy.DateCreated"), "Date Created"),
                    new(GetLocalizedText("Search.SortBy.DateModified"), "Date Modified"),
                    new(GetLocalizedText("Search.SortBy.Rating"), "Rating"),
                    new(GetLocalizedText("Search.SortBy.LastViewed"), "Last Viewed"),
                    new(GetLocalizedText("Search.SortBy.LastUpdated"), "Last Updated"),
                    new(GetLocalizedText("Search.SortBy.AestheticScore"), "Aesthetic Score"),
                    new(GetLocalizedText("Search.SortBy.Name"), "Name"),
                    new(GetLocalizedText("Search.SortBy.Prompt"), "Prompt"),
                    new(GetLocalizedText("Search.SortBy.Random"), "Random"),
                    new(GetLocalizedText("Search.SortBy.FileSize"), "File Size"),
                };

                _model.SortOrderOptions = new List<OptionValue>()
                {
                    new(GetLocalizedText("Search.SortBy.Ascending"), "A-Z"),
                    new(GetLocalizedText("Search.SortBy.Descending"), "Z-A")
                };

            }

            LocalizeDictionary.Instance.PropertyChanged += (sender, args) =>
            {
                PopulateViews();

                var sortBy = _model.SortBy;
                var sortDirection = _model.SortDirection;
                PopulateSortOptions();
                _model.SortBy = sortBy;
                _model.SortDirection = sortDirection;

                SearchImages(null);
                SetView(_model.CurrentMode);
                _model.MainModel.Status = "";
            };

            PopulateViews();
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
                        ServiceLocator.AlbumService.ReloadContextMenus();
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
                    case nameof(NavigationSectionSettings.ShowQueries):
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
                    case nameof(NavigationSection.TagState):
                        _settings.NavigationSection.TagState = _model.NavigationSection.TagState;
                        break;
                    case nameof(NavigationSection.QueryState):
                        _settings.NavigationSection.QueryState = _model.NavigationSection.QueryState;
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
                    case nameof(NavigationSection.QueryHeight):
                        _settings.NavigationSection.QueryHeight = _model.NavigationSection.QueryHeight;
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
            _model.NavigationSection.TagState = _settings.NavigationSection.TagState;
            _model.NavigationSection.QueryHeight = _settings.NavigationSection.QueryHeight;

            _model.NavigationSection.FolderHeight = _settings.NavigationSection.FolderHeight;
            _model.NavigationSection.ModelHeight = _settings.NavigationSection.ModelHeight;
            _model.NavigationSection.AlbumHeight = _settings.NavigationSection.AlbumHeight;
            _model.NavigationSection.TagHeight = _settings.NavigationSection.TagHeight;
            _model.NavigationSection.QueryState = _settings.NavigationSection.QueryState;

            //_model.NavigationSection.ShowFolders = _settings.NavigationSection.ShowFolders;
            //_model.NavigationSection.ShowModels = _settings.NavigationSection.ShowModels;
            //_model.NavigationSection.ShowAlbums = _settings.NavigationSection.ShowAlbums;
            this.Loaded += (sender, args) =>
            {
                SetAccordionResizeableState();
            };

            SetNavigationVisible(_model.MainModel.Settings.NavigationSection.ShowSection);

            SetView("images");

            DataContext = _model;


            ThumbnailListView.OnExpandToFolder = (entry) =>
            {
                _ = ExpandToPath(Path.GetDirectoryName(entry.Path));
            };

            ServiceLocator.TaggingService.TagUpdated += (sender, arguments) =>
            {
                Update(arguments.Id);
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

        public void Refresh()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                SearchImages(null);
            }
            else
            {
                ReloadMatches(null);
            }
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


        public void ClearQueryFilter()
        {
            _model.Filter.Clear();
            _currentModeSettings.LastQuery = "";
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
            ServiceLocator.Settings.MainGridWidth = MainGrid.ColumnDefinitions[0].Width.ToString();
        }

        private void WidthChanged2(object? sender, EventArgs e)
        {
            ServiceLocator.Settings.MainGridWidth2 = MainGrid.ColumnDefinitions[2].Width.ToString();
        }

        private void NavThumbWidthChanged(object? sender, EventArgs e)
        {
            ServiceLocator.Settings.NavigationThumbnailGridWidth = NavigationThumbnailGrid.ColumnDefinitions[0].Width.ToString();
        }

        private void NavThumbWidthChanged2(object? sender, EventArgs e)
        {
            ServiceLocator.Settings.NavigationThumbnailGridWidth2 = NavigationThumbnailGrid.ColumnDefinitions[2].Width.ToString();
        }

        private void HeightChanged(object? sender, EventArgs e)
        {
            ServiceLocator.Settings.PreviewGridHeight = PreviewGrid.RowDefinitions[0].Height.ToString();
        }

        private void HeightChanged2(object? sender, EventArgs e)
        {
            ServiceLocator.Settings.PreviewGridHeight2 = PreviewGrid.RowDefinitions[2].Height.ToString();
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
        public IEnumerable<ImageEntry> Images => _model.Images;

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

        private (float ScaledSize, string FormattedSize) ToIECPrefix(long size)
        {
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

            return (fsize, ssize);
        }

        public void UpdateResults()
        {
            var text = GetLocalizedText("Search.Results");
            var selectedText = GetLocalizedText("Search.Selection");

            var (fsize, formattedSize) = ToIECPrefix(_model.Size);
            text = text.Replace("{count}", $"{_model.Count:n0}")
                .Replace("{size}", $"{formattedSize}");

            if (ServiceLocator.MainModel.SelectedImages != null && ServiceLocator.MainModel.SelectedImages.Count > 1)
            {
                text = text + " | " + selectedText.Replace("{count}", $"{ServiceLocator.MainModel.SelectedImages.Count:n0}");
            }

            _model.Results = text;
        }

        public void SearchImages(QueryOptions? queryOptions, bool focus = false)
        {
            if (!ServiceLocator.FolderService.RootFolders.Any())
            {
                _model.HasNoImagePaths = true;
                //MessageBox.Show(GetLocalizedText("Messages.Errors.NoImagePaths"), GetLocalizedText("Messages.Captions.Error"),
                //    MessageBoxButton.OK, MessageBoxImage.Warning);
                ClearResults();
                return;
            }

            _model.HasNoImagePaths = false;

            try
            {
                Dispatcher.Invoke(() =>
                {
                    _model.IsBusy = true;
                });

                //_model.Images!.Clear();
                int count = 0;
                long size = 0;


                var albums = _model.MainModel.Albums.Where(d => d.IsTicked).Select(d => d.Id).ToList();
                var models = _model.MainModel.ImageModels.Where(d => d.IsTicked).Select(d => new ModelInfo { Name = d.Name, Hash = d.Hash, HashV2 = d.Hashv2 }).ToList();
                var tags = _model.MainModel.Tags.Where(d => d.IsTicked).Select(d => d.Id).ToList();

                if (queryOptions != null)
                {
                    QueryOptions = queryOptions;

                    ServiceLocator.MainModel.SelectedAlbumsCount = queryOptions.AlbumIds.Count;

                    foreach (var album in ServiceLocator.MainModel.Albums)
                    {
                        album.IsTicked = queryOptions.AlbumIds.Contains(album.Id);
                    }



                    _model.MainModel.HideNSFW = queryOptions.HideNSFW;
                    _model.MainModel.HideDeleted = queryOptions.HideDeleted;
                    _model.MainModel.HideUnavailable = queryOptions.HideUnavailable;

                    switch (queryOptions.SearchView)
                    {
                        case SearchView.Search:
                            _currentModeSettings.ViewMode = ViewMode.Search;
                            break;
                        case SearchView.Folder:
                            _currentModeSettings.ViewMode = ViewMode.Folder;
                            break;
                        case SearchView.Favorites:
                            _currentModeSettings.ViewMode = ViewMode.Search;
                            break;
                        case SearchView.Deleted:
                            _currentModeSettings.ViewMode = ViewMode.Search;
                            break;
                    }

                    _model.SearchSettings.SearchNodes = queryOptions.SearchNodes;
                    _model.SearchSettings.SearchAllProperties = queryOptions.SearchAllProperties;
                    _model.SearchSettings.SearchRawData = queryOptions.SearchRawData;

                    if (queryOptions.ComfyQueryOptions.SearchProperties != null)
                    {
                        _model.SearchSettings.IncludeNodeProperties = string.Join("\r\n", queryOptions.ComfyQueryOptions.SearchProperties);
                    }

                    _model.Filter = queryOptions.Filter.AsModel();

                    _model.SearchText = queryOptions.Query;
                }
                else
                {
                    if (ServiceLocator.MainModel.CurrentQuery != null)
                    {
                        ServiceLocator.MainModel.CurrentQuery.IsSelected = false;
                    }

                    QueryOptions = new QueryOptions()
                    {
                        AlbumIds = albums,
                        Models = models,
                        TagIds = tags,
                        Folder = _model.FolderPath == RootFolders ? null : _model.FolderPath,
                        SearchNodes = _model.SearchSettings.SearchNodes,
                        SearchAllProperties = _model.SearchSettings.SearchAllProperties,
                        SearchRawData = _model.SearchSettings.SearchRawData,
                        HideNSFW = _model.MainModel.HideNSFW,
                        HideDeleted = _model.MainModel.HideDeleted,
                        HideUnavailable = _model.MainModel.HideUnavailable,
                        ComfyQueryOptions = new ComfyQueryOptions()
                        {
                            SearchProperties = _model.SearchSettings.GetNodePropertiesList()
                        }
                    };

                    QueryOptions.Query = _model.SearchText;
                    QueryOptions.Filter = _model.Filter.AsFilter();

                }


                ServiceLocator.MainModel.HasQuery = QueryOptions.HasQuery;
                ServiceLocator.MainModel.HasFilter = QueryOptions.HasFilter;


                if (_currentModeSettings.Key == "folders")
                {
                    QueryOptions.SearchView = SearchView.Folder;
                }
                if (_currentModeSettings.Key == "favorites")
                {
                    QueryOptions.SearchView = SearchView.Favorites;
                }
                else if (_currentModeSettings.Key == "deleted")
                {
                    QueryOptions.SearchView = SearchView.Deleted;
                }

                Task.Run(() =>
                {

                    if (!string.IsNullOrEmpty(QueryOptions.Query))
                    {
                        if (_model.SearchHistory.Count == 0 || (_model.SearchHistory.Count > 0 && _model.SearchHistory[0] != QueryOptions.Query))
                        {

                            Dispatcher.Invoke(() =>
                            {
                                if (_model.SearchHistory.Count + 1 > 25)
                                {
                                    _model.SearchHistory.RemoveAt(_model.SearchHistory.Count - 1);
                                }

                                _model.SearchHistory.Insert(0, QueryOptions.Query);
                            });

                            _currentModeSettings.History = _model.SearchHistory.ToList();
                        }

                        _currentModeSettings.LastQuery = QueryOptions.Query;
                    }

                    (count, size) = ServiceLocator.DataStore.CountAndFileSizeEx(QueryOptions);



                    Dispatcher.Invoke(() =>
                    {
                        _model.IsEmpty = count == 0;

                        if (_model.IsEmpty)
                        {
                            //_model.CurrentImage.;
                        }

                        _model.Pages = count / ServiceLocator.Settings.PageSize + (count % ServiceLocator.Settings.PageSize > 1 ? 1 : 0);


                        _model.Count = count;
                        _model.Size = size;

                        UpdateResults();


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
                                return;
                            }
                        }

                        _model.Page = 1;

                        ThumbnailListView.Model.Pages = _model.Pages;
                        ThumbnailListView.Model.Page = _model.Page;

                        ThumbnailListView.SetPagingEnabled();
                    });

                    ReloadMatches(new ReloadOptions() { Focus = focus, IsEmpty = count == 0 });
                })
                .ContinueWith(d =>
                {
                    if (d.IsFaulted)
                    {
                        ServiceLocator.MessageService.Show(d.Exception.Message, "An error occured while searching", PopupButtons.OK);
                        _model.IsBusy = false;
                    }
                });

            }
            catch (Exception e)
            {
                ServiceLocator.MessageService.ShowMedium(e.Message, GetLocalizedText("Messages.Captions.Error"),
                    PopupButtons.OK);
            }
        }


        public QueryOptions QueryOptions
        {
            get => ServiceLocator.MainModel.QueryOptions;
            set => ServiceLocator.MainModel.QueryOptions = value;
        }

        public Sorting Sorting { get; private set; }

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
                }
            }
            else if (e.PropertyName == nameof(SearchModel.SortBy))
            {
                ServiceLocator.Settings.SortBy = _model.SortBy;

                ReloadMatches(new ReloadOptions() { Focus = true });
            }
            else if (e.PropertyName == nameof(SearchModel.SortDirection))
            {
                ServiceLocator.Settings.SortDirection = _model.SortDirection;

                ReloadMatches(new ReloadOptions() { Focus = true });
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

        private CancellationTokenSource? _loadPreviewBitmapCts;

        public void LoadPreviewImage(string path, ImageEntry? image = null, bool updateViewed = true)
        {
            try
            {
                if (image is { EntryType: EntryType.Folder })
                {
                    var emptyModel = new ImageViewModel();
                    emptyModel.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
                    emptyModel.Path = path;
                    //emptyModel.IsMessageVisible = true;
                    //emptyModel.Message = GetLocalizedText("Search.LoadPreview.MediaUnavailable");

                    _model.CurrentImage = emptyModel;

                    PreviewPane.ResetZoom();
                    return;
                }

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
                imageViewModel.OpenAlbumCommand = new RelayCommand<Album>((o) =>
                {
                    var albumModel = ServiceLocator.MainModel.Albums.First(d => d.Id == o.Id);
                    OpenAlbum(albumModel);
                });
                imageViewModel.RemoveFromAlbumCommand = new RelayCommand<Album>(RemoveFromAlbum);

                if (image != null)
                {
                    imageViewModel.Id = image.Id;
                    imageViewModel.Favorite = image.Favorite;
                    imageViewModel.Date = image.CreatedDate.ToString("G", LocalizeDictionary.CurrentCulture);
                    imageViewModel.Rating = image.Rating;
                    imageViewModel.NSFW = image.NSFW;
                    imageViewModel.ForDeletion = image.ForDeletion;
                    imageViewModel.Albums = ServiceLocator.DataStore.GetImageAlbums(image.Id);
                    var albumLookup = imageViewModel.Albums.ToDictionary(x => x.Id);

                    //foreach (var album in _model.MainModel.Albums)
                    //{
                    //    album.IsTicked = albumLookup.ContainsKey(album.Id);
                    //}

                }

                Task.Run(() =>
                {
                    imageViewModel.IsLoading = true;
                    try
                    {
                        var sourceImage = GetBitmapImage(path);
                        Dispatcher.Invoke(() =>
                        {
                            _model.CurrentImage.Image = sourceImage;
                        });

                    }
                    finally
                    {
                        Dispatcher.Invoke(() =>
                        {
                            imageViewModel.IsLoading = false;
                        });
                    }

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
                    imageViewModel.Type = parameters.Type;

                    try
                    {
                        var parser = new ComfyUIParser();
                        imageViewModel.Nodes = parser.Parse(parameters.WorkflowId, parameters.Workflow);
                    }
                    catch
                    {
                    }

                    imageViewModel.ImageTags = ServiceLocator.TagService.GetImageTagViews(imageViewModel.Id);

                    var notFound = GetLocalizedText("Metadata.Modelname.NotFound");

                    if (_modelLookup != null)
                    {
                        var models = _modelLookup.Where(m =>
                            !string.IsNullOrEmpty(parameters.ModelHash) &&
                            (String.Equals(m.Hash, parameters.ModelHash, StringComparison.CurrentCultureIgnoreCase)
                             ||
                             (m.SHA256 != null && string.Equals(m.SHA256.Substring(0, parameters.ModelHash.Length), parameters.ModelHash, StringComparison.CurrentCultureIgnoreCase))
                             ||
                             parameters.Model == m.Filename
                            ));

                        if (models.Any())
                        {
                            imageViewModel.ModelName = string.Join(", ", models.Select(m => m.Filename).Distinct());
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

                if (updateViewed)
                {
                    ServiceLocator.FileService.UpdateViewed(imageViewModel.Id);
                }

                //PreviewPane.ResetZoom();

                OnCurrentImageChange?.Invoke(_model.CurrentImage);
            }
            catch (FileNotFoundException)
            {
                var notFound = GetLocalizedText("Search.LoadPreview.ImageNotFound");
                var caption = GetLocalizedText("Search.LoadPreview.ImageNotFound.Caption");

                ServiceLocator.MessageService.ShowMedium(notFound, caption, PopupButtons.OK);
            }
            catch (Exception ex)
            {
                ServiceLocator.MessageService.ShowMedium($"{ex.Message}", "An error occured", PopupButtons.OK);
            }
        }


        public static BitmapImage? GetBitmapImage(string path)
        {
            BitmapImage? bitmap = null;

            using var stream = Path.GetExtension(path) == ".mp4"
                ? FrameExtractor.ExtractFrameToPNG(path)
                : new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
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

        /// <summary>
        /// Loads current search results without updating the count
        /// </summary>
        /// <param name="options"></param>
        public void ReloadMatches(ReloadOptions? options)
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    _model.IsBusy = true;
                });

                try
                {
                    LoadImageEntries();
                    ThumbnailListView.ResetView(options);
                    Dispatcher.Invoke(() =>
                    {
                        options?.OnCompleted?.Invoke();

                        if (options is not null && _model.Images is { Count: > 0 })
                        {
                            var empty = _model.Images.FirstOrDefault(d => d.IsEmpty);

                            var lastIndex = _model.Images.Count - 1;

                            if (empty != null)
                            {
                                lastIndex = _model.Images.IndexOf(empty) - 1;
                            }

                            ThumbnailListView.SelectedIndex = options.CursorPosition switch
                            {
                                CursorPosition.Start => 0,
                                CursorPosition.End => lastIndex,
                                _ => 0
                            };

                            _model.SelectedImageEntry = options.CursorPosition switch
                            {
                                CursorPosition.Start => _model.Images[0],
                                CursorPosition.End => _model.Images[lastIndex],
                                _ => _model.Images[0]
                            };


                            ThumbnailListView.FocusCurrentItem();

                            if (options.CursorPosition == CursorPosition.Start)
                            {
                                ThumbnailListView.ScrollToTop();
                            }
                            else if (options.CursorPosition == CursorPosition.End)
                            {
                                ThumbnailListView.ScrollToBottom();
                            }

                            //ServiceLocator.MainModel.SelectedImages.Clear();
                            //ServiceLocator.MainModel.SelectedImages.Add(_model.SelectedImageEntry);
                        }
                        else
                        {
                            _model.SelectedImageEntry = null;
                        }

                    });
                }
                finally
                {
                    _model.IsBusy = false;
                }

            });
        }

        private List<ImageEntry> GetFolders(long rId)
        {

            var foldersEntries = new List<ImageEntry>();

            // TODO: Better way to determine if root folders view

            if (QueryOptions.Folder == null)
            {
                var folders = ServiceLocator.FolderService.RootFolders;

                foreach (var folder in folders)
                {
                    var imageEntry = new ImageEntry(rId)
                    {
                        Id = folder.Id,
                        Path = folder.Path,
                        FileName = Path.GetFileName(folder.Path),
                        Name = folder.Path,
                        EntryType = EntryType.RootFolder,
                        IsRecursive = folder.Recursive,
                        IsWatched = folder.Watched,
                        Unavailable = folder.Unavailable,
                        IsEmpty = false,
                    };

                    foldersEntries.Add(imageEntry);
                }
            }
            else
            {
                var folders = Directory.GetDirectories(QueryOptions.Folder);

                foreach (var folder in folders.OrderBy(d => d))
                {
                    var imageEntry = new ImageEntry(rId)
                    {
                        Id = 0,
                        Path = folder,
                        FileName = Path.GetFileName(folder),
                        Name = Path.GetFileName(folder),
                        EntryType = EntryType.Folder
                    };

                    foldersEntries.Add(imageEntry);
                }
            }

            return foldersEntries;
        }

        private List<ImageEntry> GetSearchResults(long rId)
        {

            var paging = new Paging()
            {
                PageSize = ServiceLocator.Settings.PageSize,
                Offset = ServiceLocator.Settings.PageSize * (_model.Page - 1)
            };

            Sorting = new Sorting(_model.SortBy, _model.SortDirection);

            IEnumerable<ImageView> matches = Enumerable.Empty<ImageView>();

            matches = Time(() => ServiceLocator.DataStore
                .SearchEx(QueryOptions,
                    Sorting,
                    paging
                ));


            var count = 0;
            var images = new List<ImageEntry>();

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
                    Name = Path.GetFileName(file.Path),
                    NSFW = file.NSFW,
                    EntryType = EntryType.File,
                    AlbumCount = file.AlbumCount,
                    Dispatcher = Dispatcher,
                    HasError = file.HasError
                };

                images.Add(imageEntry);

                count++;
            }

            return images;

        }

        private void LoadImageEntries()
        {
            ServiceLocator.ThumbnailService.StopCurrentBatch();

            var rId = ServiceLocator.ThumbnailService.StartBatch();

            List<ImageEntry> folderEntries = new List<ImageEntry>();
            List<ImageEntry> separatorEntries = new List<ImageEntry>() { new ImageEntry(0) { EntryType = EntryType.Separator } };
            List<ImageEntry> imageEntries = new List<ImageEntry>();


            if (QueryOptions.SearchView == SearchView.Folder && _model.Page == 1)
            {
                if (QueryOptions.Folder != null && !Directory.Exists(QueryOptions.Folder))
                {
                    _ = ServiceLocator.MessageService.ShowMedium(GetLocalizedText("Search.Folders.Unavailable"),
                        GetLocalizedText("Search.Folders.Unavailable.Title"), PopupButtons.OK);
                    ServiceLocator.MainModel.CurrentFolder.IsUnavailable = true;
                    return;
                }

                folderEntries = GetFolders(rId);
            }

            imageEntries = GetSearchResults(rId);

            var totalEntries = folderEntries.Count + 1 + imageEntries.Count;

            var sw = new Stopwatch();
            sw.Start();

            // Instead of creating a new set of ImageEntries every time the search results are updated,
            // create a base set (Max # of results per page) and update the values  with the new results.
            // This prevents WPF from destroying and recreating the controls which is slow.

            // However, we still need to prevent WPF from showing the now empty thumbnails and prevent the user
            // from navigating to them, so we need to implement additional logic around to account for the empty
            // thumbnails. The IsEmpty property is used to mark a thumbnail as unused and hidden.

            Dispatcher.Invoke(() =>
            {
                if (_model.Images == null)
                {
                    _model.Images = new ObservableCollection<ImageEntry>(folderEntries.Concat(separatorEntries).Concat(imageEntries));
                }
                else if (_model.Images.Count != totalEntries)
                {
                    // In case there are more entries than currently available, extend the collection
                    if (totalEntries > _model.Images.Count)
                    {
                        var difference = totalEntries - _model.Images.Count;

                        for (var i = 0; i < difference; i++)
                        {
                            _model.Images.Add(new ImageEntry(0));
                        }
                    }
                    else if (totalEntries < _model.Images.Count)
                    {
                        // presumably some logic here to return the size back to # of results per page,
                        // But keeping the items at max doesn't seem to affect performance
                    }
                }

                // Overwrite existing ImageEntries with the latest search results

                // Render the folders first, if there are any
                for (var i = 0; i < folderEntries.Count; i++)
                {
                    var dest = _model.Images[i];
                    var src = folderEntries[i];

                    dest.PropertyChanged -= ImageEntry_OnPropertyChanged;
                    dest.BatchId = src.BatchId;
                    dest.Id = src.Id;
                    dest.EntryType = src.EntryType;
                    dest.Name = src.Name;
                    dest.Favorite = src.Favorite;
                    dest.ForDeletion = src.ForDeletion;
                    dest.Rating = src.Rating;
                    dest.Score = src.Score;
                    dest.NSFW = src.NSFW;
                    dest.FileName = src.FileName;
                    dest.Path = src.Path;
                    dest.CreatedDate = src.CreatedDate;
                    dest.AlbumCount = src.AlbumCount;
                    dest.Albums = src.Albums;
                    dest.HasError = src.HasError;
                    dest.Unavailable = src.Unavailable;
                    dest.LoadState = LoadState.Unloaded;
                    dest.Dispatcher = Dispatcher;
                    dest.Thumbnail = null;
                    dest.IsEmpty = false;
                    //dest.Count = src.Count;
                    //dest.Size = src.Size;

                    if (dest.EntryType == EntryType.RootFolder)
                    {
                        dest.IsRecursive = src.IsRecursive;
                        dest.IsWatched = src.IsWatched;
                        dest.PropertyChanged += ImageEntry_OnPropertyChanged;

                        Task.Run(() =>
                        {
                            var (count, size) = ServiceLocator.DataStore.FolderCountAndSize(dest.Id);
                            Dispatcher.Invoke(() =>
                            {
                                dest.Count = count;
                                dest.Size = size;
                            });
                        });
                    }
                    else
                    {
                        dest.IsRecursive = false;
                        dest.IsWatched = false;
                    }

                }


                var offset = folderEntries.Count;

                // Add a Separator if folder view

                if (folderEntries.Count > 0 && imageEntries.Count > 0)
                {
                    var dest = _model.Images[offset];

                    dest.BatchId = 0;
                    dest.Id = 0;
                    dest.EntryType = EntryType.Separator;
                    dest.Name = "";
                    dest.Favorite = false;
                    dest.ForDeletion = false;
                    dest.Rating = null;
                    dest.Score = "";
                    dest.NSFW = false;
                    dest.FileName = "";
                    dest.Path = "";
                    dest.CreatedDate = DateTime.MinValue;
                    dest.AlbumCount = 0;
                    dest.Albums = Enumerable.Empty<string>();
                    dest.HasError = false;
                    dest.Unavailable = false;
                    dest.LoadState = LoadState.Loaded;
                    dest.Dispatcher = Dispatcher;
                    dest.Thumbnail = null;
                    dest.IsEmpty = false;
                    dest.IsRecursive = false;
                    dest.IsWatched = false;
                    dest.Count = 0;
                    dest.Size = 0;
                    dest.PropertyChanged -= ImageEntry_OnPropertyChanged;

                    offset += 1;
                }

                // Render images
                for (var i = 0; i < imageEntries.Count; i++)
                {
                    var dest = _model.Images[offset + i];
                    var src = imageEntries[i];

                    // TODO: What happens when we switch from a folder to an image?
                    // 

                    dest.PropertyChanged -= ImageEntry_OnPropertyChanged;

                    dest.BatchId = src.BatchId;
                    dest.Id = src.Id;
                    dest.EntryType = src.EntryType;
                    dest.Name = src.Name;
                    dest.Favorite = src.Favorite;
                    dest.ForDeletion = src.ForDeletion;
                    dest.Rating = src.Rating;
                    dest.Score = src.Score;
                    dest.NSFW = src.NSFW;
                    dest.FileName = src.FileName;
                    dest.Path = src.Path;
                    dest.CreatedDate = src.CreatedDate;
                    dest.AlbumCount = src.AlbumCount;
                    dest.Albums = src.Albums;
                    dest.HasError = src.HasError;
                    dest.Unavailable = !File.Exists(src.Path);
                    dest.LoadState = LoadState.Unloaded;
                    dest.Dispatcher = Dispatcher;
                    dest.Thumbnail = null;
                    dest.IsEmpty = false;


                    dest.IsRecursive = false;
                    dest.IsWatched = false;
                    dest.Count = 0;
                    dest.Size = src.Size;
                }

                offset += imageEntries.Count;

                // Clear our any remaining entries (IsEmpty = true)

                for (var i = offset; i < _model.Images.Count; i++)
                {
                    _model.Images[i].PropertyChanged -= ImageEntry_OnPropertyChanged;
                    _model.Images[i].Clear();
                }

                ThumbnailListView.ReloadThumbnailsView();

                ThumbnailListView.ClearSelection();

                if (_model.SelectedImageEntry != null)
                {
                    // load image, but don't count as viewed
                    LoadPreviewImage(_model.SelectedImageEntry.Path, _model.SelectedImageEntry, false);
                }
            });

            sw.Stop();

            Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds:#,###,##0}ms");
        }

        private void ImageEntry_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var imageEntry = ((ImageEntry)sender);

            switch (e.PropertyName)
            {
                case nameof(ImageEntry.IsRecursive):
                case nameof(ImageEntry.IsWatched):
                    ServiceLocator.FolderService.UpdateRootFolder(imageEntry.Id, imageEntry.Path, imageEntry.IsWatched, imageEntry.IsRecursive, e.PropertyName);
                    break;
            }
        }

        public void RefreshThumbnails()
        {
            if (_model.Images != null)
            {
                foreach (var image in _model.Images)
                {
                    ServiceLocator.ThumbnailService.QueueImage(image);
                }
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

        public void SetView(string mode, string? context = null)
        {
            _currentModeSettings = GetModeSettings(mode);
            _model.IsFilterVisible = false;


            if (mode != "folders")
            {
                ServiceLocator.FolderService.ClearSelection();
            }
            else
            {
                if (ServiceLocator.MainModel.CurrentFolder != null)
                {
                    ServiceLocator.MainModel.CurrentFolder.IsSelected = true;
                }
                else
                {
                    ServiceLocator.MainModel.CurrentFolder = FolderViewModel.Home;
                }
            }

            _model.CurrentMode = mode;
            _model.CurrentViewMode = _currentModeSettings.ViewMode;
            //_model.SearchText = _currentModeSettings.LastQuery;

            ServiceLocator.SearchService.CurrentViewMode = _currentModeSettings.ViewMode;

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
                //NavigationThumbnailGrid.ColumnDefinitions[0].Width = GetGridLength(ServiceLocator.Settings.NavigationThumbnailGridWidth);
                //NavigationThumbnailGrid.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Auto);

                NavigationThumbnailGrid.ColumnDefinitions[0].Width = GetGridLength(ServiceLocator.Settings.NavigationThumbnailGridWidth);
                NavigationThumbnailGrid.ColumnDefinitions[2].Width = GetGridLength(ServiceLocator.Settings.NavigationThumbnailGridWidth2);

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
                MainGrid.ColumnDefinitions[0].Width = GetGridLength(ServiceLocator.Settings.MainGridWidth);
                MainGrid.ColumnDefinitions[2].Width = GetGridLength(ServiceLocator.Settings.MainGridWidth2);

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
            var imageData = ServiceLocator.DataStore.GetImage(id);
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
                string?[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                try
                {
                    var file = files[0];
                    if (file != null)
                    {
                        if ((File.GetAttributes(file) & FileAttributes.Directory) != 0)
                        {
                            return;
                        }
                        LoadPreviewImage(file);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Log(exception);
                    MessageBox.Show(exception.Message, "Error loading image", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        public Filter Filter => _model.Filter.AsFilter();

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


        public void SetPageSize(int pageSize)
        {
            ThumbnailListView.PageSize = pageSize;
        }

        private void DropImagesOnAlbum(object sender, DragEventArgs e)
        {
            var album = (AlbumModel)((FrameworkElement)sender).DataContext;
            _model.MainModel.AddSelectedImagesToAlbum(album);
        }

        private void OpenAlbum(AlbumModel albumModel)
        {
            ServiceLocator.MainModel.CurrentAlbum = albumModel;

            foreach (var album in ServiceLocator.MainModel.Albums)
            {
                album.IsTicked = false;
            }

            albumModel.IsTicked = true;

            var selectedAlbums = ServiceLocator.MainModel.Albums.Where(d => d.IsTicked).ToList();
            ServiceLocator.MainModel.SelectedAlbumsCount = selectedAlbums.Count;
            ServiceLocator.MainModel.HasSelectedAlbums = selectedAlbums.Any();

            SearchImages(null);
        }

        private void RemoveFromAlbum(Album albumModel)
        {
            ServiceLocator.DataStore.RemoveImagesFromAlbum(albumModel.Id, new[] { _model.CurrentImage.Id });
            UpdateImagesInPlace();
        }

        public void ResetLayout()
        {

            MainGrid.ColumnDefinitions[0].Width = new GridLength(5, GridUnitType.Star);
            MainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);

            NavigationThumbnailGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            NavigationThumbnailGrid.ColumnDefinitions[2].Width = new GridLength(3, GridUnitType.Star);

            PreviewGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            PreviewGrid.RowDefinitions[2].Height = new GridLength(3, GridUnitType.Star);

            ServiceLocator.Settings.MainGridWidth = "5*";
            ServiceLocator.Settings.MainGridWidth2 = "*";
            ServiceLocator.Settings.NavigationThumbnailGridWidth = "*";
            ServiceLocator.Settings.NavigationThumbnailGridWidth2 = "3*";
            ServiceLocator.Settings.PreviewGridHeight = "*";
            ServiceLocator.Settings.PreviewGridHeight2 = "3*";
        }

        public void UpdateImagesInPlace()
        {
            if (_model.Images != null)
            {
                var ids = _model.Images.Select(d => d.Id).ToList();

                var updatedImages = ServiceLocator.DataStore.GetImagesView(ids);

                var imageLookup = _model.Images.Where(d => d is { EntryType: EntryType.File, IsEmpty: false }).ToDictionary(d => d.Id);

                foreach (var image in updatedImages)
                {
                    if (imageLookup.TryGetValue(image.Id, out var img))
                    {
                        img.AlbumCount = image.AlbumCount;
                    }
                }
            }

            if (_model.CurrentImage != null)
            {
                _model.CurrentImage.Albums = ServiceLocator.DataStore.GetImageAlbums(_model.CurrentImage.Id);
            }
        }

        public void SetQuery(string queryInputText)
        {
            _model.SearchText = queryInputText;
        }
    }

}

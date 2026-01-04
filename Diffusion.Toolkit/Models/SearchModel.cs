using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Diffusion.Common;
using Diffusion.Common.Query;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Controls;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Models;

public class SearchModel : BaseNotify
{
    private readonly MainModel _mainModel;
    private ObservableCollection<ImageEntry>? _images;

    //public object _rowLock = new object();

    private string _searchText;

    private bool _isEmpty;
    private int _pages;
    private string _searchHint;
    private ImageViewModel? _currentImage;
    private float _imageOpacity;
    private ObservableCollection<string> _searchHistory;

    private bool _isFilterVisible;
    private FilterControlModel _filter;
    private string _sortBy;
    private string _sortDirection;
    private ViewMode _currentViewMode;
    private ObservableCollection<Album> _albums;

    public SearchModel()
    {
        _images = new ObservableCollection<ImageEntry>();
        _searchHistory = new ObservableCollection<string>();
        _currentImage = new ImageViewModel();
        _filter = new FilterControlModel();
        _imageOpacity = 1;
        _isEmpty = true;
        //_resultStatus = "Type anything to begin";
        _searchHint = "Search for the answer to the the question of life, the universe, and everything";
        _sortBy = "Date Created";
        _sortDirection = "Z-A";
        _isFilterVisible = false;
        _searchText = "";
        MetadataSection = new MetadataSection();
        NavigationSection = new NavigationSection();
        SearchSettings = new SearchSettings();
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentImage))
        {
            ServiceLocator.MainModel.CurrentImage = CurrentImage;
        }
        if (e.PropertyName == nameof(SelectedImageEntry))
        {
            ServiceLocator.MainModel.SelectedImageEntry = SelectedImageEntry;
        }
    }

    //public SearchModel(MainModel mainModel)
    //{
    //    _mainModel = mainModel;
    //    _images = new ObservableCollection<ImageEntry>();
    //    _searchHistory = new ObservableCollection<string>();
    //    _currentImage = new ImageViewModel();
    //    _filter = new FilterControlModel();
    //    _imageOpacity = 1;
    //    _isEmpty = true;
    //    //_resultStatus = "Type anything to begin";
    //    _searchHint = "Search for the answer to the the question of life, the universe, and everything";
    //    _sortBy = "Date Created";
    //    _sortDirection = "Z-A";
    //    _isFilterVisible = false;
    //    MetadataSection = new MetadataSection();
    //    NavigationSection = new NavigationSection();
    //    SearchSettings = new SearchSettings();
    //}

    public MainModel MainModel => ServiceLocator.MainModel;

    public ObservableCollection<ImageEntry>? Images
    {
        get => _images;
        set => SetField(ref _images, value);
    }

    public ImageViewModel? CurrentImage
    {
        get => _currentImage;
        set => SetField(ref _currentImage, value);
    }


    public ImageEntry? SelectedImageEntry
    {
        get;
        set => SetField(ref field, value);
    }


    public int CurrentPosition
    {
        get;
        set => SetField(ref field, value);
    }

    public int TotalFiles
    {
        get;
        set => SetField(ref field, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public ObservableCollection<string> SearchHistory
    {
        get => _searchHistory;
        set
        {
            SetField(ref _searchHistory, value);
            OnPropertyChanged("ReverseSearchHistory");
        }
    }

    public ICommand SearchCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public int Page
    {
        get;
        set
        {
            if (value > _pages)
            {
                value = _pages;
            }

            if (_pages == 0)
            {
                value = 0;
            }
            else if (value < 1)
            {
                value = 1;
            }

            SetField(ref field, value);
        }
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetField(ref _isEmpty, value);
    }

    public int Pages
    {
        get => _pages;
        set => SetField(ref _pages, value);
    }

    public string Results
    {
        get;
        set => SetField(ref field, value);
    }

    public string ResultStatus
    {
        get;
        set => SetField(ref field, value);
    }

    public string SearchHint
    {
        get => _searchHint;
        set => SetField(ref _searchHint, value);
    }

    public float ImageOpacity
    {
        get => _imageOpacity;
        set => SetField(ref _imageOpacity, value);
    }

    public bool HideIcons
    {
        get;
        set => SetField(ref field, value);
    }

    [field: AllowNull, MaybeNull]
    public ICommand Refresh
    {
        get;
        set => SetField(ref field, value);
    }

    [field: AllowNull, MaybeNull]
    public ICommand FocusSearch
    {
        get;
        set => SetField(ref field, value);
    }

    [field: AllowNull, MaybeNull]
    public string ModeName
    {
        get;
        set => SetField(ref field, value);
    }

    [field: AllowNull, MaybeNull]
    public ICommand ShowDropDown
    {
        get;
        set => SetField(ref field, value);
    }

    [field: AllowNull, MaybeNull]
    public ICommand HideDropDown
    {
        get;
        set => SetField(ref field, value);
    }

    [field: AllowNull, MaybeNull]
    public ICommand CopyFiles
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ShowSearchHelp
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ShowSearchSettings
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ShowFilter
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand HideFilter
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ClearSearch
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsFilterVisible
    {
        get => _isFilterVisible;
        set => SetField(ref _isFilterVisible, value);
    }

    public FilterControlModel Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public ICommand FilterCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ClearCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public IEnumerable<OptionValue> SortOptions
    {
        get;
        set => SetField(ref field, value);
    }

    public string SortBy
    {
        get => _sortBy;
        set => SetField(ref _sortBy, value);
    }

    public IEnumerable<OptionValue> SortOrderOptions
    {
        get;
        set => SetField(ref field, value);
    }

    public string SortDirection
    {
        get => _sortDirection;
        set => SetField(ref _sortDirection, value);
    }

    public ICommand OpenCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand GoHome
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand GoUp
    {
        get;
        set => SetField(ref field, value);
    }

    public ViewMode CurrentViewMode
    {
        get => _currentViewMode;
        set
        {
            SetField(ref _currentViewMode, value);
            OnPropertyChanged(nameof(IsFolderView));
        }
    }

    public bool IsFolderView
    {
        get => _currentViewMode == ViewMode.Folder;
    }


    // TODO: merge this into above
    public string FolderPath
    {
        get;
        set => SetField(ref field, value);
    }

    public string Album
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand PageChangedCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public NavigationSection NavigationSection
    {
        get;
        set => SetField(ref field, value);
    }

    public MetadataSection MetadataSection
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsBusy
    {
        get;
        set => SetField(ref field, value);
    }

    public string CurrentMode
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSearchSettingsVisible
    {
        get;
        set => SetField(ref field, value);
    }

    public SearchSettings SearchSettings { get; set; }

    public bool IsSearchHelpVisible
    {
        get;
        set => SetField(ref field, value);
    }

    public string SearchHelpMarkdown
    {
        get;
        set => SetField(ref field, value);
    }

    public Style SearchHelpStyle
    {
        get;
        set => SetField(ref field, value);
    }

    public int Count { get; set; }
    public long Size { get; set; }

    public bool HasNoImagePaths
    {
        get;
        set => SetField(ref field, value);
    }

    public TagsMode TagsMode
    {
        get;
        set => SetField(ref field, value);
    }
}
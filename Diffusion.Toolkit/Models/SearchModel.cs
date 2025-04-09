using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Diffusion.Common;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Controls;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Models;

public class SearchModel : BaseNotify
{
    private readonly MainModel _mainModel;
    private ObservableCollection<ImageEntry>? _images;
    private ImageEntry? _selectedImage;
    //public object _rowLock = new object();
    private int _totalFiles;
    private int _currentPosition;

    private ICommand _searchCommand;
    private string _searchText;

    private int _page;
    private bool _isEmpty;
    private int _pages;
    private string _results;
    private string _resultStatus;
    private string _searchHint;
    private ImageViewModel? _currentImage;
    private float _imageOpacity;
    private bool _hideIcons;
    private ObservableCollection<string?> _searchHistory;

    private ICommand? _refresh;
    private ICommand? _focusSearch;
    private string? _modeName;
    private ICommand? _showDropDown;
    private ICommand? _hideDropDown;
    private ICommand? _copyFiles;
    private ICommand _showFilter;
    private ICommand _hideFilter;
    private ICommand _clearSearch;
    private bool _isFilterVisible;
    private FilterControlModel _filter;
    private ICommand _filterCommand;
    private ICommand _clearCommand;
    private string _sortBy;
    private string _sortDirection;
    private ICommand _openCommand;
    private ICommand _goHome;
    private ViewMode _currentViewMode;
    private string _folderPath;
    private ICommand _goUp;
    private string _album;
    private ObservableCollection<Album> _albums;
    private ICommand _pageChangedCommand;
    private IEnumerable<OptionValue> _sortOptions;
    private IEnumerable<OptionValue> _sortOrderOptions;

    private NavigationSection _navigationSection;
    private MetadataSection _metadataSection;
    private bool _isBusy;
    private ICommand _showSearchSettings;
    private bool _isSearchSettingsVisible;

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
        get => _selectedImage;
        set => SetField(ref _selectedImage, value);
    }


    public int CurrentPosition
    {
        get => _currentPosition;
        set => SetField(ref _currentPosition, value);
    }

    public int TotalFiles
    {
        get => _totalFiles;
        set => SetField(ref _totalFiles, value);
    }

    public string? SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public ObservableCollection<string?> SearchHistory
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
        get => _searchCommand;
        set => SetField(ref _searchCommand, value);
    }

    public int Page
    {
        get => _page;
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

            SetField(ref _page, value);
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
        get => _results;
        set => SetField(ref _results, value);
    }

    public string ResultStatus
    {
        get => _resultStatus;
        set => SetField(ref _resultStatus, value);
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
        get => _hideIcons;
        set => SetField(ref _hideIcons, value);
    }

    public ICommand Refresh
    {
        get => _refresh;
        set => SetField(ref _refresh, value);
    }

    public ICommand FocusSearch
    {
        get => _focusSearch;
        set => SetField(ref _focusSearch, value);
    }

    public string ModeName
    {
        get => _modeName;
        set => SetField(ref _modeName, value);
    }

    public ICommand ShowDropDown
    {
        get => _showDropDown;
        set => SetField(ref _showDropDown, value);
    }

    public ICommand HideDropDown
    {
        get => _hideDropDown;
        set => SetField(ref _hideDropDown, value);
    }

    public ICommand CopyFiles
    {
        get => _copyFiles;
        set => SetField(ref _copyFiles, value);
    }

    public ICommand ShowSearchSettings
    {
        get => _showSearchSettings;
        set => SetField(ref _showSearchSettings, value);
    }

    public ICommand ShowFilter
    {
        get => _showFilter;
        set => SetField(ref _showFilter, value);
    }

    public ICommand HideFilter
    {
        get => _hideFilter;
        set => SetField(ref _hideFilter, value);
    }

    public ICommand ClearSearch
    {
        get => _clearSearch;
        set => SetField(ref _clearSearch, value);
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
        get => _filterCommand;
        set => SetField(ref _filterCommand, value);
    }

    public ICommand ClearCommand
    {
        get => _clearCommand;
        set => SetField(ref _clearCommand, value);
    }

    public IEnumerable<OptionValue> SortOptions
    {
        get => _sortOptions;
        set => SetField(ref _sortOptions, value);
    }

    public string SortBy
    {
        get => _sortBy;
        set => SetField(ref _sortBy, value);
    }

    public IEnumerable<OptionValue> SortOrderOptions
    {
        get => _sortOrderOptions;
        set => SetField(ref _sortOrderOptions, value);
    }

    public string SortDirection
    {
        get => _sortDirection;
        set => SetField(ref _sortDirection, value);
    }

    public ICommand OpenCommand
    {
        get => _openCommand;
        set => SetField(ref _openCommand, value);
    }

    public ICommand GoHome
    {
        get => _goHome;
        set => SetField(ref _goHome, value);
    }

    public ICommand GoUp
    {
        get => _goUp;
        set => SetField(ref _goUp, value);
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
        get => _folderPath;
        set => SetField(ref _folderPath, value);
    }

    public string Album
    {
        get => _album;
        set => SetField(ref _album, value);
    }

    public ICommand PageChangedCommand
    {
        get => _pageChangedCommand;
        set => SetField(ref _pageChangedCommand, value);
    }

    public NavigationSection NavigationSection
    {
        get => _navigationSection;
        set => SetField(ref _navigationSection, value);
    }

    public MetadataSection MetadataSection
    {
        get => _metadataSection;
        set => SetField(ref _metadataSection, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }

    public string CurrentMode { get; set; }

    public bool IsSearchSettingsVisible
    {
        get => _isSearchSettingsVisible;
        set => SetField(ref _isSearchSettingsVisible, value);
    }

    public SearchSettings SearchSettings { get; set; }
}
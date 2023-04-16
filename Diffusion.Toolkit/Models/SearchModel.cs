using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Controls;
using Diffusion.Toolkit.Converters;

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

    private int _imageCount;
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
    private bool _nsfwBlur;
    private ICommand _showFilter;
    private bool _isFilterVisible;
    private SearchControlModel _filter;
    private ICommand _doFilter;
    private ICommand _clearFilter;
    private string _sortBy;
    private string _sortDirection;
    private ICommand _openCommand;
    private ICommand _goHome;
    private ViewMode _currentViewMode;
    private string _folderPath;
    private ICommand _goUp;
    private ICommand _addAlbumCommand;
    private ICommand _addToAlbumCommand;
    private string _album;
    private ICommand _removeFromAlbumCommand;
    private ICommand _removeAlbumCommand;
    private ICommand _renameAlbumCommand;

    public SearchModel()
    {
        _mainModel = new MainModel();
        _images = new ObservableCollection<ImageEntry>();
        _searchHistory = new ObservableCollection<string>();
        _currentImage = new ImageViewModel();
        _filter = new SearchControlModel();
        _imageOpacity = 1;
        _isEmpty = true;
        _resultStatus = "Type anything to begin";
        _searchHint = "Search for the answer to the the question of life, the universe, and everything";
        _sortBy = "Date Created";
        _sortDirection = "Descending";
        _isFilterVisible = false;
    }

    public SearchModel(MainModel mainModel)
    {
        _mainModel = mainModel;
        _images = new ObservableCollection<ImageEntry>();
        _searchHistory = new ObservableCollection<string>();
        _currentImage = new ImageViewModel();
        _filter = new SearchControlModel();
        _imageOpacity = 1;
        _isEmpty = true;
        _resultStatus = "Type anything to begin";
        _searchHint = "Search for the answer to the the question of life, the universe, and everything";
        _sortBy = "Date Created";
        _sortDirection = "Descending";
        _isFilterVisible = false;
    }

    public MainModel MainModel => _mainModel;

    public DataStore DataStore
    {
        get;
        set;
    }


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

    public ICommand ShowFilter
    {
        get => _showFilter;
        set => SetField(ref _showFilter, value);
    }

    public bool IsFilterVisible
    {
        get => _isFilterVisible;
        set => SetField(ref _isFilterVisible, value);
    }

    public SearchControlModel Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public ICommand DoFilter
    {
        get => _doFilter;
        set => SetField(ref _doFilter, value);
    }

    public ICommand ClearFilter
    {
        get => _clearFilter;
        set => SetField(ref _clearFilter, value);
    }

    public string SortBy
    {
        get => _sortBy;
        set => SetField(ref _sortBy, value);
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

    public ICommand AddAlbumCommand
    {
        get => _addAlbumCommand;
        set => SetField(ref _addAlbumCommand, value);
    }

    public ICommand AddToAlbumCommand
    {
        get => _addToAlbumCommand;
        set => SetField(ref _addToAlbumCommand, value);
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
        set => SetField(ref _currentViewMode, value);
    }

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

    public ICommand RemoveFromAlbumCommand
    {
        get => _removeFromAlbumCommand;
        set => SetField(ref _removeFromAlbumCommand, value);
    }

    public ICommand RenameAlbumCommand
    {
        get => _renameAlbumCommand;
        set => SetField(ref _renameAlbumCommand, value);
    }

    public ICommand RemoveAlbumCommand
    {
        get => _removeAlbumCommand;
        set => SetField(ref _removeAlbumCommand, value);
    }
}

public static class SearchControlModelExtensions
{
    public static Filter AsFilter(this SearchControlModel model)
    {
        var filter = new Filter();

        filter.UsePrompt = model.UsePrompt;
        filter.Prompt = model.Prompt;
        filter.UseNegativePrompt = model.UseNegativePrompt;
        filter.NegativePrompt = model.NegativePrompt;
        filter.UseSteps = model.UseSteps;
        filter.Steps = model.Steps;
        filter.UseSampler = model.UseSampler;
        filter.Sampler = model.Sampler;
        filter.UseSeed = model.UseSeed;
        filter.SeedStart = model.SeedStart;
        filter.SeedEnd = model.SeedEnd;
        filter.UseCFGScale = model.UseCFGScale;
        filter.CFGScale = model.CFGScale;
        filter.UseSize = model.UseSize;
        filter.Width = model.Width;
        filter.Height = model.Height;
        filter.UseModelHash = model.UseModelHash;
        filter.ModelHash = model.ModelHash;
        filter.UseModelName = model.UseModelName;
        filter.ModelName = model.ModelName;

        filter.UseFavorite = model.UseFavorite;
        filter.Favorite = model.Favorite;
        filter.UseRating = model.UseRating;
        filter.RatingOp = model.RatingOp;
        filter.Rating = model.Rating;
        filter.Unrated = model.Unrated;
        filter.UseNSFW = model.UseNSFW;
        filter.NSFW = model.NSFW;
        filter.UseForDeletion = model.UseForDeletion;
        filter.ForDeletion = model.ForDeletion;

        filter.UseBatchSize = model.UseBatchSize;
        filter.BatchSize = model.BatchSize;

        filter.UseBatchPos = model.UseBatchPos;
        filter.BatchPos = model.BatchPos;

        filter.UseAestheticScore = model.UseAestheticScore;
        filter.AestheticScoreOp = model.AestheticScoreOp;
        filter.AestheticScore = model.AestheticScore;

        filter.UsePath = model.UsePath;
        filter.Path = model.Path;

        filter.UseCreationDate = model.UseCreationDate;
        filter.Start = model.Start;
        filter.End = model.End;

        filter.UseHyperNet = model.UseHyperNet;
        filter.HyperNet = model.HyperNet;

        filter.UseHyperNetStr = model.UseHyperNetStr;
        filter.HyperNetStrOp = model.HyperNetStrOp;
        filter.HyperNetStr = model.HyperNetStr;

        filter.UseNoMetadata = model.UseNoMetadata;
        filter.NoMetadata = model.NoMetadata;

        return filter;
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Diffusion.Database;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Controls;
using Filter = Diffusion.Database.Filter;

namespace Diffusion.Toolkit.Models;

public class OptionValue
{
    public string Name { get; }
    public string Value { get; }

    public OptionValue(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

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
    private ICommand _addAlbumCommand;
    private ICommand _addToAlbumCommand;
    private string _album;
    private ICommand _removeFromAlbumCommand;
    private ICommand _removeAlbumCommand;
    private ICommand _renameAlbumCommand;
    private ObservableCollection<Album> _albums;
    private ICommand _pageChangedCommand;
    private IEnumerable<OptionValue> _sortOptions;
    private IEnumerable<OptionValue> _sortOrderOptions;

    private NavigationSection _navigationSection;
    private MetadataSection _metadataSection;
    private bool _isBusy;

    public SearchModel()
    {
        _mainModel = new MainModel();
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
    }

    public SearchModel(MainModel mainModel)
    {
        _mainModel = mainModel;
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
    }

    public MainModel MainModel => _mainModel;

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
        set => SetField(ref _currentViewMode, value);
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
}

public class NavigationSection : BaseNotify
{
    private AccordionState _folderState;
    private AccordionState _modelState;
    private AccordionState _albumState;
    private bool _showFolders;
    private bool _showModels;
    private bool _showAlbums;
    private FoldersSection? _foldersSection;
    private double _folderHeight;
    private double _albumHeight;
    private double _modelHeight;

    public NavigationSection()
    {
        FoldersSection = new FoldersSection();
    }

    public bool ShowFolders
    {
        get => _showFolders;
        set => SetField(ref _showFolders, value);
    }

    public bool ShowModels
    {
        get => _showModels;
        set => SetField(ref _showModels, value);
    }

    public bool ShowAlbums
    {
        get => _showAlbums;
        set => SetField(ref _showAlbums, value);
    }

    public AccordionState FolderState
    {
        get => _folderState;
        set => SetField(ref _folderState, value);
    }

    public double FolderHeight
    {
        get => _folderHeight;
        set => SetField(ref _folderHeight, value);
    }


    public AccordionState ModelState
    {
        get => _modelState;
        set => SetField(ref _modelState, value);
    }

    public double ModelHeight
    {
        get => _modelHeight;
        set => SetField(ref _modelHeight, value);
    }

    public AccordionState AlbumState
    {
        get => _albumState;
        set => SetField(ref _albumState, value);
    }

    public double AlbumHeight
    {
        get => _albumHeight;
        set => SetField(ref _albumHeight, value);
    }

    public FoldersSection FoldersSection
    {
        get => _foldersSection;
        set => SetField(ref _foldersSection, value);
    }
}

public class FoldersSection : BaseNotify
{
    private bool _canDelete;
    private bool _canRename;

    public bool CanDelete
    {
        get => _canDelete;
        set => SetField(ref _canDelete, value);
    }

    public bool CanRename
    {
        get => _canRename;
        set => SetField(ref _canRename, value);
    }
}

public class MetadataSection : BaseNotify
{
    private AccordionState _promptState;
    private AccordionState _negativePromptState;
    private AccordionState _seedState;
    private AccordionState _samplerState;
    private AccordionState _pathState;
    private AccordionState _albumState;
    private AccordionState _othersState;
    private AccordionState _modelState;
    private AccordionState _dateState;
    private AccordionState _workflowState;

    public AccordionState PromptState
    {
        get => _promptState;
        set => SetField(ref _promptState, value);
    }

    public AccordionState NegativePromptState
    {
        get => _negativePromptState;
        set => SetField(ref _negativePromptState, value);
    }

    public AccordionState SeedState
    {
        get => _seedState;
        set => SetField(ref _seedState, value);
    }

    public AccordionState SamplerState
    {
        get => _samplerState;
        set => SetField(ref _samplerState, value);
    }

    public AccordionState OthersState
    {
        get => _othersState;
        set => SetField(ref _othersState, value);
    }

    public AccordionState ModelState
    {
        get => _modelState;
        set => SetField(ref _modelState, value);
    }

    public AccordionState PathState
    {
        get => _pathState;
        set => SetField(ref _pathState, value);
    }

    public AccordionState DateState
    {
        get => _dateState;
        set => SetField(ref _dateState, value);
    }

    public AccordionState AlbumState
    {
        get => _albumState;
        set => SetField(ref _albumState, value);
    }

    public AccordionState WorkflowState
    {
        get => _workflowState; 
        set => SetField(ref _workflowState, value);
    }
}

public static class SearchControlModelExtensions
{
    public static Filter AsFilter(this FilterControlModel model)
    {
        var filter = new Filter();

        filter.UsePrompt = model.UsePrompt;
        filter.Prompt = model.Prompt;
        filter.UsePromptEx = model.UsePromptEx;
        filter.PromptEx = model.PromptEx;
        filter.UseNegativePrompt = model.UseNegativePrompt;
        filter.NegativePrompt = model.NegativePrompt;
        filter.UseNegativePromptEx = model.UseNegativePromptEx;
        filter.NegativePromptEx = model.NegativePromptEx;

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

        filter.NoAestheticScore = model.NoAestheticScore;
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

        filter.UseInAlbum = model.UseInAlbum;
        filter.InAlbum = model.InAlbum;

        filter.UseUnavailable = model.UseUnavailable;
        filter.Unavailable = model.Unavailable;

        return filter;
    }


}

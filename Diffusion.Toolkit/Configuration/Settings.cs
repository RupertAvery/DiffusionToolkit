using System;
using System.Collections.Generic;
using System.Windows;
using Diffusion.Toolkit.Controls;

namespace Diffusion.Toolkit.Configuration;

public class Settings : SettingsContainer, IScanOptions
{
    public static Settings Instance { get; private set; }

    private List<string> _imagePaths;
    private List<string> _excludePaths;
    private string _modelRootPath;
    private string _fileExtensions;
    private int _pageSize;
    private string? _mainGridWidth;
    private string? _mainGridWidth2;
    private string? _navigationThumbnailGridWidth;
    private string? _navigationThumbnailGridWidth2;
    private string? _previewGridHeight;
    private string? _previewGridHeight2;
    private WindowState? _windowState;
    private Size? _windowSize;
    private bool _dontShowWelcomeOnStartup;
    private string _theme;
    private bool _watchFolders;
    private bool _hideNsfw;
    private bool _hideDeleted;
    private bool _nsfwBlur;
    private bool _scanForNewImagesOnStartup;
    private bool _checkForUpdatesOnStartup;
    private bool _fitToPreview;
    private int _thumbnailSize;
    private bool _autoTagNSFW;
    private List<string> _nsfwTags;
    private string _hashCache;
    private bool _portableMode;
    //private bool? _recurseFolders;
    private bool? _useBuiltInViewer;
    private bool? _openInFullScreen;
    private bool? _useSystemDefault;
    private bool? _useCustomViewer;
    private string _customCommandLine;
    private string _customCommandLineArgs;
    private string _sortAlbumsBy;
    private string _sortBy;
    private string _sortDirection;
    private bool _autoRefresh;
    private string? _culture;
    private bool _actualSize;
    private int _slideShowDelay;
    private bool _scrollNavigation;
    private bool _autoAdvance;

    private double? _top;
    private double? _left;
    private bool _hideUnavailable;
    private List<string> _includeNodeProperties;
    private bool _searchNodes;
    private bool _searchAllProperties;
    private bool _searchRawData;
    private bool _storeMetadata;
    private bool _storeWorkflow;
    private bool _scanUnavailable;
    private bool _showNotifications;
    private List<ExternalApplication> _externalApplications;
    private string _sortQueriesBy;
    private bool _showTags;
    private bool _permanentlyDelete;
    private bool _confirmDeletion;
    private string _version;
    private bool _showFilenames;
    private int _thumbnailSpacing;
    private ThumbnailViewMode _thumbnailViewMode;

    public Settings()
    {
        ImagePaths = new List<string>();
        ExcludePaths = new List<string>();
        NSFWTags = new List<string>() { "nsfw", "nude", "naked" };
        FileExtensions = ".png, .jpg, .jpeg, .webp";
        Theme = "System";
        PageSize = 100;
        ThumbnailSize = 128;
        UseBuiltInViewer = true;
        OpenInFullScreen = true;
        CustomCommandLineArgs = "%1";
        Culture = "default";

        SortAlbumsBy = "Name";
        SortBy = "Date Created";
        SortQueriesBy = "Name";
        
        SortDirection = "Z-A";
        MetadataSection = new MetadataSectionSettings();
        MetadataSection.Attach(this);

        MainGridWidth = "5*";
        MainGridWidth2 = "*";
        NavigationThumbnailGridWidth = "*";
        NavigationThumbnailGridWidth2 = "3*";
        PreviewGridHeight = "*";
        PreviewGridHeight2 = "3*";
        SlideShowDelay = 5;

        WatchFolders = true;
        ScanUnavailable = false;
        ShowNotifications = true;
        FitToPreview = true;
        ExternalApplications = new List<ExternalApplication>();

        SearchNodes = true;
        IncludeNodeProperties = new List<string>
        {
            "text",
            "text_g",
            "text_l",
            "text_positive",
            "text_negative",
        };

        ConfirmDeletion = true;

        NavigationSection = new NavigationSectionSettings();
        NavigationSection.Attach(this);

        //if (initialize)
        //{
        //    RecurseFolders = true;
        //}

        Instance = this;
    }

    public List<string> IncludeNodeProperties
    {
        get => _includeNodeProperties;
        set => UpdateList(ref _includeNodeProperties, value);
    }


    public string FileExtensions
    {
        get => _fileExtensions;
        set => UpdateValue(ref _fileExtensions, value);
    }

    //public bool? RecurseFolders
    //{
    //    get => _recurseFolders;
    //    set => UpdateValue(ref _recurseFolders, value);
    //}

    public List<string> NSFWTags
    {
        get => _nsfwTags;
        set => UpdateList(ref _nsfwTags, value);
    }

    public string ModelRootPath
    {
        get => _modelRootPath;
        set => UpdateValue(ref _modelRootPath, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => UpdateValue(ref _pageSize, value);
    }


    #region Window State
    public string? MainGridWidth
    {
        get => _mainGridWidth;
        set => UpdateValue(ref _mainGridWidth, value);
    }

    public string? MainGridWidth2
    {
        get => _mainGridWidth2;
        set => UpdateValue(ref _mainGridWidth2, value);
    }

    public string? NavigationThumbnailGridWidth
    {
        get => _navigationThumbnailGridWidth;
        set => UpdateValue(ref _navigationThumbnailGridWidth, value);
    }

    public string? NavigationThumbnailGridWidth2
    {
        get => _navigationThumbnailGridWidth2;
        set => UpdateValue(ref _navigationThumbnailGridWidth2, value);
    }


    public string? PreviewGridHeight
    {
        get => _previewGridHeight;
        set => UpdateValue(ref _previewGridHeight, value);
    }

    public string? PreviewGridHeight2
    {
        get => _previewGridHeight2;
        set => UpdateValue(ref _previewGridHeight2, value);
    }
    
    public double? Top
    {
        get => _top;
        set => UpdateValue(ref _top, value);
    }

    public double? Left
    {
        get => _left;
        set => UpdateValue(ref _left, value);
    }

    public WindowState? WindowState
    {
        get => _windowState;
        set => UpdateValue(ref _windowState, value);
    }

    public Size? WindowSize
    {
        get => _windowSize;
        set => UpdateValue(ref _windowSize, value);
    }
    #endregion


    public string Theme
    {
        get => _theme;
        set => UpdateValue(ref _theme, value);
    }

    public bool WatchFolders
    {
        get => _watchFolders;
        set => UpdateValue(ref _watchFolders, value);
    }

    public bool HideNSFW
    {
        get => _hideNsfw;
        set => UpdateValue(ref _hideNsfw, value);
    }

    public bool HideDeleted
    {
        get => _hideDeleted;
        set => UpdateValue(ref _hideDeleted, value);
    }

    public bool NSFWBlur
    {
        get => _nsfwBlur;
        set => UpdateValue(ref _nsfwBlur, value);
    }
  
    public bool CheckForUpdatesOnStartup
    {
        get => _checkForUpdatesOnStartup;
        set => UpdateValue(ref _checkForUpdatesOnStartup, value);
    }

    public bool ScanForNewImagesOnStartup
    {
        get => _scanForNewImagesOnStartup;
        set => UpdateValue(ref _scanForNewImagesOnStartup, value);
    }

    public bool FitToPreview
    {
        get => _fitToPreview;
        set => UpdateValue(ref _fitToPreview, value);
    }

    public int ThumbnailSize
    {
        get => _thumbnailSize;
        set => UpdateValue(ref _thumbnailSize, value);
    }

    public bool AutoTagNSFW
    {
        get => _autoTagNSFW;
        set => UpdateValue(ref _autoTagNSFW, value);
    }

    public string HashCache
    {
        get => _hashCache;
        set => UpdateValue(ref _hashCache, value);
    }

    public bool PortableMode
    {
        get => _portableMode;
        set => UpdateValue(ref _portableMode, value);
    }

    public bool? UseBuiltInViewer
    {
        get => _useBuiltInViewer;
        set => UpdateValue(ref _useBuiltInViewer, value);
    }

    public bool? OpenInFullScreen
    {
        get => _openInFullScreen;
        set => UpdateValue(ref _openInFullScreen, value);
    }

    public bool? UseSystemDefault
    {
        get => _useSystemDefault;
        set => UpdateValue(ref _useSystemDefault, value);
    }

    public bool? UseCustomViewer
    {
        get => _useCustomViewer;
        set => UpdateValue(ref _useCustomViewer, value);
    }

    public string CustomCommandLine
    {
        get => _customCommandLine;
        set => UpdateValue(ref _customCommandLine, value);
    }

    public string CustomCommandLineArgs
    {
        get => _customCommandLineArgs;
        set => UpdateValue(ref _customCommandLineArgs, value);
    }

    public string SortAlbumsBy
    {
        get => _sortAlbumsBy;
        set => UpdateValue(ref _sortAlbumsBy, value);
    }

    public string SortBy
    {
        get => _sortBy;
        set => UpdateValue(ref _sortBy, value);
    }

    public string SortDirection
    {
        get => _sortDirection;
        set => UpdateValue(ref _sortDirection, value);
    }

    public bool AutoRefresh
    {
        get => _autoRefresh;
        set => UpdateValue(ref _autoRefresh, value);
    }

    public string? Culture
    {
        get => _culture;
        set => UpdateValue(ref _culture, value);
    }

    public MetadataSectionSettings MetadataSection { get; set; }

    public NavigationSectionSettings NavigationSection { get; set; }

    public bool ActualSize
    {
        get => _actualSize;
        set => UpdateValue(ref _actualSize, value);
    }

    public int SlideShowDelay
    {
        get => _slideShowDelay;
        set => UpdateValue(ref _slideShowDelay, value);
    }

    public bool ScrollNavigation
    {
        get => _scrollNavigation;
        set => UpdateValue(ref _scrollNavigation, value);
    }

    public bool AutoAdvance
    {
        get => _autoAdvance;
        set => UpdateValue(ref _autoAdvance, value);
    }

    public bool HideUnavailable
    {
        get => _hideUnavailable;
        set => UpdateValue(ref _hideUnavailable, value);
    }

    public bool SearchNodes
    {
        get => _searchNodes;
        set => UpdateValue(ref _searchNodes, value);
    }

    public bool SearchAllProperties
    {
        get => _searchAllProperties;
        set => UpdateValue(ref _searchAllProperties, value);
    }

    public bool SearchRawData
    {
        get => _searchRawData;
        set => UpdateValue(ref _searchRawData, value);
    }

    public bool StoreMetadata
    {
        get => _storeMetadata;
        set => UpdateValue(ref _storeMetadata, value);
    }

    public bool StoreWorkflow
    {
        get => _storeWorkflow;
        set => UpdateValue(ref _storeWorkflow, value);
    }

    public bool ScanUnavailable
    {
        get => _scanUnavailable;
        set => UpdateValue(ref _scanUnavailable, value);
    }

    public bool ShowNotifications
    {
        get => _showNotifications;
        set => UpdateValue(ref _showNotifications, value);
    }

    public List<ExternalApplication> ExternalApplications
    {
        get => _externalApplications;
        set => UpdateValue(ref _externalApplications, value);
    }

    public string SortQueriesBy
    {
        get => _sortQueriesBy;
        set => UpdateValue(ref _sortQueriesBy, value);
    }


    [Obsolete]
    public List<string> ImagePaths
    {
        get => _imagePaths;
        set => UpdateList(ref _imagePaths, value);
    }

    [Obsolete]
    public List<string> ExcludePaths
    {
        get => _excludePaths;
        set => UpdateList(ref _excludePaths, value);
    }

    public bool ShowTags
    {
        get => _showTags;
        set => UpdateValue(ref _showTags, value);
    }

    public bool PermanentlyDelete
    {
        get => _permanentlyDelete;
        set => UpdateValue(ref _permanentlyDelete, value);
    }

    public string Version
    {
        get => _version;
        set => UpdateValue(ref _version, value);
    }

    public bool ConfirmDeletion
    {
        get => _confirmDeletion;
        set => UpdateValue(ref _confirmDeletion, value);
    }


    public bool ShowFilenames
    {
        get => _showFilenames;
        set => UpdateValue(ref _showFilenames, value);
    }

    public int ThumbnailSpacing
    {
        get => _thumbnailSpacing;
        set => UpdateValue(ref _thumbnailSpacing, value);
    }

    public ThumbnailViewMode ThumbnailViewMode
    {
        get => _thumbnailViewMode;
        set => UpdateValue(ref _thumbnailViewMode, value);
    }
}
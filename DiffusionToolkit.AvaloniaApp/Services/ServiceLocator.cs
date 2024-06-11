using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Thumbnails;

namespace DiffusionToolkit.AvaloniaApp.Services;



public class ServiceLocator
{
    private static DataStore? _dataStore;
    private static NavigationService? _navigationService;
    private static ScanService? _scanManager;
    private static Settings? _settings;
    private static PreviewService? _previewService;
    private static ThumbnailNavigationService? _thumbnailNavigationService;
    private static SearchService? _searchService;
    private static ThumbnailCache? _thumbnailCache;
    private static ThumbnailLoader? _thumbnailLoader;
    private static TaggingService? _taggingService;
    private static NotificationService? _notificationService;

    public static DataStore? DataStore => _dataStore;
    public static ThumbnailCache? ThumbnailCache => _thumbnailCache;
    public static Settings? Settings => _settings;
    public static ThumbnailLoader? ThumbnailLoader => _thumbnailLoader;

    public static void SetThumbnailLoader(ThumbnailLoader thumbnailLoader)
    {
        _thumbnailLoader = thumbnailLoader;
    }

    public static void SetThumbnailCache(ThumbnailCache thumbnailCache)
    {
        _thumbnailCache = thumbnailCache;
    }


    public static void SetDataStore(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public static void SetSettings(Settings? settings)
    {
        _settings = settings;
    }
    
    public static NavigationService NavigationService
    {
        get { return _navigationService ??= new NavigationService(); }
    }

    public static PreviewService PreviewService
    {
        get { return _previewService ??= new PreviewService(); }
    }

    public static ScanService ScanService
    {
        get { return _scanManager ??= new ScanService(); }
    }

    public static ThumbnailNavigationService ThumbnailNavigationService
    {
        get { return _thumbnailNavigationService ??= new ThumbnailNavigationService(); }
    }
    
    public static SearchService SearchService
    {
        get { return _searchService ??= new SearchService(); }
    }


    public static TaggingService TaggingService
    {
        get { return _taggingService ??= new TaggingService(); }
    }

    public static NotificationService NotificationService
    {
        get { return _notificationService ??= new NotificationService();  }
    }
}
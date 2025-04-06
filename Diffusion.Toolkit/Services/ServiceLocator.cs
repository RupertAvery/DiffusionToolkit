using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Thumbnails;

namespace Diffusion.Toolkit.Services;

public class ServiceLocator
{
    private static ThumbnailService _thumbnailService;
    private static DatabaseWriterService? _databaseWriterService;
    private static MetadataScannerService? _metadataScannerService;
    private static FolderService _folderService;
    private static MessageService _messageServuce;
    private static ProgressService _progressService;
    private static DataStore? _dataStore;
    private static NavigationService? _navigationService;
    //private static ScanService? _scanManager;
    private static Settings? _settings;
    private static PreviewService? _previewService;
    private static ThumbnailNavigationService? _thumbnailNavigationService;
    private static SearchService? _searchService;
    private static ThumbnailCache? _thumbnailCache;
    private static ThumbnailService? _thumbnailLoader;
    private static TaggingService? _taggingService;
    private static NotificationService? _notificationService;
    private static ScanningService? _scanningService;

    public static DataStore? DataStore => _dataStore;
    public static ThumbnailCache? ThumbnailCache => _thumbnailCache;
    public static Settings? Settings => _settings;
    public static ThumbnailService? ThumbnailLoader => _thumbnailLoader;
    public static ToastService ToastService { get; set; }
    public static Dispatcher Dispatcher { get; set; }

    public static void SetThumbnailLoader(ThumbnailService thumbnailService)
    {
        _thumbnailLoader = thumbnailService;
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

    //public static NavigationService NavigationService
    //{
    //    get { return _navigationService ??= new NavigationService(); }
    //}

    public static PreviewService PreviewService
    {
        get { return _previewService ??= new PreviewService(); }
    }

    public static ThumbnailNavigationService ThumbnailNavigationService
    {
        get { return _thumbnailNavigationService ??= new ThumbnailNavigationService(); }
    }

    public static SearchService SearchService
    {
        get;
        set;
    }

    public static TaggingService TaggingService
    {
        get { return _taggingService ??= new TaggingService(); }
    }

    public static NotificationService NotificationService
    {
        get { return _notificationService ??= new NotificationService(); }
    }

    public static ScanningService ScanningService
    {
        get { return _scanningService ??= new ScanningService(); }
    }

    public static MainModel? MainModel { get; set; }

    public static FolderService FolderService
    {
        get { return _folderService ??= new FolderService(); }
    }


    public static ProgressService ProgressService
    {
        get { return _progressService ??= new ProgressService(); }
    }

    public static MessageService MessageService
    {
        get;
        set;
    }

    public static MetadataScannerService MetadataScannerService
    {
        get { return _metadataScannerService ??= new MetadataScannerService(); }
    }


    public static DatabaseWriterService DatabaseWriterService
    {
        get { return _databaseWriterService ??= new DatabaseWriterService(); }
    }

    public static ThumbnailService ThumbnailService
    {
        get { return _thumbnailService ??= new ThumbnailService(); }
    }
}


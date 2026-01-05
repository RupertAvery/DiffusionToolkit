using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Thumbnails;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Windows.Threading;
using Diffusion.ComfyUI;

namespace Diffusion.Toolkit.Services;

public class ServiceLocator
{
    private static NavigatorService? _navigatorService;
    private static MessageService? _messageServuce;
    private static DataStore? _dataStore;

    private static NavigationService? _navigationService;

    //private static ScanService? _scanManager;
    private static Settings? _settings;
    private static SearchService? _searchService;
    private static ThumbnailCache? _thumbnailCache;
    private static ThumbnailService? _thumbnailLoader;

    public static DataStore? DataStore => _dataStore;
    public static Settings? Settings => _settings;
    public static ToastService ToastService { get; set; }
    public static Dispatcher Dispatcher { get; set; }
    public static NodePropertyCache NodePropertyCache { get; set; }

    public static void SetDataStore(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public static void SetSettings(Settings? settings)
    {
        _settings = settings;
    }

    public static void SetNavigatorService(NavigatorService navigatorService)
    {
        _navigatorService = navigatorService;
    }

    public static PreviewService PreviewService
    {
        get { return field ??= new PreviewService(); }
    }

    public static ThumbnailNavigationService ThumbnailNavigationService
    {
        get { return field ??= new ThumbnailNavigationService(); }
    }

    public static SearchService SearchService
    {
        get;
        set;
    }

    public static TaggingService TaggingService
    {
        get { return field ??= new TaggingService(); }
    }

    public static NotificationService NotificationService
    {
        get { return field ??= new NotificationService(); }
    }

    public static ScanningService ScanningService
    {
        get { return field ??= new ScanningService(); }
    }

    public static MainModel? MainModel { get; set; }

    public static FolderService FolderService
    {
        get { return field ??= new FolderService(); }
    }


    public static ProgressService ProgressService
    {
        get { return field ??= new ProgressService(); }
    }

    public static MessageService MessageService
    {
        get;
        set;
    }

    public static MetadataScannerService MetadataScannerService
    {
        get { return field ??= new MetadataScannerService(); }
    }


    public static DatabaseWriterService DatabaseWriterService
    {
        get { return field ??= new DatabaseWriterService(); }
    }

    public static ThumbnailService ThumbnailService
    {
        get { return field ??= new ThumbnailService(); }
    }

    public static ContextMenuService ContextMenuService
    {
        get { return field ??= new ContextMenuService(); }
    }

    public static ExternalApplicationsService ExternalApplicationsService
    {
        get { return field ??= new ExternalApplicationsService(); }
    }

    public static NavigatorService NavigatorService
    {
        get { return _navigatorService; }
    }

    public static FileService FileService
    {
        get { return field ??= new FileService(); }
    }

    public static AlbumService AlbumService
    {
        get { return field ??= new AlbumService(); }
    }

    public static TagService TagService
    {
        get { return field ??= new TagService(); }
    }

    public static WindowService WindowService
    {
        get { return field ??= new WindowService(); }
    }
}


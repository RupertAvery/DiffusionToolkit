using System.Threading;
using Diffusion.Database;

namespace DiffusionToolkit.AvaloniaApp.Common;



public class ServiceLocator
{
    private static DataStore? _dataStore;
    private static NavigationManager? _navigationManager;
    private static ScanManager? _scanManager;
    private static Settings? _settings;
    private static PreviewManager? _previewManager;
    private static ThumbnailNavigationManager? _thumbnailNavigationManager;
    private static SearchManager? _searchManager;

    public static DataStore? DataStore => _dataStore;
    public static Settings? Settings => _settings;

    public static void SetDataStore(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public static void SetSettings(Settings? settings)
    {
        _settings = settings;
    }
    
    public static NavigationManager NavigationManager
    {
        get { return _navigationManager ??= new NavigationManager(); }
    }

    public static PreviewManager PreviewManager
    {
        get { return _previewManager ??= new PreviewManager(); }
    }

    public static ScanManager ScanManager
    {
        get { return _scanManager ??= new ScanManager(); }
    }

    public static ThumbnailNavigationManager ThumbnailNavigationManager
    {
        get { return _thumbnailNavigationManager ??= new ThumbnailNavigationManager(); }
    }
    
    public static SearchManager SearchManager
    {
        get { return _searchManager ??= new SearchManager(); }
    }

}
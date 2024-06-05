using System.Threading;
using Diffusion.Database;

namespace DiffusionToolkit.AvaloniaApp.Common;

public class ServiceLocator
{
    private static DataStore? _dataStore;
    private static NavigationManager? _navigationManager;
    private static ScanManager? _scanManager;
    private static Settings? _settings;

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

    public static ScanManager ScanManager
    {
        get { return _scanManager ??= new ScanManager(); }
    }

}
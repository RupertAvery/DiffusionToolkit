using Diffusion.Database;

namespace DiffusionToolkit.AvaloniaApp.Common;

public class ServiceLocator
{
    private static DataStore? _dataStore;

    public static DataStore DataStore
    {
        get
        {
            var path = @"C:\Users\ruper\AppData\Roaming\DiffusionToolkit\diffusion-toolkit.db";

            if (_dataStore == null)
            {
                _dataStore = new DataStore(path);
            }

            return _dataStore;
        }
    }
}
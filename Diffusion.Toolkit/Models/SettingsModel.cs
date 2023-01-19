using System.Collections.ObjectModel;

namespace Diffusion.Toolkit;

public class SettingsModel : BaseNotify
{
    private string _modelRootPath;
    private ObservableCollection<string> _imagePaths;
    private int _selectedIndex;
    private string _fileExtensions;
    private int _pageSize;
    private string _theme;
    private bool _checkForUpdatesOnStartup;
    private bool _scanForNewImagesOnStartup;
    private bool _autoTagNsfw;
    private string _nsfwTags;
    private string _hashCache;

    public SettingsModel()
    {
        _imagePaths = new ObservableCollection<string>();
    }

    public ObservableCollection<string> ImagePaths
    {
        get => _imagePaths;
        set => SetField(ref _imagePaths, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetField(ref _selectedIndex, value);
    }

    public string FileExtensions
    {
        get => _fileExtensions;
        set => SetField(ref _fileExtensions, value);
    }

    public string ModelRootPath
    {
        get => _modelRootPath;
        set => SetField(ref _modelRootPath, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => SetField(ref _pageSize, value);
    }

    public string Theme
    {
        get => _theme;
        set => SetField(ref _theme, value);
    }

    public bool WatchFolders
    {
        get;
        set;
    }

    public bool CheckForUpdatesOnStartup
    {
        get => _checkForUpdatesOnStartup;
        set => SetField(ref _checkForUpdatesOnStartup, value);
    }

    public bool ScanForNewImagesOnStartup
    {
        get => _scanForNewImagesOnStartup;
        set => SetField(ref _scanForNewImagesOnStartup, value);
    }

    public bool AutoTagNSFW
    {
        get => _autoTagNsfw;
        set => SetField(ref _autoTagNsfw, value);
    }

    public string NSFWTags
    {
        get => _nsfwTags;
        set => SetField(ref _nsfwTags, value);
    }

    public string HashCache
    {
        get => _hashCache;
        set => SetField(ref _hashCache, value);
    }
}
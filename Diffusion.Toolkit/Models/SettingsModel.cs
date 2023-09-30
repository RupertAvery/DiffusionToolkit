using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diffusion.Toolkit;

public class SettingsModel : BaseNotify
{
    private string _modelRootPath;
    private ObservableCollection<string> _imagePaths;
    private int _selectedIndex;
    private int _excludedSelectedIndex;
    private string _fileExtensions;
    private int _pageSize;
    private string _theme;
    private bool _checkForUpdatesOnStartup;
    private bool _scanForNewImagesOnStartup;
    private bool _autoTagNsfw;
    private string _nsfwTags;
    private string _hashCache;
    private bool _portableMode;
    private bool _watchFolders;
    private ObservableCollection<string> _excludePaths;
    private bool? _recurseFolders;
    private ICommand _escape;

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

    public int ExcludedSelectedIndex
    {
        get => _excludedSelectedIndex;
        set => SetField(ref _excludedSelectedIndex, value);
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
        get => _watchFolders;
        set => SetField(ref _watchFolders, value);
    }

    public bool CheckForUpdatesOnStartup
    {
        get => _checkForUpdatesOnStartup;
        set => SetField(ref _checkForUpdatesOnStartup, value);
    }

    public bool PortableMode
    {
        get => _portableMode;
        set => SetField(ref _portableMode, value);
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

    public ObservableCollection<string> ExcludePaths
    {
        get => _excludePaths;
        set => SetField(ref _excludePaths, value);
    }

    public bool? RecurseFolders
    {
        get => _recurseFolders;
        set => SetField(ref _recurseFolders, value);
    }

    public ICommand Escape
    {
        get => _escape;
        set => SetField(ref _escape, value);
    }
}
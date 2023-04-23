using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Diffusion.Toolkit;

public interface IScanOptions
{

    List<string> ImagePaths {  get; set; }

    List<string> ExcludePaths { get; set; }

    bool? RecurseFolders { get; set; }

    string FileExtensions { get; set; }

}

public class Settings : IScanOptions
{
    private List<string> _imagePaths;
    private List<string> _excludePaths;
    private string _modelRootPath;
    private string _fileExtensions;
    private int _pageSize;
    private string _mainGridWidth;
    private string _mainGridWidth2;
    private WindowState? _windowState;
    private Size? _windowSize;
    private bool _dontShowWelcomeOnStartup;
    private string _theme;
    private bool _watchFolders;
    private bool _hideNsfw;
    private bool _nsfwBlur;
    private bool _scanForNewImagesOnStartup;
    private bool _checkForUpdatesOnStartup;
    private bool _fitToPreview;
    private int _thumbnailSize;
    private bool _autoTagNSFW;
    private List<string> _nsfwTags;
    private string _hashCache;
    private bool _portableMode;
    private bool? _showAlbumPanel;
    private bool? _recurseFolders;
    private readonly Dictionary<string, bool> _isPropertyDirty = new Dictionary<string, bool>();
    private bool _isDirty;

    public bool IsDirty()
    {
        return _isDirty;
    }

    public Settings() : this(false)
    {

    }
    
    public Settings(bool initialize)
    {
        DontShowWelcomeOnStartup = false;
        ImagePaths = new List<string>();
        ExcludePaths = new List<string>();
        NSFWTags = new List<string>() { "nsfw", "nude", "naked" };
        FileExtensions = ".png, .jpg, .jpeg, .webp";
        Theme = "System";
        PageSize = 100;
        ThumbnailSize = 128;
        if (initialize)
        {
            ShowAlbumPanel = true;
            RecurseFolders = true;
        }
    }

  
 
    public bool IsPropertyDirty(string name)
    {
        return _isPropertyDirty.TryGetValue(name, out bool val) && val;
    }

    public void SetPristine()
    {
        _isDirty = false;
        _isPropertyDirty.Clear();
    }


    public List<string> ImagePaths
    {
        get => _imagePaths;
        set => UpdateList(ref _imagePaths, value);
    }

    public List<string> ExcludePaths
    {
        get => _excludePaths;
        set => UpdateList(ref _excludePaths, value);
    }

    public string FileExtensions
    {
        get => _fileExtensions;
        set => UpdateValue(ref _fileExtensions, value);
    }

    public bool? RecurseFolders
    {
        get => _recurseFolders;
        set => UpdateValue(ref _recurseFolders, value);
    }

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
    public string MainGridWidth
    {
        get => _mainGridWidth;
        set => UpdateValue(ref _mainGridWidth, value);
    }

    public string MainGridWidth2
    {
        get => _mainGridWidth2;
        set => UpdateValue(ref _mainGridWidth2, value);
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

    public bool NSFWBlur
    {
        get => _nsfwBlur;
        set => UpdateValue(ref _nsfwBlur, value);
    }
    public bool DontShowWelcomeOnStartup
    {
        get => _dontShowWelcomeOnStartup;
        set => UpdateValue(ref _dontShowWelcomeOnStartup, value);
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

    public bool? ShowAlbumPanel
    {
        get => _showAlbumPanel;
        set => UpdateValue(ref _showAlbumPanel, value);
    }

    public bool PortableMode
    {
        get => _portableMode;
        set => UpdateValue(ref _portableMode, value);
    }

    public void Apply(Settings settings)
    {
        var props = typeof(Settings).GetProperties();

        foreach (var prop in props)
        {
            var value = prop.GetValue(settings);
            prop.SetValue(this, value);
        }
    }

    private void UpdateValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        _isPropertyDirty[propertyName] = true;
        _isDirty = true;
    }

    private void UpdateList<T>(ref List<T>? field, List<T>? value, [CallerMemberName] string propertyName = "")
    {
        bool hasDiff = false;
        if (field != null && value != null)
        {
            var firstNotSecond = field.Except(value).ToList();
            var secondNotFirst = value.Except(field).ToList();

            hasDiff = firstNotSecond.Any() || secondNotFirst.Any();
        }
        else if (field == null && value != null)
        {
            hasDiff = true;
        }
        else
        {
            hasDiff = value != null || value!.Any();
        }

        if (!hasDiff) return;
        field = value;
        _isPropertyDirty[propertyName] = true;
        _isDirty = true;
    }

}

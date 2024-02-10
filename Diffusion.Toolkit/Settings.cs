using System;
using Diffusion.Toolkit.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.ComponentModel;

namespace Diffusion.Toolkit;

public interface IScanOptions
{

    List<string> ImagePaths {  get; set; }

    List<string> ExcludePaths { get; set; }

    bool? RecurseFolders { get; set; }

    string FileExtensions { get; set; }

}

public class SettingChangedEventArgs
{
    public string SettingName { get; set; }
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}

public delegate void SettingChangedEventHander(object sender, SettingChangedEventArgs args);


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
    private bool _nsfwBlur;
    private bool _scanForNewImagesOnStartup;
    private bool _checkForUpdatesOnStartup;
    private bool _fitToPreview;
    private int _thumbnailSize;
    private bool _autoTagNSFW;
    private List<string> _nsfwTags;
    private string _hashCache;
    private bool _portableMode;
    private bool? _recurseFolders;
    private bool? _useBuiltInViewer;
    private bool? _openInFullScreen;
    private bool? _useSystemDefault;
    private bool? _useCustomViewer;
    private string _customCommandLine;
    private string _customCommandLineArgs;
    private string _sortAlbumsBy;
    private string _sortBy;
    private string _sortDirection;

    public Settings() : this(false)
    {
        Instance = this;
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
        UseBuiltInViewer = true;
        OpenInFullScreen = true;
        CustomCommandLineArgs = "%1";
        SortAlbumsBy = "Name";
        Culture = "default";
        SortBy = "Date Created";
        SortDirection = "Z-A";
        MetadataSection = new MetadataSectionSettings();
        MetadataSection.Attach(this);

        NavigationSection = new NavigationSectionSettings(initialize);
        NavigationSection.Attach(this);

        if (initialize)
        {
            RecurseFolders = true;
        }
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

    private double? _top;
    private double? _left;
    
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


    private bool _autoRefresh;

    public bool AutoRefresh
    {
        get => _autoRefresh;
        set => UpdateValue(ref _autoRefresh, value);
    }

    private string? _culture;

    public string? Culture
    {
        get => _culture;
        set => UpdateValue(ref _culture, value);
    }

    public MetadataSectionSettings MetadataSection { get; set; }

    public NavigationSectionSettings NavigationSection { get; set; }


}

public static class TypeHelpers
{
    public static void Copy(object source, object dest)
    {
        var props = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            var value = prop.GetValue(source);

            //if (value.GetType().IsClass)
            //{
            //    var newObject = Activator.CreateInstance(value.GetType());
            //    Copy(value, newObject);
            //    prop.SetValue(dest, newObject);
            //}
            //else
            //{
            //    prop.SetValue(dest, value);
            //}
            prop.SetValue(dest, value);
        }
    }
}


public abstract class SettingsContainer : INotifyPropertyChanged
{
    private bool _isDirty;
    private readonly Dictionary<string, bool> _isPropertyDirty = new Dictionary<string, bool>();

    public bool IsPropertyDirty(string name)
    {
        return _isPropertyDirty.TryGetValue(name, out bool val) && val;
    }

    public void SetPristine()
    {
        _isDirty = false;
        _isPropertyDirty.Clear();
    }

    public void SetDirty()
    {
        _isDirty = true;
    }


    public bool IsDirty()
    {
        return _isDirty;
    }


    public event SettingChangedEventHander SettingChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool UpdateValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        var oldValue = field;

        field = value;

        _isPropertyDirty[propertyName] = true;

        _isDirty = true;

        SettingChanged?.Invoke(this, new SettingChangedEventArgs()
        {
            SettingName = propertyName,
            OldValue = oldValue,
            NewValue = value,
        });

        OnPropertyChanged(propertyName);

        return true;
    }

    protected bool UpdateList<T>(ref List<T>? field, List<T>? value, [CallerMemberName] string propertyName = "")
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

        if (!hasDiff) return false;

        field = value;
        _isPropertyDirty[propertyName] = true;
        _isDirty = true;

        OnPropertyChanged(propertyName);

        return true;
    }

}

public class NavigationSectionSettings : SettingsContainer
{
    private AccordionState _modelState;
    private AccordionState _albumState;
    private AccordionState _folderState;
    private bool _showFolders;
    private bool _showModels;
    private bool _showAlbums;
    
    public NavigationSectionSettings()
    {

    }

    public NavigationSectionSettings(bool initialize)
    {
        if (initialize)
        {
            ShowFolders = true;
            ShowModels = true;
            ShowAlbums = true;
        }
    }

    public AccordionState ModelState
    {
        get => _modelState;
        set => UpdateValue(ref _modelState, value);
    }

    public AccordionState AlbumState
    {
        get => _albumState;
        set => UpdateValue(ref _albumState, value);
    }

    public AccordionState FolderState
    {
        get => _folderState;
        set => UpdateValue(ref _folderState, value);
    }

    public bool ShowFolders
    {
        get => _showFolders;
        set
        {
            UpdateValue(ref _showFolders, value);
            OnPropertyChanged(nameof(ShowSection));
        }
    }

    public bool ShowModels
    {
        get => _showModels;
        set
        {
            UpdateValue(ref _showModels, value);
            OnPropertyChanged(nameof(ShowSection));
        }
    }

    public bool ShowAlbums
    {
        get => _showAlbums;
        set
        {
            UpdateValue(ref _showAlbums, value);
            OnPropertyChanged(nameof(ShowSection));
        }
    }

    public bool ShowSection
    {
        get => _showFolders || _showModels || ShowAlbums;
    }

    public void Attach(Settings settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }
}


public class MetadataSectionSettings : SettingsContainer
{
    private AccordionState _promptState;
    private AccordionState _negativePromptState;
    private AccordionState _seedState;
    private AccordionState _samplerState;
    private AccordionState _pathState;
    private AccordionState _albumState;
    private AccordionState _othersState;
    private AccordionState _modelState;
    private AccordionState _dateState;

    public void Attach(Settings settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }

    public AccordionState PromptState
    {
        get => _promptState;
        set => UpdateValue(ref _promptState, value);
    }

    public AccordionState NegativePromptState
    {
        get => _negativePromptState;
        set => UpdateValue(ref _negativePromptState, value);
    }

    public AccordionState SeedState
    {
        get => _seedState;
        set => UpdateValue(ref _seedState, value);
    }
    
    public AccordionState SamplerState
    {
        get => _samplerState;
        set => UpdateValue(ref _samplerState, value);
    }

    public AccordionState PathState
    {
        get => _pathState;
        set => UpdateValue(ref _pathState, value);
    }

    public AccordionState AlbumState
    {
        get => _albumState;
        set => UpdateValue(ref _albumState, value);
    }

    public AccordionState OthersState
    {
        get => _othersState;
        set => UpdateValue(ref _othersState, value);
    }

    public AccordionState ModelState
    {
        get => _modelState;
        set => UpdateValue(ref _modelState, value);
    }

    public AccordionState DateState
    {
        get => _dateState;
        set => UpdateValue(ref _dateState, value);
    }

}
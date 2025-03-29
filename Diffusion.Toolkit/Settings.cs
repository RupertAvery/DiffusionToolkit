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

    List<string> ImagePaths { get; set; }

    List<string> ExcludePaths { get; set; }

    bool? RecurseFolders { get; set; }

    string FileExtensions { get; set; }

    bool StoreMetadata { get; set; }

    bool StoreWorkflow { get; set; }
    bool ScanUnavailable { get; set; }
}

public class SettingChangedEventArgs
{
    public string SettingName { get; set; }
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}

public delegate void SettingChangedEventHander(object sender, SettingChangedEventArgs args);


public class ExternalApplication
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string CommandLineArgs { get; set; }
}

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
    private bool _hideDeleted;
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
    private bool _autoRefresh;
    private string? _culture;
    private bool _actualSize;
    private int _slideShowDelay;
    private bool _scrollNavigation;
    private bool _autoAdvance;

    private double? _top;
    private double? _left;
    private bool _hideUnavailable;
    private List<string> _includeNodeProperties;
    private bool _searchNodes;
    private bool _searchAllProperties;
    private bool _searchRawData;
    private bool _storeMetadata;
    private bool _storeWorkflow;
    private bool _scanUnavailable;
    private bool _showNotifications;
    private List<ExternalApplication> _externalApplications;

    public Settings() : this(false)
    {
        Instance = this;
    }

    public Settings(bool initialize)
    {
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

        MainGridWidth = "5*";
        MainGridWidth2 = "*";
        NavigationThumbnailGridWidth = "*";
        NavigationThumbnailGridWidth2 = "3*";
        PreviewGridHeight = "*";
        PreviewGridHeight2 = "3*";
        SlideShowDelay = 5;

        WatchFolders = true;
        ScanUnavailable = false;
        ShowNotifications = true;
        FitToPreview = true;
        ExternalApplications = new List<ExternalApplication>();

        SearchNodes = true;
        IncludeNodeProperties = new List<string>
        {
            "text",
            "text_g",
            "text_l",
            "text_positive",
            "text_negative",
        };

        NavigationSection = new NavigationSectionSettings(initialize);
        NavigationSection.Attach(this);

        if (initialize)
        {
            RecurseFolders = true;
        }
    }

    public List<string> IncludeNodeProperties
    {
        get => _includeNodeProperties;
        set => UpdateList(ref _includeNodeProperties, value);
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

    public bool HideDeleted
    {
        get => _hideDeleted;
        set => UpdateValue(ref _hideDeleted, value);
    }

    public bool NSFWBlur
    {
        get => _nsfwBlur;
        set => UpdateValue(ref _nsfwBlur, value);
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

    public bool AutoRefresh
    {
        get => _autoRefresh;
        set => UpdateValue(ref _autoRefresh, value);
    }

    public string? Culture
    {
        get => _culture;
        set => UpdateValue(ref _culture, value);
    }

    public MetadataSectionSettings MetadataSection { get; set; }

    public NavigationSectionSettings NavigationSection { get; set; }

    public bool ActualSize
    {
        get => _actualSize;
        set => UpdateValue(ref _actualSize, value);
    }

    public int SlideShowDelay
    {
        get => _slideShowDelay;
        set => UpdateValue(ref _slideShowDelay, value);
    }

    public bool ScrollNavigation
    {
        get => _scrollNavigation;
        set => UpdateValue(ref _scrollNavigation, value);
    }

    public bool AutoAdvance
    {
        get => _autoAdvance;
        set => UpdateValue(ref _autoAdvance, value);
    }

    public bool HideUnavailable
    {
        get => _hideUnavailable;
        set => UpdateValue(ref _hideUnavailable, value);
    }

    public bool SearchNodes
    {
        get => _searchNodes;
        set => UpdateValue(ref _searchNodes, value);
    }

    public bool SearchAllProperties
    {
        get => _searchAllProperties;
        set => UpdateValue(ref _searchAllProperties, value);
    }

    public bool SearchRawData
    {
        get => _searchRawData;
        set => UpdateValue(ref _searchRawData, value);
    }

    public bool StoreMetadata
    {
        get => _storeMetadata;
        set => UpdateValue(ref _storeMetadata, value);
    }

    public bool StoreWorkflow
    {
        get => _storeWorkflow;
        set => UpdateValue(ref _storeWorkflow, value);
    }

    public bool ScanUnavailable
    {
        get => _scanUnavailable;
        set => UpdateValue(ref _scanUnavailable, value);
    }

    public bool ShowNotifications
    {
        get => _showNotifications;
        set => UpdateValue(ref _showNotifications, value);
    }

    public List<ExternalApplication> ExternalApplications
    {
        get => _externalApplications;
        set => UpdateValue(ref _externalApplications, value);
    }
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
    private AccordionState _folderState;
    private AccordionState _modelState;
    private AccordionState _albumState;
    private bool _showFolders;
    private bool _showModels;
    private bool _showAlbums;
    private double _folderHeight;
    private double _modelHeight;
    private double _albumHeight;
    private bool _showSection;

    public NavigationSectionSettings()
    {
        FolderHeight = Double.PositiveInfinity;
        ModelHeight = Double.PositiveInfinity;
        AlbumHeight = Double.PositiveInfinity;
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

    public AccordionState FolderState
    {
        get => _folderState;
        set => UpdateValue(ref _folderState, value);
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

    public double FolderHeight
    {
        get => _folderHeight;
        set => UpdateValue(ref _folderHeight, value);
    }

    public double ModelHeight
    {
        get => _modelHeight;
        set => UpdateValue(ref _modelHeight, value);
    }

    public double AlbumHeight
    {
        get => _albumHeight;
        set => UpdateValue(ref _albumHeight, value);
    }

    public bool ShowFolders
    {
        get => _showFolders;
        set
        {
            UpdateValue(ref _showFolders, value);
            UpdateShowSection();
        }
    }

    public bool ShowModels
    {
        get => _showModels;
        set
        {
            UpdateValue(ref _showModels, value);
            UpdateShowSection();
        }
    }

    public bool ShowAlbums
    {
        get => _showAlbums;
        set
        {
            UpdateValue(ref _showAlbums, value);
            UpdateShowSection();
        }
    }

    private bool HasVisibilePanels => _showFolders || _showModels || ShowAlbums;

    private void UpdateShowSection()
    {
        ShowSection = HasVisibilePanels;
    }

    public void ToggleSection()
    {
        ShowSection = HasVisibilePanels && !ShowSection;
        //if (HasVisibilePanels)
        //{
        //    ShowSection = !ShowSection;
        //}
        //else
        //{
        //    ShowSection = false;
        //}
    }

    public bool ShowSection
    {
        get => _showSection;
        set => UpdateValue(ref _showSection, value);
    }

    public void Attach(Settings settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }
}


public class AccordionSetting : SettingsContainer
{
    private AccordionState _accordionState;
    private double _containerHeight;

    public void Attach(SettingsContainer settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }
    public AccordionState AccordionState
    {
        get => _accordionState;
        set => UpdateValue(ref _accordionState, value);
    }

    public double ContainerHeight
    {
        get => _containerHeight;
        set => UpdateValue(ref _containerHeight, value);
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

    public void Attach(SettingsContainer settings)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Diffusion.Toolkit.Models;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace Diffusion.Toolkit;

public class Langauge
{
    public string Name { get; set; }
    public string Culture { get; set; }

    public Langauge(string name, string culture)
    {
        Name = name;
        Culture = culture;
    }
}


public class SettingsModel : BaseNotify
{
    private string _modelRootPath;
    private ObservableCollection<string> _imagePaths;
    private int _selectedIndex;
    private int _excludedSelectedIndex;
    private string _fileExtensions;
    private int _pageSize;
    private string _theme;
    private string _culture;
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
    private bool _useBuiltInViewer;
    private bool _openInFullScreen;
    private bool _useSystemDefault;
    private bool _useCustomViewer;
    private string _customCommandLine;
    private string _customCommandLineArgs;
    private bool _autoRefresh;
    private IEnumerable<Langauge> _cultures;
    private int _slideShowDelay;
    private bool _scrollNavigation;
    private bool _advanceOnTag;
    private bool _storeMetadata;
    private bool _storeWorkflow;
    private ObservableCollection<ExternalApplicationModel> _externalApplications;
    private bool _scanUnavailable;
    private ExternalApplicationModel? _selectedApplication;
    private IEnumerable<OptionValue> _themeOptions;
    private bool _isFoldersDirty;

    public SettingsModel()
    {
        ImagePaths = new ObservableCollection<string>();
        ExcludePaths = new ObservableCollection<string>();
        SelectedIndex = -1;
        ExcludedSelectedIndex = -1;
        ExternalApplications = new ObservableCollection<ExternalApplicationModel>();
    }

    public ObservableCollection<string> ImagePaths
    {
        get => _imagePaths;
        set
        {
            SetField(ref _imagePaths, value, false);
            RegisterObservableChanges(_imagePaths, SetFoldersDirty);
        }
    }

    public ObservableCollection<string> ExcludePaths
    {
        get => _excludePaths;
        set
        {
            SetField(ref _excludePaths, value, false);
            RegisterObservableChanges(_excludePaths, SetFoldersDirty);
        }
    }

    public bool? RecurseFolders
    {
        get => _recurseFolders;
        set
        {
            SetField(ref _recurseFolders, value, false);
            SetFoldersDirty();
        }
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetField(ref _selectedIndex, value, false);
    }

    public int ExcludedSelectedIndex
    {
        get => _excludedSelectedIndex;
        set => SetField(ref _excludedSelectedIndex, value, false);
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

    public string Culture
    {
        get => _culture;
        set => SetField(ref _culture, value);
    }

    public IEnumerable<Langauge> Cultures
    {
        get => _cultures;
        set => SetField(ref _cultures, value);
    }

    public bool WatchFolders
    {
        get => _watchFolders;
        set => SetField(ref _watchFolders, value);
    }
    
    public bool AutoRefresh
    {
        get => _autoRefresh;
        set => SetField(ref _autoRefresh, value);
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

    public ICommand Escape
    {
        get => _escape;
        set => SetField(ref _escape, value);
    }

    public bool UseBuiltInViewer
    {
        get => _useBuiltInViewer;
        set => SetField(ref _useBuiltInViewer, value);
    }

    public bool OpenInFullScreen
    {
        get => _openInFullScreen;
        set => SetField(ref _openInFullScreen, value);
    }

    public bool UseSystemDefault
    {
        get => _useSystemDefault;
        set => SetField(ref _useSystemDefault, value);
    }

    public bool UseCustomViewer
    {
        get => _useCustomViewer;
        set => SetField(ref _useCustomViewer, value);
    }

    public string CustomCommandLine
    {
        get => _customCommandLine;
        set => SetField(ref _customCommandLine, value);
    }
    public string CustomCommandLineArgs
    {
        get => _customCommandLineArgs;
        set => SetField(ref _customCommandLineArgs, value);
    }

    public IEnumerable<OptionValue> ThemeOptions
    {
        get => _themeOptions;
        set => _themeOptions = value;
    }

    public int SlideShowDelay
    {
        get => _slideShowDelay;
        set => SetField(ref _slideShowDelay, value);
    }

    public bool ScrollNavigation
    {
        get => _scrollNavigation;
        set => SetField(ref _scrollNavigation, value);
    }

    public bool AdvanceOnTag
    {
        get => _advanceOnTag;
        set => SetField(ref _advanceOnTag, value);
    }

    public bool StoreMetadata
    {
        get => _storeMetadata;
        set => SetField(ref _storeMetadata, value);
    }

    public bool StoreWorkflow
    {
        get => _storeWorkflow;
        set => SetField(ref _storeWorkflow, value);
    }

    public bool ScanUnavailable
    {
        get => _scanUnavailable;
        set => SetField(ref _scanUnavailable, value);
    }

    public ObservableCollection<ExternalApplicationModel> ExternalApplications
    {
        get => _externalApplications;
        set
        {
            SetField(ref _externalApplications, value);
            RegisterObservableChanges(_externalApplications);
        }
    }

    public ExternalApplicationModel? SelectedApplication
    {
        get => _selectedApplication;
        set => SetField(ref _selectedApplication, value, false);
    }

    public override bool IsDirty => _isDirty || _isFoldersDirty;

    public bool IsFoldersDirty
    {
        get => _isFoldersDirty;
    }


    public void SetFoldersDirty()
    {
        _isFoldersDirty = true;
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(IsFoldersDirty));
    }

    public void SetFoldersPristine()
    {
        _isFoldersDirty = false;
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(IsFoldersDirty));
    }

}

public class ExternalApplicationModel : BaseNotify
{
    private string _name;
    private string _path;
    private string _commandLineArgs;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    public string CommandLineArgs
    {
        get => _commandLineArgs;
        set => SetField(ref _commandLineArgs, value);
    }
}
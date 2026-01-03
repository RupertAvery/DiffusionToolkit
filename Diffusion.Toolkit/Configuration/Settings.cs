using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using Diffusion.Toolkit.Controls;

namespace Diffusion.Toolkit.Configuration;

public class Settings : SettingsContainer, IScanOptions
{
    public static Settings Instance { get; private set; }

    private List<string> _imagePaths;
    private List<string> _excludePaths;
    private bool _dontShowWelcomeOnStartup;

    //private bool _watchFolders;
    //private bool? _recurseFolders;

    public Settings()
    {
        NSFWTags = new List<string>() { "nsfw", "nude", "naked" };
        FileExtensions = ".png, .jpg, .jpeg, .webp";
        Theme = "System";
        PageSize = 100;
        ThumbnailSize = 128;
        UseBuiltInViewer = true;
        OpenInFullScreen = true;
        CustomCommandLineArgs = "%1";
        Culture = "default";

        SortAlbumsBy = "Name";
        SortBy = "Date Created";
        SortQueriesBy = "Name";
        
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

        //WatchFolders = true;
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

        ConfirmDeletion = true;

        NavigationSection = new NavigationSectionSettings();
        NavigationSection.Attach(this);
        PreviewWindowState = new PreviewWindowState();

        //if (initialize)
        //{
        //    RecurseFolders = true;
        //}

        Instance = this;
    }

    public List<string> IncludeNodeProperties
    {
        get;
        set => UpdateList(ref field, value);
    }


    public string FileExtensions
    {
        get;
        set => UpdateValue(ref field, value);
    }

    //public bool? RecurseFolders
    //{
    //    get => _recurseFolders;
    //    set => UpdateValue(ref _recurseFolders, value);
    //}

    public List<string> NSFWTags
    {
        get;
        set => UpdateList(ref field, value);
    }

    public string ModelRootPath
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public int PageSize
    {
        get;
        set => UpdateValue(ref field, value);
    }


    #region Window State

    public string? MainGridWidth
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string? MainGridWidth2
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string? NavigationThumbnailGridWidth
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string? NavigationThumbnailGridWidth2
    {
        get;
        set => UpdateValue(ref field, value);
    }


    public string? PreviewGridHeight
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string? PreviewGridHeight2
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public double? Top
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public double? Left
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public WindowState? WindowState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public Size? WindowSize
    {
        get;
        set => UpdateValue(ref field, value);
    }

    #endregion


    public string Theme
    {
        get;
        set => UpdateValue(ref field, value);
    }

    //public bool WatchFolders
    //{
    //    get => _watchFolders;
    //    set => UpdateValue(ref _watchFolders, value);
    //}

    public bool HideNSFW
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool HideDeleted
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool NSFWBlur
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool CheckForUpdatesOnStartup
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool ScanForNewImagesOnStartup
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool FitToPreview
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public int ThumbnailSize
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool AutoTagNSFW
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string HashCache
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool PortableMode
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool? UseBuiltInViewer
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool? OpenInFullScreen
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool? UseSystemDefault
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool? UseCustomViewer
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string CustomCommandLine
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string CustomCommandLineArgs
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string SortAlbumsBy
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string SortBy
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string SortDirection
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool AutoRefresh
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string? Culture
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public MetadataSectionSettings MetadataSection { get; set; }

    public NavigationSectionSettings NavigationSection { get; set; }

    public bool ActualSize
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public int SlideShowDelay
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool ScrollNavigation
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool AutoAdvance
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool HideUnavailable
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool SearchNodes
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool SearchAllProperties
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool SearchRawData
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool StoreMetadata
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool StoreWorkflow
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool ScanUnavailable
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool ShowNotifications
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public List<ExternalApplication> ExternalApplications
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string SortQueriesBy
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool ShowTags
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool PermanentlyDelete
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public string Version
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool ConfirmDeletion
    {
        get;
        set => UpdateValue(ref field, value);
    }


    public bool ShowFilenames
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public int ThumbnailSpacing
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public ThumbnailViewMode ThumbnailViewMode
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public RenderMode RenderMode
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public PreviewWindowState PreviewWindowState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public bool LoopVideo
    {
        get;
        set => UpdateValue(ref field, value);
    }
}

public class PreviewWindowState
{
    public bool IsSet { get; set; }
    public WindowState State { get; set; }
    public double Top { get; set; }
    public double Left { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsFullScreen { get; set; }
}
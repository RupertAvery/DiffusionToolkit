using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Models;

public class SettingsModel : BaseNotify
{
    private ObservableCollection<string> _imagePaths;

    private ObservableCollection<string> _excludePaths;
    //private bool? _recurseFolders;
    private bool _isFoldersDirty;

    public SettingsModel()
    {
        SelectedIndex = -1;
        ExcludedSelectedIndex = -1;
        ExternalApplications = new ObservableCollection<ExternalApplicationModel>();
        //ExternalApplications.CollectionChanged += ExternalApplicationsOnCollectionChanged;

        PropertyChanged += OnPropertyChanged;
    }

    private void ExternalApplicationsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        int i = 1;

        foreach (var application in ExternalApplications)
        {
            application.Shortcut = i switch
            {
                >= 1 and <= 9 => $"Shift+{i}",
                10 => $"Shift+0",
                _ => ""
            };

            i++;
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ServiceLocator.MainModel.IsSettingsDirty = IsDirty;
    }

    public int SelectedIndex
    {
        get;
        set => SetField(ref field, value, false);
    }

    public int ExcludedSelectedIndex
    {
        get;
        set => SetField(ref field, value, false);
    }

    public string FileExtensions
    {
        get;
        set => SetField(ref field, value);
    }

    public string ModelRootPath
    {
        get;
        set => SetField(ref field, value);
    }

    public int PageSize
    {
        get;
        set => SetField(ref field, value);
    }

    public string Theme
    {
        get;
        set => SetField(ref field, value);
    }

    public string Culture
    {
        get;
        set => SetField(ref field, value);
    }

    public IEnumerable<Langauge> Cultures
    {
        get;
        set => SetField(ref field, value);
    }

    public bool AutoRefresh
    {
        get;
        set => SetField(ref field, value);
    }


    public bool CheckForUpdatesOnStartup
    {
        get;
        set => SetField(ref field, value);
    }

    public bool PortableMode
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ScanForNewImagesOnStartup
    {
        get;
        set => SetField(ref field, value);
    }

    public bool AutoTagNSFW
    {
        get;
        set => SetField(ref field, value);
    }

    public string NSFWTags
    {
        get;
        set => SetField(ref field, value);
    }

    public string HashCache
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand Escape
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseBuiltInViewer
    {
        get;
        set => SetField(ref field, value);
    }

    public bool OpenInFullScreen
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseSystemDefault
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseCustomViewer
    {
        get;
        set => SetField(ref field, value);
    }

    public string CustomCommandLine
    {
        get;
        set => SetField(ref field, value);
    }

    public string CustomCommandLineArgs
    {
        get;
        set => SetField(ref field, value);
    }

    public IEnumerable<OptionValue> ThemeOptions
    {
        get;
        set => field = value;
    }

    public int SlideShowDelay
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ScrollNavigation
    {
        get;
        set => SetField(ref field, value);
    }

    public bool AdvanceOnTag
    {
        get;
        set => SetField(ref field, value);
    }

    public bool StoreMetadata
    {
        get;
        set => SetField(ref field, value);
    }

    public bool StoreWorkflow
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ScanUnavailable
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ShowFilenames
    {
        get;
        set => SetField(ref field, value);
    }

    public bool PermanentlyDelete
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ConfirmDeletion
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<ExternalApplicationModel> ExternalApplications
    {
        get;
        set
        {
            SetField(ref field, value);
            RegisterObservableChanges(field);
            field.CollectionChanged += ExternalApplicationsOnCollectionChanged;
        }
    }

    public bool SoftwareOnly
    {
        get;
        set => SetField(ref field, value);
    }

    public ExternalApplicationModel? SelectedApplication
    {
        get;
        set => SetField(ref field, value, false);
    }

    public bool LoopVideo
    {
        get;
        set => SetField(ref field, value);
    }


    public override bool IsDirty => _isDirty;

}
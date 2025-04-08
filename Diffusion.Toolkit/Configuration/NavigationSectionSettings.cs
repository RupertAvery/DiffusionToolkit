using System;

namespace Diffusion.Toolkit.Configuration;

public class NavigationSectionSettings : SettingsContainer
{
    private AccordionState _folderState;
    private AccordionState _modelState;
    private AccordionState _albumState;
    private AccordionState _queryState;
    private bool _showFolders;
    private bool _showModels;
    private bool _showAlbums;
    private bool _showQueries;
    private double _folderHeight;
    private double _modelHeight;
    private double _albumHeight;
    private double _queryHeight;
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
            ShowQueries = true;
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

    public AccordionState QueryState
    {
        get => _queryState;
        set => UpdateValue(ref _queryState, value);
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

    public double QueryHeight
    {
        get => _queryHeight;
        set => UpdateValue(ref _queryHeight, value);
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
    
    public bool ShowQueries
    {
        get => _showQueries;
        set
        {
            UpdateValue(ref _showQueries, value);
            UpdateShowSection();
        }
    }

    private bool HasVisibilePanels => _showFolders || _showModels || _showAlbums || _showQueries;

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
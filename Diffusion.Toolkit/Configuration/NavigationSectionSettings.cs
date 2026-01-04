using System;

namespace Diffusion.Toolkit.Configuration;

public class NavigationSectionSettings : SettingsContainer
{
    private bool _showFolders;
    private bool _showModels;
    private bool _showAlbums;
    private bool _showQueries;

    public NavigationSectionSettings()
    {
        FolderHeight = Double.PositiveInfinity;
        ModelHeight = Double.PositiveInfinity;
        AlbumHeight = Double.PositiveInfinity;
        TagHeight = Double.PositiveInfinity;
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
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState ModelState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState AlbumState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState TagState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState QueryState
    {
        get;
        set => UpdateValue(ref field, value);
    }


    public double FolderHeight
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public double ModelHeight
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public double AlbumHeight
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public double TagHeight
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public double QueryHeight
    {
        get;
        set => UpdateValue(ref field, value);
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
        get;
        set => UpdateValue(ref field, value);
    }


    public void Attach(Settings settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }
}
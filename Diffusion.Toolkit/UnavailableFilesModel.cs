using System.Collections.Generic;
using System.Collections.ObjectModel;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit;

public class ImageFileItem : BaseNotify
{
    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public string Path { get; set; }
    public bool Recursive { get; set; }
    public string DisplayPath { get; set; }
    public bool IsUnavailable { get; set; }
}

public class UnavailableFilesModel : BaseNotify
{
    public bool UseRootFolders
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseSpecificFolders
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<ImageFileItem> ImagePaths
    {
        get;
        set => SetField(ref field, value);
    }

    public int SelectedIndex
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ShowUnavailableRootFolders
    {
        get;
        set => SetField(ref field, value);
    }

    public bool JustUpdate
    {
        get;
        set => SetField(ref field, value);
    }

    public bool RemoveImmediately
    {
        get;
        set => SetField(ref field, value);
    }

    public bool MarkForDeletion
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsStartEnabled
    {
        get;
        set => SetField(ref field, value);
    }
}
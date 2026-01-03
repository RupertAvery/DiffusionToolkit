using System.Diagnostics.CodeAnalysis;
using Diffusion.Toolkit.Configuration;

namespace Diffusion.Toolkit.Models;

public class NavigationSection : BaseNotify
{
    public NavigationSection()
    {
        FoldersSection = new FoldersSection();
    }

    public bool ShowFolders
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ShowModels
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ShowAlbums
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ShowQueries
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState FolderState
    {
        get;
        set => SetField(ref field, value);
    }

    public double FolderHeight
    {
        get;
        set => SetField(ref field, value);
    }


    public AccordionState ModelState
    {
        get;
        set => SetField(ref field, value);
    }

    public double ModelHeight
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState AlbumState
    {
        get;
        set => SetField(ref field, value);
    }

    public double AlbumHeight
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState QueryState
    {
        get;
        set => SetField(ref field, value);
    }

    public double QueryHeight
    {
        get;
        set => SetField(ref field, value);
    }

    [field: AllowNull, MaybeNull]
    public FoldersSection FoldersSection
    {
        get;
        set => SetField(ref field, value);
    }
}
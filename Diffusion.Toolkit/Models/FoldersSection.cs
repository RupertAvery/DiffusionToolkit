namespace Diffusion.Toolkit.Models;

public class FoldersSection : BaseNotify
{
    public bool CanRescan
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanCreateFolder
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanDelete
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanRename
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanArchive
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanUnarchive
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanArchiveTree
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanUnarchiveTree
    {
        get;
        set => SetField(ref field, value);
    }


    public bool CanExclude
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanUnexclude
    {
        get;
        set => SetField(ref field, value);
    }


    public bool CanShowInExplorer
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanRemove
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanExcludeTree
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanUnexcludeTree
    {
        get;
        set => SetField(ref field, value);
    }
}
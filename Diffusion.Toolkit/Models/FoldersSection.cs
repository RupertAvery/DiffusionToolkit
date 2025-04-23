namespace Diffusion.Toolkit.Models;

public class FoldersSection : BaseNotify
{
    private bool _canRescan;
    private bool _canCreateFolder;
    private bool _canDelete;
    private bool _canRename;
    private bool _canRemove;
    private bool _canArchive;
    private bool _canUnarchive;
    private bool _canArchiveTree;
    private bool _canUnarchiveTree;
    private bool _canExclude;
    private bool _canUnexclude;
    private bool _canShowInExplorer;
    private bool _canUnexcludeTree;
    private bool _canExcludeTree;

    public bool CanRescan
    {
        get => _canRescan;
        set => SetField(ref _canRescan, value);
    }

    public bool CanCreateFolder
    {
        get => _canCreateFolder;
        set => SetField(ref _canCreateFolder, value);
    }

    public bool CanDelete
    {
        get => _canDelete;
        set => SetField(ref _canDelete, value);
    }

    public bool CanRename
    {
        get => _canRename;
        set => SetField(ref _canRename, value);
    }

    public bool CanArchive
    {
        get => _canArchive;
        set => SetField(ref _canArchive, value);
    }

    public bool CanUnarchive
    {
        get => _canUnarchive;
        set => SetField(ref _canUnarchive, value);
    }

    public bool CanArchiveTree
    {
        get => _canArchiveTree;
        set => SetField(ref _canArchiveTree, value);
    }

    public bool CanUnarchiveTree
    {
        get => _canUnarchiveTree;
        set => SetField(ref _canUnarchiveTree, value);
    }


    public bool CanExclude
    {
        get => _canExclude;
        set => SetField(ref _canExclude, value);
    }

    public bool CanUnexclude
    {
        get => _canUnexclude;
        set => SetField(ref _canUnexclude, value);
    }


    public bool CanShowInExplorer
    {
        get => _canShowInExplorer;
        set => SetField(ref _canShowInExplorer, value);
    }

    public bool CanRemove
    {
        get => _canRemove;
        set => SetField(ref _canRemove, value);
    }

    public bool CanExcludeTree
    {
        get => _canExcludeTree;
        set => SetField(ref _canExcludeTree, value);
    }

    public bool CanUnexcludeTree
    {
        get => _canUnexcludeTree;
        set => SetField(ref _canUnexcludeTree, value);
    }
}
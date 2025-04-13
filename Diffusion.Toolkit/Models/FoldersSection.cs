namespace Diffusion.Toolkit.Models;

public class FoldersSection : BaseNotify
{
    private bool _canRescan;
    private bool _canCreateFolder;
    private bool _canDelete;
    private bool _canRename;
    private bool _canArchive;
    private bool _canUnarchive;
    private bool _canShowInExplorer;

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

    public bool CanShowInExplorer
    {
        get => _canShowInExplorer;
        set => SetField(ref _canShowInExplorer, value);
    }


}
namespace Diffusion.Toolkit.Models;

public class FoldersSection : BaseNotify
{
    private bool _canDelete;
    private bool _canRename;

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
}
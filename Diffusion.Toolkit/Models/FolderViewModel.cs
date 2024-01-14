using System.Collections.Generic;

namespace Diffusion.Toolkit.Models;

public enum FolderState
{
    Collapsed,
    Expanded
}

public class FolderViewModel : BaseNotify
{
    private FolderState _state;
    private bool _hasChildren;
    private bool _visible;

    public FolderState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    public bool HasChildren
    {
        get => _hasChildren;
        set => SetField(ref _hasChildren, value);
    }

    public FolderViewModel Parent { get; set; }

    public bool Visible
    {
        get => _visible;
        set => SetField(ref _visible, value);
    }

    public List<FolderViewModel>? Children { get; set; }
    public int Depth { get; set; }
    public string Path { get; set; }
    public string Name { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is FolderViewModel folder)
        {
            return this.Path == folder.Path;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.Path.GetHashCode();
    }
}
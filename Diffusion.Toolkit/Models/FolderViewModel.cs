using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Navigation;

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
    private bool _isSelected;
    private string _name;
    private FolderStatus _status;

    public FolderState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
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

    public ObservableCollection<FolderViewModel>? Children { get; set; }

    public int Depth { get; set; }
    public string Path { get; set; }

    public FolderStatus Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

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
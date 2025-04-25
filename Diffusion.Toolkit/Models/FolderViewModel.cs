using Diffusion.Toolkit.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Navigation;

namespace Diffusion.Toolkit.Models;

[DebuggerDisplay("{Path}")]
public class FolderViewModel : BaseNotify
{
    private FolderState _state;
    private bool _hasChildren;
    private bool _visible;
    private bool _isSelected;
    private string _name;
    private bool _isArchived;
    private bool _isExcluded;
    private bool _isRecursive;
    private bool _isUnavailable;
    private bool _isScanned;
    private bool _isBusy;
    private ObservableCollection<FolderViewModel>? _children;
    private bool _isHome;

    public MainModel MainModel => ServiceLocator.MainModel;

    public int Id { get; set; }

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

    public FolderViewModel? Parent { get; set; }

    public bool Visible
    {
        get => _visible;
        set => SetField(ref _visible, value);
    }

    public ObservableCollection<FolderViewModel>? Children
    {
        get => _children;
        set => SetField(ref _children, value);
    }

    public int Depth { get; set; }
    public string Path { get; set; }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public bool IsUnavailable
    {
        get => _isUnavailable;
        set => SetField(ref _isUnavailable, value);
    }


    public bool IsArchived
    {
        get => _isArchived;
        set => SetField(ref _isArchived, value);
    }

    public bool IsExcluded
    {
        get => _isExcluded;
        set => SetField(ref _isExcluded, value);
    }

    public bool IsRecursive
    {
        get => _isRecursive;
        set => SetField(ref _isRecursive, value);
    }

    public bool IsScanned
    {
        get => _isScanned;
        set
        {
            SetField(ref _isScanned, value);
            OnPropertyChanged(nameof(IsNotScanned));
        }
    }

    public bool IsNotScanned
    {
        get => !_isScanned;
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }

    public bool IsHome
    {
        get => _isHome;
        set => SetField(ref _isHome, value);
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

    public static FolderViewModel Home = new FolderViewModel()
    {
        Path = "Root Folders",
        Depth = -1,
        IsHome = true,
        IsScanned = true,
    };
}

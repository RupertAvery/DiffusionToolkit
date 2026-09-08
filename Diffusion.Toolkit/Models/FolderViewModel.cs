using Diffusion.Toolkit.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Navigation;

namespace Diffusion.Toolkit.Models;

[DebuggerDisplay("{Path}")]
public class FolderViewModel : BaseNotify
{
    private bool _isScanned;

    public MainModel MainModel => ServiceLocator.MainModel;

    public int Id { get; set; }

    public FolderState State
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public bool HasChildren
    {
        get;
        set => SetField(ref field, value);
    }

    public FolderViewModel? Parent { get; set; }

    public bool Visible
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<FolderViewModel>? Children
    {
        get;
        set => SetField(ref field, value);
    }

    public int Depth { get; set; }
    public string Path { get; set; }

    public bool IsRoot => Depth == 0;

    public string Name
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsUnavailable
    {
        get;
        set => SetField(ref field, value);
    }


    public bool IsArchived
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsExcluded
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsRecursive
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ForRemoval { get; set; }

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
        get;
        set => SetField(ref field, value);
    }

    public bool IsHome
    {
        get;
        set => SetField(ref field, value);
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

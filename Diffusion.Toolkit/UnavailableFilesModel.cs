using System.Collections.Generic;
using System.Collections.ObjectModel;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit;

public class ImageFileItem : BaseNotify
{
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public string Path { get; set; }
    public bool Recursive { get; set; }
    public string DisplayPath { get; set; }
    public bool IsUnavailable { get; set; }
}

public class UnavailableFilesModel : BaseNotify
{
    private bool _useRootFolders;
    private bool _useSpecificFolders;
    private ObservableCollection<ImageFileItem> _imagePaths;
    private int _selectedIndex;
    private bool _showUnavailableRootFolders;
    private bool _removeImmediately;
    private bool _markForDeletion;
    private bool _isStartEnabled;
    private bool _justUpdate;

    public bool UseRootFolders
    {
        get => _useRootFolders;
        set => SetField(ref _useRootFolders, value);
    }

    public bool UseSpecificFolders
    {
        get => _useSpecificFolders;
        set => SetField(ref _useSpecificFolders, value);
    }

    public ObservableCollection<ImageFileItem> ImagePaths
    {
        get => _imagePaths;
        set => SetField(ref _imagePaths, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetField(ref _selectedIndex, value);
    }

    public bool ShowUnavailableRootFolders
    {
        get => _showUnavailableRootFolders;
        set => SetField(ref _showUnavailableRootFolders, value);
    }

    public bool JustUpdate
    {
        get => _justUpdate;
        set => SetField(ref _justUpdate, value);
    }

    public bool RemoveImmediately
    {
        get => _removeImmediately;
        set => SetField(ref _removeImmediately, value);
    }

    public bool MarkForDeletion
    {
        get => _markForDeletion;
        set => SetField(ref _markForDeletion, value);
    }

    public bool IsStartEnabled
    {
        get => _isStartEnabled;
        set => SetField(ref _isStartEnabled, value);
    }
}
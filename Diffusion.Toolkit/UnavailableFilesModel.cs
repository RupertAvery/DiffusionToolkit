using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    public string DisplayPath { get; set; }
    public bool IsUnavailable { get; set; }
}

public class UnavailableFilesModel : BaseNotify
{
    private bool _useRootFolders;
    private bool _useSpecificFolders;
    private ObservableCollection<ImageFileItem> _imagePaths;
    private int _selectedIndex;
    private bool _removeFromUnavailableRootFolders;
    private bool _deleteImmediately;
    private bool _markForDeletion;
    private bool _isStartEnabled;

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

    public bool RemoveFromUnavailableRootFolders
    {
        get => _removeFromUnavailableRootFolders;
        set => SetField(ref _removeFromUnavailableRootFolders, value);
    }

    public bool DeleteImmediately
    {
        get => _deleteImmediately;
        set => SetField(ref _deleteImmediately, value);
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
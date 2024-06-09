using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using DiffusionToolkit.AvaloniaApp.ViewModels;
using ReactiveUI;

namespace DiffusionToolkit.AvaloniaApp.Common;

public class Settings : ViewModelBase
{
    private string _sortBy = "Date Created";
    private string _sortOrder = "Z-A";
    private int _iconSize = 256;
    private bool _recurseFolders = true;
    private int _pageSize = 250;
    private bool _hideNsfw = true;
    private ObservableCollection<string> _includedFolders;
    private ObservableCollection<string> _excludedFolders;
    private bool _hideDeleted = false;
    private GridSettings _mainGrid;
    private GridSettings _imageGrid;
    private string? _theme = "Default";

    public ObservableCollection<string> IncludedFolders
    {
        get => _includedFolders;
        set => this.RaiseAndSetIfChanged(ref _includedFolders, value);
    }

    public ObservableCollection<string> ExcludedFolders
    {
        get => _excludedFolders;
        set => this.RaiseAndSetIfChanged(ref _excludedFolders, value);
    }

    public string SortBy
    {
        get => _sortBy;
        set => this.RaiseAndSetIfChanged(ref _sortBy, value);
    }

    public string SortOrder
    {
        get => _sortOrder;
        set => this.RaiseAndSetIfChanged(ref _sortOrder, value);
    }

    public int IconSize
    {
        get => _iconSize;
        set => this.RaiseAndSetIfChanged(ref _iconSize, value);
    }

    public bool HideNSFW
    {
        get => _hideNsfw;
        set => this.RaiseAndSetIfChanged(ref _hideNsfw, value);
    }

    public bool HideDeleted
    {
        get => _hideDeleted;
        set => this.RaiseAndSetIfChanged(ref _hideDeleted, value);
    }


    public bool RecurseFolders
    {
        get => _recurseFolders;
        set => this.RaiseAndSetIfChanged(ref _recurseFolders, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => this.RaiseAndSetIfChanged(ref _pageSize, value);
    }

    public GridSettings MainGrid
    {
        get => _mainGrid;
        set => this.RaiseAndSetIfChanged(ref _mainGrid, value);
    }

    public GridSettings ImageGrid
    {
        get => _imageGrid;
        set => this.RaiseAndSetIfChanged(ref _imageGrid, value);
    }


    public WindowPosition? Preview { get; set; }

    [JsonIgnore]
    public bool IsDirty { get; private set; }

    public string? Theme
    {
        get => _theme;
        set => this.RaiseAndSetIfChanged(ref _theme, value);
    }

    public Settings()
    {
        IncludedFolders = new ObservableCollection<string>();
        ExcludedFolders = new ObservableCollection<string>();
        PropertyChanged += OnPropertyChanged;
        IncludedFolders.CollectionChanged += IncludedFoldersOnCollectionChanged;
        ExcludedFolders.CollectionChanged += ExcludedFoldersOnCollectionChanged;
        MainGrid = new GridSettings();
        ImageGrid = new GridSettings();
    }

    public void SetPristine()
    {
        IsDirty = true;
    }

    private void ExcludedFoldersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsDirty = true;
    }

    private void IncludedFoldersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsDirty = true;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        IsDirty = true;
    }

}


public static class GridLengthExtensions
{
    public static GridLength ToGridLength(this GridLengthSetting setting)
    {
        return GridLength.Parse(setting.Value);
    }

    public static GridLengthSetting ToSetting(this GridLength gridLength)
    {
        return new GridLengthSetting() { Value = gridLength.ToString() };
    }
}

public class GridLengthSetting
{
    public string Value { get; set; }
}

public class GridSettings
{
    public List<GridLengthSetting> GridLengths { get; set; }

    public GridSettings()
    {
        GridLengths = new List<GridLengthSetting>();
    }
}

public class WindowPosition
{
    public WindowState WindowState { get; set; }
    public Size ClientSize { get; set; }
    public PixelPoint Position { get; set; }
    public Size MaxClientSize { get; set; }
    public PixelPoint MaxPosition { get; set; }
}
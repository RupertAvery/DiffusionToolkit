using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Services;
using DiffusionToolkit.AvaloniaApp.ViewModels;
using ReactiveUI;

namespace DiffusionToolkit.AvaloniaApp.Pages.Settings;

public class SettingsPageViewModel : ViewModelBase
{
    private IEnumerable<string> _menuItems;
    private string _selectedMenuItem;
    private ObservableCollection<string> _includedFolders;
    private ObservableCollection<string> _excludedFolders;
    private string _selectedIncludedFolder;
    private string _selectedExcludedFolder;
    private bool _recurseFolders;
    private string _theme;

    public IEnumerable<string> MenuItems
    {
        get => _menuItems;
        set => this.RaiseAndSetIfChanged(ref _menuItems, value);
    }

    public string SelectedMenuItem
    {
        get => _selectedMenuItem;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
    }

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

    public string SelectedIncludedFolder
    {
        get => _selectedIncludedFolder;
        set => this.RaiseAndSetIfChanged(ref _selectedIncludedFolder, value);
    }

    public string SelectedExcludedFolder
    {
        get => _selectedExcludedFolder;
        set => this.RaiseAndSetIfChanged(ref _selectedExcludedFolder, value);
    }

    public ICommand AddIncludedFolderCommand { get; set; }
    public ICommand RemoveIncludedFolderCommand { get; set; }
    public ICommand AddExcludedFolderCommand { get; set; }
    public ICommand RemoveExcludedFolderCommand { get; set; }

    public Func<Task<string>> SelectFolderDelegate { get; set; }

    public bool IsRescanRequired { get; private set; }

    public bool RecurseFolders
    {
        get => _recurseFolders;
        set => this.RaiseAndSetIfChanged(ref _recurseFolders, value);
    }

    public string Theme
    {
        get => _theme;
        set => this.RaiseAndSetIfChanged(ref _theme, value);
    }

    public SettingsPageViewModel()
    {

        MenuItems = new List<string>()
        {
            "General",
            "Theme"
        };

        SelectedMenuItem = "General";

        AddIncludedFolderCommand = ReactiveCommand.Create(AddIncludedFolder);
        RemoveIncludedFolderCommand = ReactiveCommand.Create(RemoveIncludedFolder);
        AddExcludedFolderCommand = ReactiveCommand.Create(AddExcludedFolder);
        RemoveExcludedFolderCommand = ReactiveCommand.Create(RemoveExcludedFolder);

        IncludedFolders = new ObservableCollection<string>();
        ExcludedFolders = new ObservableCollection<string>();


        Themes = new List<string>()
        {
            "Default",
            "Light",
            "Dark",
        };

        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Theme))
        {
            var app = ((App)Application.Current);

            app.RequestedThemeVariant = Theme switch
            {
                "Dark" => ThemeVariant.Dark,
                "Light" => ThemeVariant.Light,
                "Default" => ThemeVariant.Default,
                _ => app.RequestedThemeVariant
            };

            ServiceLocator.Settings.Theme = Theme;
        }

    }

    private List<string> Themes { get; }

    private async void AddIncludedFolder()
    {
        var folder = await SelectFolderDelegate();
        if (folder != null)
        {
            IncludedFolders.Add(folder);
            IsRescanRequired = true;
        }
    }

    private void RemoveIncludedFolder()
    {
        IncludedFolders.Remove(SelectedIncludedFolder);
        IsRescanRequired = true;
    }

    private async void AddExcludedFolder()
    {
        var folder = await SelectFolderDelegate();
        if (folder != null)
        {
            ExcludedFolders.Add(folder);
            IsRescanRequired = true;
        }
    }

    private void RemoveExcludedFolder()
    {
        ExcludedFolders.Remove(SelectedExcludedFolder);
        IsRescanRequired = true;
    }

    public void LoadSettings(Common.Settings settings)
    {
        this.ExcludedFolders = settings.ExcludedFolders;
        this.IncludedFolders = settings.IncludedFolders;
        this.RecurseFolders = settings.RecurseFolders;
        this.Theme = settings.Theme ?? "Default";
        //this.HideNSFW = settings.HideNSFW;
    }

}
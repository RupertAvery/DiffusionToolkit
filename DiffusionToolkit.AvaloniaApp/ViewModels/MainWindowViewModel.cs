using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Pages.Search;
using DiffusionToolkit.AvaloniaApp.Pages.Settings;
using ReactiveUI;

namespace DiffusionToolkit.AvaloniaApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private INavigationTarget? _currentPage;
    private readonly NavigationManager _navigationManager;
    private NavigationMenuItem _currentMenuItem;
    private bool _isBusy;
    private int _currentProgress;
    private int _totalProgress;
    private string _progressStatus;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public int CurrentProgress
    {
        get => _currentProgress;
        set => this.RaiseAndSetIfChanged(ref _currentProgress, value);
    }

    public int TotalProgress
    {
        get => _totalProgress;
        set => this.RaiseAndSetIfChanged(ref _totalProgress, value);
    }

    public INavigationTarget? CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public IEnumerable<NavigationMenuItem> NavigationMenuItems { get; set; }

    public NavigationMenuItem CurrentMenuItem
    {
        get => _currentMenuItem;
        set => this.RaiseAndSetIfChanged(ref _currentMenuItem, value);
    }

    public Settings Settings { get; set; }

    public ReactiveCommand<string, Unit> SortOrderCommand { get; set; }
    public ReactiveCommand<string, Unit> SortByCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ToggleDeletedCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ToggleNSFWCommand { get; set; }
    public ICommand ScanFoldersCommand { get; set; }
    public ICommand RebuildDatabaseCommand { get; set; }
    public ICommand StopCommand { get; set; }

    public string ProgressStatus
    {
        get => _progressStatus;
        set => this.RaiseAndSetIfChanged(ref _progressStatus, value);
    }

    public ReactiveCommand<string, Unit> SetIconSizeCommand { get; set; }

    public MainWindowViewModel()
    {
        _navigationManager = ServiceLocator.NavigationManager;

        _navigationManager.AddNavigationTarget("Search", new SearchPage());
        _navigationManager.AddNavigationTarget("Settings", new SettingsPage());
        _navigationManager.Navigate += OnNavigate;

        _navigationManager.Goto("Search");

        NavigationMenuItems = new List<NavigationMenuItem>()
        {
            new NavigationMenuItem()
            {
                Key = "Search",
                Name = "Search",
                Icon = StreamGeometry.Parse("M11.5,2.75 C16.3324916,2.75 20.25,6.66750844 20.25,11.5 C20.25,13.6461673 19.4773285,15.6118676 18.1949905,17.1340957 L25.0303301,23.9696699 C25.3232233,24.2625631 25.3232233,24.7374369 25.0303301,25.0303301 C24.7640635,25.2965966 24.3473998,25.3208027 24.0537883,25.1029482 L23.9696699,25.0303301 L17.1340957,18.1949905 C15.6118676,19.4773285 13.6461673,20.25 11.5,20.25 C6.66750844,20.25 2.75,16.3324916 2.75,11.5 C2.75,6.66750844 6.66750844,2.75 11.5,2.75 Z M11.5,4.25 C7.49593556,4.25 4.25,7.49593556 4.25,11.5 C4.25,15.5040644 7.49593556,18.75 11.5,18.75 C15.5040644,18.75 18.75,15.5040644 18.75,11.5 C18.75,7.49593556 15.5040644,4.25 11.5,4.25 Z"),
                Action = () => {
                    ServiceLocator.SearchManager.SetView(SearchView.Search);
                    _navigationManager.Goto("Search");
                }
            },
            new NavigationMenuItem()
            {
                Key = "RecycleBin",
                Name = "Recycle Bin",
                Icon = StreamGeometry.Parse("M24,7.25 C27.1017853,7.25 29.629937,9.70601719 29.7458479,12.7794443 L29.75,13 L37,13 C37.6903559,13 38.25,13.5596441 38.25,14.25 C38.25,14.8972087 37.7581253,15.4295339 37.1278052,15.4935464 L37,15.5 L35.909,15.5 L34.2058308,38.0698451 C34.0385226,40.2866784 32.1910211,42 29.9678833,42 L18.0321167,42 C15.8089789,42 13.9614774,40.2866784 13.7941692,38.0698451 L12.09,15.5 L11,15.5 C10.3527913,15.5 9.8204661,15.0081253 9.75645361,14.3778052 L9.75,14.25 C9.75,13.6027913 10.2418747,13.0704661 10.8721948,13.0064536 L11,13 L18.25,13 C18.25,9.82436269 20.8243627,7.25 24,7.25 Z M33.4021054,15.5 L14.5978946,15.5 L16.2870795,37.8817009 C16.3559711,38.7945146 17.116707,39.5 18.0321167,39.5 L29.9678833,39.5 C30.883293,39.5 31.6440289,38.7945146 31.7129205,37.8817009 L33.4021054,15.5 Z M27.25,20.75 C27.8972087,20.75 28.4295339,21.2418747 28.4935464,21.8721948 L28.5,22 L28.5,33 C28.5,33.6903559 27.9403559,34.25 27.25,34.25 C26.6027913,34.25 26.0704661,33.7581253 26.0064536,33.1278052 L26,33 L26,22 C26,21.3096441 26.5596441,20.75 27.25,20.75 Z M20.75,20.75 C21.3972087,20.75 21.9295339,21.2418747 21.9935464,21.8721948 L22,22 L22,33 C22,33.6903559 21.4403559,34.25 20.75,34.25 C20.1027913,34.25 19.5704661,33.7581253 19.5064536,33.1278052 L19.5,33 L19.5,22 C19.5,21.3096441 20.0596441,20.75 20.75,20.75 Z M24,9.75 C22.2669685,9.75 20.8507541,11.1064548 20.7551448,12.8155761 L20.75,13 L27.25,13 C27.25,11.2050746 25.7949254,9.75 24,9.75 Z"),
                Action = () =>
                {
                    ServiceLocator.SearchManager.SetView(SearchView.RecycleBin);
                    _navigationManager.Goto("Search");
                }
            },
            new NavigationMenuItem()
            {
                Key = "Settings",
                Name = "Settings",
                Icon = StreamGeometry.Parse("M14 9.50006C11.5147 9.50006 9.5 11.5148 9.5 14.0001C9.5 16.4853 11.5147 18.5001 14 18.5001C15.3488 18.5001 16.559 17.9066 17.3838 16.9666C18.0787 16.1746 18.5 15.1365 18.5 14.0001C18.5 13.5401 18.431 13.0963 18.3028 12.6784C17.7382 10.8381 16.0253 9.50006 14 9.50006ZM11 14.0001C11 12.3432 12.3431 11.0001 14 11.0001C15.6569 11.0001 17 12.3432 17 14.0001C17 15.6569 15.6569 17.0001 14 17.0001C12.3431 17.0001 11 15.6569 11 14.0001Z M21.7093 22.3948L19.9818 21.6364C19.4876 21.4197 18.9071 21.4515 18.44 21.7219C17.9729 21.9924 17.675 22.4693 17.6157 23.0066L17.408 24.8855C17.3651 25.273 17.084 25.5917 16.7055 25.682C14.9263 26.1061 13.0725 26.1061 11.2933 25.682C10.9148 25.5917 10.6336 25.273 10.5908 24.8855L10.3834 23.0093C10.3225 22.4731 10.0112 21.9976 9.54452 21.7281C9.07783 21.4586 8.51117 21.4269 8.01859 21.6424L6.29071 22.4009C5.93281 22.558 5.51493 22.4718 5.24806 22.1859C4.00474 20.8536 3.07924 19.2561 2.54122 17.5137C2.42533 17.1384 2.55922 16.7307 2.8749 16.4977L4.40219 15.3703C4.83721 15.0501 5.09414 14.5415 5.09414 14.0007C5.09414 13.4598 4.83721 12.9512 4.40162 12.6306L2.87529 11.5051C2.55914 11.272 2.42513 10.8638 2.54142 10.4882C3.08038 8.74734 4.00637 7.15163 5.24971 5.82114C5.51684 5.53528 5.93492 5.44941 6.29276 5.60691L8.01296 6.36404C8.50793 6.58168 9.07696 6.54881 9.54617 6.27415C10.0133 6.00264 10.3244 5.52527 10.3844 4.98794L10.5933 3.11017C10.637 2.71803 10.9245 2.39704 11.3089 2.31138C12.19 2.11504 13.0891 2.01071 14.0131 2.00006C14.9147 2.01047 15.8128 2.11485 16.6928 2.31149C17.077 2.39734 17.3643 2.71823 17.4079 3.11017L17.617 4.98937C17.7116 5.85221 18.4387 6.50572 19.3055 6.50663C19.5385 6.507 19.769 6.45838 19.9843 6.36294L21.7048 5.60568C22.0626 5.44818 22.4807 5.53405 22.7478 5.81991C23.9912 7.1504 24.9172 8.74611 25.4561 10.487C25.5723 10.8623 25.4386 11.2703 25.1228 11.5035L23.5978 12.6297C23.1628 12.95 22.9 13.4586 22.9 13.9994C22.9 14.5403 23.1628 15.0489 23.5988 15.3698L25.1251 16.4965C25.441 16.7296 25.5748 17.1376 25.4586 17.5131C24.9198 19.2536 23.9944 20.8492 22.7517 22.1799C22.4849 22.4657 22.0671 22.5518 21.7093 22.3948ZM16.263 22.1966C16.4982 21.4685 16.9889 20.8288 17.6884 20.4238C18.5702 19.9132 19.6536 19.8547 20.5841 20.2627L21.9281 20.8526C22.791 19.8538 23.4593 18.7013 23.8981 17.4552L22.7095 16.5778L22.7086 16.5771C21.898 15.98 21.4 15.0277 21.4 13.9994C21.4 12.9719 21.8974 12.0195 22.7073 11.4227L22.7085 11.4218L23.8957 10.545C23.4567 9.2988 22.7881 8.14636 21.9248 7.1477L20.5922 7.73425L20.5899 7.73527C20.1844 7.91463 19.7472 8.00722 19.3039 8.00663C17.6715 8.00453 16.3046 6.77431 16.1261 5.15465L16.1259 5.15291L15.9635 3.69304C15.3202 3.57328 14.6677 3.50872 14.013 3.50017C13.3389 3.50891 12.6821 3.57367 12.0377 3.69328L11.8751 5.15452C11.7625 6.16272 11.1793 7.05909 10.3019 7.56986C9.41937 8.0856 8.34453 8.14844 7.40869 7.73694L6.07273 7.14893C5.20949 8.14751 4.54092 9.29983 4.10196 10.5459L5.29181 11.4233C6.11115 12.0269 6.59414 12.9837 6.59414 14.0007C6.59414 15.0173 6.11142 15.9742 5.29237 16.5776L4.10161 17.4566C4.54002 18.7044 5.2085 19.8585 6.07205 20.8587L7.41742 20.2682C8.34745 19.8613 9.41573 19.9215 10.2947 20.4292C11.174 20.937 11.7593 21.832 11.8738 22.84L11.8744 22.8445L12.0362 24.3088C13.3326 24.5638 14.6662 24.5638 15.9626 24.3088L16.1247 22.8418C16.1491 22.6217 16.1955 22.4055 16.263 22.1966Z"),
                Action = () => {
                    _navigationManager.Goto("Settings");
                }
            }
        };

        PropertyChanged += OnPropertyChanged;

        Settings = ServiceLocator.Settings;
        // TODO: loading state should be done somewhere else
        QueryBuilder.HideNSFW = ServiceLocator.Settings.HideNSFW;
        QueryBuilder.HideDeleted = ServiceLocator.Settings.HideDeleted;


        SortByCommand = ReactiveCommand.Create<string, Unit>(SortBy);
        SortOrderCommand = ReactiveCommand.Create<string, Unit>(SortOrder);

        ToggleNSFWCommand = ReactiveCommand.Create(ToggleNSFW);
        ToggleDeletedCommand = ReactiveCommand.Create(ToggleDeleted);

        ServiceLocator.ScanManager.ScanProgress += ScanManagerOnScanProgress;

        ScanFoldersCommand = ReactiveCommand.Create(ScanFolders);
        RebuildDatabaseCommand = ReactiveCommand.Create(RebuildDatabase);
        StopCommand = ReactiveCommand.Create(Stop);

        SetIconSizeCommand = ReactiveCommand.Create<string, Unit>(SetIconSize);
    }

    private Unit SetIconSize(string size)
    {
        ServiceLocator.Settings.IconSize = int.Parse(size);

        return Unit.Default;
    }

    private void Stop()
    {
        ServiceLocator.ScanManager.Cancel();
    }

    private void ScanManagerOnScanProgress(object? sender, ScanProgressEventArgs e)
    {
        ProgressStatus = e.Message.Replace("{progress}", $"{e.Progress:N0}")
            .Replace("{total}", $"{e.Total:N0}");
    }

    private void ToggleDeleted()
    {
        ServiceLocator.Settings.HideDeleted = !ServiceLocator.Settings.HideDeleted;
        QueryBuilder.HideDeleted = ServiceLocator.Settings.HideDeleted;
        ServiceLocator.SearchManager.ExecuteSearch();
    }

    private void ToggleNSFW()
    {
        ServiceLocator.Settings.HideNSFW = !ServiceLocator.Settings.HideNSFW;
        QueryBuilder.HideNSFW = ServiceLocator.Settings.HideNSFW;
        ServiceLocator.SearchManager.ExecuteSearch();
    }

    private void RebuildDatabase()
    {
        Task.Run(() =>
        {
            ServiceLocator.ScanManager.RebuildMetadata();
        });
    }

    private void ScanFolders()
    {
        Task.Run(() =>
        {
            ServiceLocator.ScanManager.ScanFolders();
        });
    }

    private Unit SortBy(string parameter)
    {
        // TODO: centralize this somewhere?
        ServiceLocator.Settings.SortBy = parameter;
        ServiceLocator.SearchManager.SetSortBy(parameter);
        return Unit.Default;
    }

    private Unit SortOrder(string parameter)
    {
        // TODO: centralize this somewhere?
        ServiceLocator.Settings.SortOrder = parameter;
        ServiceLocator.SearchManager.SetSortOrder(parameter);
        return Unit.Default;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentMenuItem))
        {
            CurrentMenuItem.Action();
        }
    }

    private void OnNavigate(object? sender, INavigationTarget e)
    {
        CurrentPage?.Deactivate();
        CurrentPage = e;
        CurrentPage.Activate();
    }

}
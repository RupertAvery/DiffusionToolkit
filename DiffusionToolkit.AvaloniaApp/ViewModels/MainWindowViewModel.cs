using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;
using DiffusionToolkit.AvaloniaApp.Pages.Search;
using ReactiveUI;

namespace DiffusionToolkit.AvaloniaApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private UserControl _currentPage;

    public UserControl CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage , value);
    }

    private Dictionary<string, UserControl> _navigation;


    public MainWindowViewModel()
    {
        _navigation = new Dictionary<string, UserControl>();
        _navigation.Add("Search", new SearchPage());

        Navigate("Search");
    }

    private void Navigate(string page)
    {
        CurrentPage = _navigation[page];
    }


}
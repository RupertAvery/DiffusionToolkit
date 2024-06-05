using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;

namespace DiffusionToolkit.AvaloniaApp.Pages.Search;

public partial class SearchPage : UserControl, INavigationTarget
{
    SearchPageViewModel _viewModel = new SearchPageViewModel();

    public SearchPage()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }


    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.Search();
            e.Handled = true;
        }
    }

    private void Thumbnail_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Shift) != 0 &&
            (e.KeyModifiers & KeyModifiers.Control) != 0 &&
            e.Key == Key.N)
        {
            _viewModel.ToggleNSFW();
            e.Handled = true;
        }

        else if (e.Key == Key.I)
        {
            _viewModel.IsMetadataVisible = !_viewModel.IsMetadataVisible;
            e.Handled = true;
        }
    }

    public void Activate()
    {
    }

    public void Deactivate()
    {
    }
}
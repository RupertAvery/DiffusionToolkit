using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

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
        var isShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        var isCtrlPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        if (isShiftPressed && isCtrlPressed && e.Key == Key.N)
        {
            _viewModel.ToggleNSFW();
            e.Handled = true;
        }

        else if (e.Key == Key.I)
        {
            _viewModel.IsMetadataVisible = !_viewModel.IsMetadataVisible;
            e.Handled = true;
        }

        else if (e.Key == Key.Enter)
        {
            ServiceLocator.PreviewManager.ShowPreview(_viewModel.SelectedEntry.Path, isShiftPressed);
        }
    }

    public void Activate()
    {
    }

    public void Deactivate()
    {
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _viewModel.Search();
    }

    private void Page_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.UpdateResults();
        }
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var isShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        ServiceLocator.PreviewManager.ShowPreview(_viewModel.SelectedEntry.Path, isShiftPressed);
    }

    private void ThumbnailControl_OnNavigationChanged(object? sender, NavigationEventArgs e)
    {
        if (e.NavigationState == NavigationState.StartOfPage)
        {
            _viewModel.GotoPrevPage();
        }
        if (e.NavigationState == NavigationState.EndOfPage)
        {
            _viewModel.GotoNextPage();
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace DiffusionToolkit.AvaloniaApp.Pages.Search;

public partial class SearchPage : UserControl
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
        }
    }
}
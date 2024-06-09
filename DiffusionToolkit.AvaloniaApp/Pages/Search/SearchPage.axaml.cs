using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;
using Key = Avalonia.Input.Key;

namespace DiffusionToolkit.AvaloniaApp.Pages.Search;

public partial class SearchPage : UserControl, INavigationTarget
{
    SearchPageViewModel _viewModel = new SearchPageViewModel();

    public SearchPage()
    {
        InitializeComponent();
        DataContext = _viewModel;

        Grid mainGrid = MainGrid;
        Grid imageGrid = ImageGrid;

        if (ServiceLocator.Settings.MainGrid.GridLengths.Any())
        {
            mainGrid.ColumnDefinitions[0].Width = ServiceLocator.Settings.MainGrid.GridLengths[0].ToGridLength();
            mainGrid.ColumnDefinitions[2].Width = ServiceLocator.Settings.MainGrid.GridLengths[1].ToGridLength();
        }

        if (ServiceLocator.Settings.ImageGrid.GridLengths.Any())
        {
            imageGrid.RowDefinitions[0].Height = ServiceLocator.Settings.ImageGrid.GridLengths[0].ToGridLength();
            imageGrid.RowDefinitions[2].Height = ServiceLocator.Settings.ImageGrid.GridLengths[1].ToGridLength();
        }

        mainGrid.ColumnDefinitions[0].PropertyChanged += OnGridChanged;
        imageGrid.RowDefinitions[0].PropertyChanged += OnGridChanged;
    }

    private void OnGridChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        Grid mainGrid = MainGrid;
        ServiceLocator.Settings.MainGrid.GridLengths = new List<GridLengthSetting>() {
            mainGrid.ColumnDefinitions[0].Width.ToSetting(), 
            mainGrid.ColumnDefinitions[2].Width.ToSetting()
        };

        Grid imageGrid = ImageGrid;
        ServiceLocator.Settings.ImageGrid.GridLengths = new List<GridLengthSetting>() {
            imageGrid.RowDefinitions[0].Height.ToSetting(), 
            imageGrid.RowDefinitions[2].Height.ToSetting()
        };
    }


    private async void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await _viewModel.SearchAsync();
            e.Handled = true;
        }
    }

    private async void Thumbnail_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var isShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        var isCtrlPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        if (isShiftPressed && isCtrlPressed && e.Key == Key.N)
        {
            _viewModel.ToggleNSFW();
            e.Handled = true;
        }

        else if (e.Key == Key.F5)
        {
            await _viewModel.UpdateResultsAsync();
            e.Handled = true;
        }

        else if (e.Key == Key.I)
        {
            _viewModel.IsMetadataVisible = !_viewModel.IsMetadataVisible;
            e.Handled = true;
        }

        else if (e.Key == Key.Delete)
        {
            if (_viewModel.SelectedItems != null)
            {
                var forDeletion = !_viewModel.SelectedItems.GroupBy(e => e.ForDeletion).OrderByDescending(g => g.Count()).First().Key;
                
                foreach (var item in _viewModel.SelectedItems)
                {
                    item.ForDeletion = forDeletion;
                }

                var ids = _viewModel.SelectedItems.Select(item => item.Id).ToList();

                ServiceLocator.DataStore!.SetDeleted(ids, forDeletion);

            }
            e.Handled = true;
        }

        else if (e.Key == Key.N)
        {
            if (_viewModel.SelectedItems != null)
            {
                var nsfw = !_viewModel.SelectedItems.GroupBy(e => e.NSFW).OrderByDescending(g => g.Count()).First().Key;

                foreach (var item in _viewModel.SelectedItems)
                {
                    item.NSFW = nsfw;
                }

                var ids = _viewModel.SelectedItems.Select(item => item.Id).ToList();

                ServiceLocator.DataStore!.SetNSFW(ids, nsfw);
            }

            e.Handled = true;
        }

        else if (e.Key == Key.F)
        {
            if (_viewModel.SelectedItems != null)
            {
                var favorite = !_viewModel.SelectedItems.GroupBy(e => e.Favorite).OrderByDescending(g => g.Count()).First().Key;

                foreach (var item in _viewModel.SelectedItems)
                {
                    item.Favorite = favorite;
                }

                var ids = _viewModel.SelectedItems.Select(item => item.Id).ToList();

                ServiceLocator.DataStore!.SetFavorite(ids, favorite);
            }

            e.Handled = true;
        }

        else if (e.Key is >= Key.D0 and <= Key.D9)
        {
            if (_viewModel.SelectedItems != null)
            {
                int? rating = e.Key - Key.D0;

                if (rating == 0) rating = 10;

                var ratings = _viewModel.SelectedItems.Select(item => item.Rating).ToList();

                // The selected ratings match the new rating, toggle them all off
                if (ratings.All(r => r == rating))
                {
                    rating = null;
                }

                foreach (var item in _viewModel.SelectedItems)
                {
                    item.Rating = rating;
                }

                var ids = _viewModel.SelectedItems.Select(item => item.Id).ToList();

                ServiceLocator.DataStore!.SetRating(ids, rating);
            }

            e.Handled = true;
        }

        else if (e.Key == Key.Enter)
        {
            if (_viewModel.SelectedEntry != null)
            {
                ServiceLocator.PreviewManager.ShowPreview(_viewModel.SelectedEntry.Path, isShiftPressed);
            }
        }
    }

    public void Activate()
    {
    }

    public void Deactivate()
    {
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        await _viewModel.SearchAsync();
    }

    private async void Page_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await _viewModel.UpdateResultsAsync();
        }
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var isShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        if (_viewModel.SelectedEntry is { })
        {
            ServiceLocator.PreviewManager.ShowPreview(_viewModel.SelectedEntry.Path, isShiftPressed);
        }
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

    private async void ThumbnailControl_OnDragStart(object? sender, DragStartEventArgs e)
    {
        if (_viewModel.SelectedItems != null)
        {
            var selectedFiles = _viewModel.SelectedItems.Select(t => t.Path).ToArray();
            DataObject dataObject = new DataObject();
            dataObject.Set(DataFormats.Files, selectedFiles);
            dataObject.Set("DTCustomDragSource", true);

            var effects = await DragDrop.DoDragDrop(e.PointerEventArgs, dataObject, DragDropEffects.Move | DragDropEffects.Copy);
        }
    }
}
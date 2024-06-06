using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Diffusion.Database;
using Diffusion.IO;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Controls.Metadata;
using DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;
using DiffusionToolkit.AvaloniaApp.ViewModels;
using ReactiveUI;

namespace DiffusionToolkit.AvaloniaApp.Pages.Search;

public class SearchPageViewModel : ViewModelBase
{
    private readonly DataStore _dataStore;
    private IList<ThumbnailViewModel> _searchResults;
    private ThumbnailViewModel? _selectedEntry;
    private Bitmap _previewImage;
    private string _previewImageSource;
    private string _searchText;
    private int _page;
    private int _pages;
    private MetadataViewModel? _metadata;
    private bool _isMetadataVisible;
    private string _sortOrder;
    private string _sortBy;

    public ThumbnailViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set => this.RaiseAndSetIfChanged(ref _selectedEntry, value);
    }

    public IList<ThumbnailViewModel> SearchResults
    {
        get => _searchResults;
        set => this.RaiseAndSetIfChanged(ref _searchResults, value);
    }

    public Bitmap PreviewImage
    {
        get => _previewImage;
        set => this.RaiseAndSetIfChanged(ref _previewImage, value);
    }

    public string PreviewImageSource
    {
        get => _previewImageSource;
        set => this.RaiseAndSetIfChanged(ref _previewImageSource, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public bool IsMetadataVisible
    {
        get => _isMetadataVisible;
        set => this.RaiseAndSetIfChanged(ref _isMetadataVisible, value);
    }

    public ICommand GotoStart { get; set; }
    public ICommand GotoPrev { get; set; }
    public ICommand GotoNext { get; set; }
    public ICommand GotoEnd { get; set; }

    public int Page
    {
        get => _page;
        set => this.RaiseAndSetIfChanged(ref _page, value);
        //set => this.RaiseAndSetIfChanged(ref _page, Math.Clamp(value, 0, Pages));
    }

    public int Pages
    {
        get => _pages;
        set => this.RaiseAndSetIfChanged(ref _pages, value);
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

    public IEnumerable<string> SortByOptions { get; set; }
    public IEnumerable<string> SortOrderOptions { get; set; }

    public SearchPageViewModel()
    {
        _dataStore = ServiceLocator.DataStore;

        PropertyChanged += OnPropertyChanged;

        GotoStart = ReactiveCommand.Create(GotoStartPage);
        GotoPrev = ReactiveCommand.Create(GotoPrevPage);
        GotoNext = ReactiveCommand.Create(GotoNextPage);
        GotoEnd = ReactiveCommand.Create(GotoEndPage);

        ServiceLocator.SearchManager.SortOrder += OnSortOrder;
        ServiceLocator.SearchManager.SortBy += OnSortBy;

        SortByOptions = DataStore.SortByOptions;
        SortOrderOptions = new List<string>() { "A-Z", "Z-A" };

        SortBy = "Date Created";
        SortOrder = "Z-A";
    }

    private void OnSortBy(object? sender, string e)
    {
        SortBy = e;
        Search();
    }

    private void OnSortOrder(object? sender, string e)
    {
        SortOrder = e;
        Search();
    }

    private void OnKeyDown(TopLevel topLevel, KeyEventArgs args)
    {
        if (args.Key == Key.I)
        {
            IsMetadataVisible = !IsMetadataVisible;
        }
    }

    public void GotoStartPage()
    {
        Page = 1;
        UpdateResults();
        Dispatcher.UIThread.Post(() =>
        {
            SelectedEntry = SearchResults[0];
        });

    }

    public void GotoPrevPage()
    {
        if (Page > 1)
        {
            Page -= 1;
            UpdateResults();
            Dispatcher.UIThread.Post(() =>
            {
                SelectedEntry = SearchResults[^1];
            });

        }
    }

    public void GotoNextPage()
    {
        if (Page < Pages)
        {
            Page += 1;
            UpdateResults();
            Dispatcher.UIThread.Post(() =>
            {
                SelectedEntry = SearchResults[0];
            });
        }
    }

    public void GotoEndPage()
    {
        Page = Pages;
        UpdateResults();
        Dispatcher.UIThread.Post(() =>
        {
            SelectedEntry = SearchResults[0];
        });
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedEntry))
        {
            if (SelectedEntry != null)
            {
                if (File.Exists(SelectedEntry.Path))
                {
                    Metadata = MetadataViewModel.FromFileParameters(Diffusion.IO.Metadata.ReadFromFile(SelectedEntry.Path)!);
                    PreviewImage = new Bitmap(SelectedEntry.Path);
                    ServiceLocator.PreviewManager.UpdatePreview(SelectedEntry.Path);
                }
            }
        }
    }

    public MetadataViewModel Metadata
    {
        get => _metadata;
        set => this.RaiseAndSetIfChanged(ref _metadata, value);
    }



    public void ToggleNSFW()
    {
        QueryBuilder.HideNSFW = !QueryBuilder.HideNSFW;
        Search();
    }

    private int PageSize => ServiceLocator.Settings!.PageSize;

    public void Search()
    {
        var pageSize = PageSize;

        var total = _dataStore.Count(SearchText);
        Pages = total / pageSize + (total % pageSize > 0 ? 1 : 0);
        Page = 1;
        UpdateResults();
    }

    public void UpdateResults()
    {
        var pageSize = PageSize;

        var results = _dataStore.Search(SearchText, pageSize, (Page - 1) * pageSize, SortBy, SortOrder);

        var images = new List<ThumbnailViewModel>();

        foreach (var image in results)
        {
            images.Add(new ThumbnailViewModel()
            {
                Source = image,
                ForDeletion = image.ForDeletion,
                Path = image.Path,
                Rating = image.Rating,
                NSFW = image.NSFW,
            });
        }

        SearchResults = images;
    }


}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
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
    private string _resultsSummary;
    private ObservableCollection<ThumbnailViewModel>? _selectedItems;

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

    public string ResultsSummary
    {
        get => _resultsSummary;
        set => this.RaiseAndSetIfChanged(ref _resultsSummary, value);
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
        ServiceLocator.SearchManager.Filter += OnSetFilter;
        ServiceLocator.SearchManager.Search += OnSearch;
        ServiceLocator.SearchManager.View += OnView;

        SelectedItems = new ObservableCollection<ThumbnailViewModel>();

        SortByOptions = DataStore.SortByOptions;
        SortOrderOptions = new List<string>() { "A-Z", "Z-A" };
    }

    private SearchView _view = SearchView.Search;

    private void OnView(object? sender, SearchView e)
    {
        _view = e;
        SearchAsync();
    }

    public Task SearchAsync()
    {
        return Task.Run(() =>
        {
            Search();
        });
    }

    private void OnSearch(object? sender, EventArgs e)
    {
        SearchAsync();
    }

    private void OnSortBy(object? sender, string e)
    {
        SearchAsync();
    }

    private void OnSortOrder(object? sender, string e)
    {
        SearchAsync();
    }

    public void OnSetFilter(object? sender, SearchFilter filter)
    {
        SearchText = filter.Query;
        SearchAsync();
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
        Task.Run(() =>
        {
            Search();
        });
    }

    private int PageSize => ServiceLocator.Settings!.PageSize;

    private void Search()
    {
        _filter = FilterBuilder.ParseFilter(SearchText);

        if (_view == SearchView.RecycleBin)
        {
            _filter.ForDeletion = true;
            _filter.UseForDeletion = true;
        }

        var pageSize = PageSize;

        var totals = _dataStore.CountTotals(_filter);

        Pages = totals.Count / pageSize + (totals.Count % pageSize > 0 ? 1 : 0);
        TotalSize = totals.TotalSize;

        float fsize = totals.TotalSize;

        var ssize = $"{fsize:n} B";

        if (fsize > 1073741824)
        {
            fsize /= 1073741824;
            ssize = $"{fsize:n2} GiB";
        }
        else if (fsize > 1048576)
        {
            fsize /= 1048576;
            ssize = $"{fsize:n2} MiB";
        }
        else if (fsize > 1024)
        {
            fsize /= 1024;
            ssize = $"{fsize:n2} KiB";
        }

        //var text = ServiceLocator.Localization.GetLocalizedText("Search.Results");

        var text = "{count} results found ({size})".Replace("{count}", $"{totals.Count:n0}")
            .Replace("{size}", $"{ssize}");

        ResultsSummary = text;

        Page = 1;
        UpdateResults();
    }

    public long TotalSize { get; set; }

    public string SortBy => ServiceLocator.Settings.SortBy;
    public string SortOrder => ServiceLocator.Settings.SortOrder;

    public ObservableCollection<ThumbnailViewModel>? SelectedItems
    {
        get => _selectedItems;
        set => this.RaiseAndSetIfChanged(ref _selectedItems, value);
    }

    public async Task UpdateResultsAsync()
    {
        await Task.Run(() => UpdateResults());
    }

    private Filter _filter = new Filter();

    private void UpdateResults()
    {
        var pageSize = PageSize;

        var results = _dataStore.Search(_filter, pageSize, (Page - 1) * pageSize, SortBy, SortOrder);

        var images = new List<ThumbnailViewModel>();

        foreach (var image in results)
        {
            var thumb = new ThumbnailViewModel()
            {
                Id = image.Id,
                Source = image,
                Path = image.Path,
                Rating = image.Rating,
                NSFW = image.NSFW,
                Favorite = image.Favorite,
                ForDeletion = image.ForDeletion
            };


            images.Add(thumb);


            //thumb.PropertyChanged += ThumbOnPropertyChanged;

        }

        SearchResults = images;
    }

    //private void ThumbOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    //{
    //    if (e.PropertyName == nameof(ThumbnailViewModel.IsSelected))
    //    {
    //        var x = 1;
    //    }
    //}
}
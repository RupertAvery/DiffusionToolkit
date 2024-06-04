using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;
using ReactiveUI;

namespace DiffusionToolkit.AvaloniaApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DataStore _dataStore;
    private IList<ThumbnailViewModel> _searchResults;
    private ThumbnailViewModel _selectedEntry;
    private Bitmap _previewImage;
    private string _previewImageSource;
    private string _searchText;
    private int _page;
    private int _pages;

    private ThumbnailViewModel SelectedEntry
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

    public ICommand GotoStart { get; set; }
    public ICommand GotoPrev { get; set; }
    public ICommand GotoNext { get; set; }
    public ICommand GotoEnd { get; set; }

    public int Page
    {
        get => _page;
        set => this.RaiseAndSetIfChanged(ref _page, value);
    }

    public int Pages
    {
        get => _pages;
        set => this.RaiseAndSetIfChanged(ref _pages, value);
    }

    public CancellationToken ThumbnailCancellationToken
    {
        get => _thumbnailCancellationToken;
        set => this.RaiseAndSetIfChanged(ref _thumbnailCancellationToken, value);
    }


    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(DataStore dataStore)
    {
        _dataStore = dataStore;

        PropertyChanged += OnPropertyChanged;

        GotoStart = ReactiveCommand.Create(GotoStartPage);
        GotoPrev = ReactiveCommand.Create(GotoPrevPage);
        GotoNext = ReactiveCommand.Create(GotoNextPage);
        GotoEnd = ReactiveCommand.Create(GotoEndPage);

        Search();
    }

    private void GotoStartPage()
    {
        Page = 1;
        UpdateResults();
    }

    private void GotoPrevPage()
    {
        if (Page > 1)
        {
            Page -= 1;
            UpdateResults();
        }
    }

    private void GotoNextPage()
    {
        if (Page < Pages)
        {
            Page += 1;
            UpdateResults();
        }
    }

    private void GotoEndPage()
    {
        Page = Pages;
        UpdateResults();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedEntry))
        {
            if (SelectedEntry != null)
            {
                Metadata = ((ImageView)SelectedEntry.Source);
                //PreviewImageSource = "D:/asuka/205443800-3203535900-(german_1.2) girl Asuka Langley Sohryu from neon genesis evangelion, wearing a red full-body plug suit, blue eyes, pigtails, lit.png";
                PreviewImage = new Bitmap(SelectedEntry.Path);
                //PreviewImageSource = SelectedEntry.Path;
            }
        }
    }

    public ImageView Metadata
    {
        get => _metadata;
        set => this.RaiseAndSetIfChanged(ref _metadata, value);
    }

    private int pageSize = 250;
    private int lastPage = 0;
    private CancellationToken _thumbnailCancellationToken;
    private ImageView _metadata;

    public void Search()
    {
        var total = _dataStore.Count(SearchText);
        Page = 1;
        Pages = total / pageSize + (total % pageSize > 0 ? 1 : 0);
        UpdateResults();
    }

    public void UpdateResults()
    {
        var results = _dataStore.Search(SearchText, pageSize, (Page - 1) * pageSize, "Date Created", "DESC");

        var images = new List<ThumbnailViewModel>();

        foreach (var image in results)
        {
            images.Add(new ThumbnailViewModel()
            {
                Source = image,
                Path = image.Path,
            });
        }

        SearchResults = images;
    }


}
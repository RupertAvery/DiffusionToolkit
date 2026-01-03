using System.Collections.ObjectModel;
using System.Windows.Input;
using Diffusion.Database;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls;

public class ThumbnailPaneModel : BaseNotify
{
    private ObservableCollection<ImageEntry>? _images;

    //public object _rowLock = new object();

    private string _searchText;

    private int _imageCount;
    private bool _isEmpty;
    private int _pages;
    private string _searchHint;
    private ImageViewModel? _currentImage;
    private float _imageOpacity;
    private ObservableCollection<string?> _searchHistory;

    private ICommand _prevPage;
    private ICommand _nextPage;
    private ICommand _firstPage;
    private ICommand _lastPage;
    private bool _nextEnabled;
    private bool _prevPageEnabled;
    private bool _firstPageEnabled;
    private bool _lastPageEnabled;
    private bool _fitToPreview;

    public ThumbnailPaneModel()
    {
        _images = new ObservableCollection<ImageEntry>();
        _searchHistory = new ObservableCollection<string>();
        _currentImage = new ImageViewModel();
        _imageOpacity = 1;
        _isEmpty = true;
        //_resultStatus = "Type anything to begin";
        _searchHint = "Search for the answer to the the question of life, the universe, and everything";
    }

    public DataStore DataStore
    {
        get;
        set;
    }


    public ObservableCollection<ImageEntry>? Images
    {
        get => _images;
        set => SetField(ref _images, value);
    }

    public ImageViewModel? CurrentImage
    {
        get => _currentImage;
        set => SetField(ref _currentImage, value);
    }


    public int CurrentPosition
    {
        get;
        set => SetField(ref field, value);
    }

    public int TotalFiles
    {
        get;
        set => SetField(ref field, value);
    }

    public string? SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public ObservableCollection<string?> SearchHistory
    {
        get => _searchHistory;
        set
        {
            SetField(ref _searchHistory, value);
            OnPropertyChanged("ReverseSearchHistory");
        }
    }

    public ICommand ClearCommand
    {
        get;
        set => SetField(ref field, value);
    }


    public ICommand SearchCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public int Page
    {
        get;
        set
        {
            if (value > _pages)
            {
                value = _pages;
            }

            if (_pages == 0)
            {
                value = 0;
            }
            else if (value < 1)
            {
                value = 1;
            }

            SetField(ref field, value);
        }
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetField(ref _isEmpty, value);
    }

    public int Pages
    {
        get => _pages;
        set => SetField(ref _pages, value);
    }

    public string Results
    {
        get;
        set => SetField(ref field, value);
    }

    public string ResultStatus
    {
        get;
        set => SetField(ref field, value);
    }

    public string SearchHint
    {
        get => _searchHint;
        set => SetField(ref _searchHint, value);
    }

    public float ImageOpacity
    {
        get => _imageOpacity;
        set => SetField(ref _imageOpacity, value);
    }

    public bool HideIcons
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand Refresh
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand FocusSearch
    {
        get;
        set => SetField(ref field, value);
    }

    public string ModeName
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ShowDropDown
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand HideDropDown
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ToggleParameters
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CopyFiles
    {
        get;
        set => SetField(ref field, value);
    }


    public bool NSFWBlur
    {
        get;
        set => SetField(ref field, value);
    }


    public ImageEntry? SelectedImageEntry
    {
        get;
        set => SetField(ref field, value);
    }
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Diffusion.Toolkit.Models;

public class SearchModel : BaseNotify
{
    private ObservableCollection<ImageEntry>? _images;
    private ImageEntry? _selectedImage;
    //public object _rowLock = new object();
    private int _totalFiles;
    private int _currentPosition;

    private ICommand _searchCommand;
    private string _searchText;

    private int _imageCount;
    private int _page;
    private bool _isEmpty;
    private int _pages;
    private string _results;
    private string _resultStatus;
    private string _searchHint;
    private ImageViewModel? _currentImage;
    private float _imageOpacity;
    private bool _hideIcons;
    private ObservableCollection<string?> _searchHistory;

    private ICommand _prevPage;
    private ICommand _nextPage;
    private ICommand _firstPage;
    private ICommand _lastPage;
    private ICommand _refresh;
    private ICommand _focusSearch;
    private bool _nextEnabled;
    private bool _prevPageEnabled;
    private bool _firstPageEnabled;
    private bool _lastPageEnabled;
    private string _modeName;

    public SearchModel()
    {
        _images = new ObservableCollection<ImageEntry>();
        _searchHistory = new ObservableCollection<string>();
        _currentImage = new ImageViewModel();
        _imageOpacity = 1;
        _isEmpty = true;
        _resultStatus = "Type anything to begin";
        _searchHint = "Search for the answer to the the question of life, the universe, and everything";
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


    public ImageEntry? SelectedImageEntry
    {
        get => _selectedImage;
        set => SetField(ref _selectedImage, value);
    }


    public int CurrentPosition
    {
        get => _currentPosition;
        set => SetField(ref _currentPosition, value);
    }

    public int TotalFiles
    {
        get => _totalFiles;
        set => SetField(ref _totalFiles, value);
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

    public ICommand SearchCommand
    {
        get => _searchCommand;
        set => SetField(ref _searchCommand, value);
    }

    public bool NextPageEnabled
    {
        get => _nextEnabled;
        private set => SetField(ref _nextEnabled, value);
    }

    public bool PrevPageEnabled
    {
        get => _prevPageEnabled;
        private set => SetField(ref _prevPageEnabled, value);
    }

    public bool FirstPageEnabled
    {
        get => _firstPageEnabled;
        private set => SetField(ref _firstPageEnabled, value);
    }

    public bool LastPageEnabled
    {
        get => _lastPageEnabled;
        private set => SetField(ref _lastPageEnabled, value);
    }


    public int Page
    {
        get => _page;
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

            PrevPageEnabled = value > 1;
            NextPageEnabled = value < _pages;
            FirstPageEnabled = value > 1;
            LastPageEnabled = value < _pages;

            SetField(ref _page, value);
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
        get => _results;
        set => SetField(ref _results, value);
    }

    public string ResultStatus
    {
        get => _resultStatus;
        set => SetField(ref _resultStatus, value);
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
        get => _hideIcons;
        set => SetField(ref _hideIcons, value);
    }

    public ICommand PrevPage
    {
        get => _prevPage;
        set => SetField(ref _prevPage, value);
    }

    public ICommand NextPage
    {
        get => _nextPage;
        set => SetField(ref _nextPage, value);
    }

    public ICommand FirstPage
    {
        get => _firstPage;
        set => SetField(ref _firstPage, value);
    }

    public ICommand LastPage
    {
        get => _lastPage;
        set => SetField(ref _lastPage, value);
    }

    public ICommand Refresh
    {
        get => _refresh;
        set => SetField(ref _refresh, value);
    }

    public ICommand FocusSearch
    {
        get => _focusSearch;
        set => SetField(ref _focusSearch, value);
    }

    public string ModeName
    {
        get => _modeName;
        set => SetField(ref _modeName, value);
    }
}
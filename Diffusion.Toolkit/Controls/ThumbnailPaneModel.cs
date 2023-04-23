using System.Collections.ObjectModel;
using System.Windows.Input;
using Diffusion.Database;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls;

public class ThumbnailPaneModel : BaseNotify
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
    private ICommand _showDropDown;
    private ICommand _hideDropDown;
    private ICommand _toggleParameters;
    private ICommand _copyFiles;
    private bool _nsfwBlur;
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

    public ICommand ShowDropDown
    {
        get => _showDropDown;
        set => SetField(ref _showDropDown, value);
    }

    public ICommand HideDropDown
    {
        get => _hideDropDown;
        set => SetField(ref _hideDropDown, value);
    }

    public ICommand ToggleParameters
    {
        get => _toggleParameters;
        set => SetField(ref _toggleParameters, value);
    }

    public ICommand CopyFiles
    {
        get => _copyFiles;
        set => SetField(ref _copyFiles, value);
    }


    public bool NSFWBlur
    {
        get => _nsfwBlur;
        set => SetField(ref _nsfwBlur, value);
    }


    public ImageEntry? SelectedImageEntry
    {
        get => _selectedImage;
        set => SetField(ref _selectedImage, value);
    }
}
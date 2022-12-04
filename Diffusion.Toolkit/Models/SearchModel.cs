using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Diffusion.Toolkit.Models;

public class SearchModel : BaseNotify
{
    private ObservableCollection<ImageEntry>? _images;
    private ImageEntry? _selectedImage;
    //public object _rowLock = new object();
    private int _totalFiles;
    private int _currentPosition;
    private int _totalFilesScan;
    private int _currentPositionScan;
    private ICommand _searchCommand;
    private string _searchText;

    private int _imageCount;
    private string _status;
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


    public SearchModel()
    {
        _images = new ObservableCollection<ImageEntry>();
        _searchHistory = new ObservableCollection<string>();
        _currentImage = new ImageViewModel();
        _imageOpacity = 1;
        _isEmpty = true;
        _status = "Ready";
        _resultStatus = "Type anything to begin";
        _searchHint = "Search for the answer to the the question of life, the universe, and everything";
        //BindingOperations.EnableCollectionSynchronization(Images, _rowLock);
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


    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public int ImageCount
    {
        get => _imageCount;
        set => SetField(ref _imageCount, value);
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

    public int CurrentPositionScan
    {
        get => _currentPositionScan;
        set => SetField(ref _currentPositionScan, value);
    }

    public int TotalFilesScan
    {
        get => _totalFilesScan;
        set => SetField(ref _totalFilesScan, value);
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

    public IEnumerable<string?> ReverseSearchHistory
    {
        get => _searchHistory.Reverse();
    }

    public ICommand SearchCommand
    {
        get => _searchCommand;
        set => SetField(ref _searchCommand, value);
    }
    
    public int Page
    {
        get => _page;
        set => SetField(ref _page, value);
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

}
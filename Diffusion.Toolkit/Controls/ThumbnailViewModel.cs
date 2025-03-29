using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls;

public class ThumbnailViewModel : BaseNotify
{
    private ObservableCollection<ImageEntry>? _images;
    private ImageEntry? _selectedImage;

    private int _totalFiles;
    private int _currentPosition;

    private int _imageCount;
    private int _page;
    private bool _isEmpty;
    private bool _isBusy;
    private int _pages;
    private string _results;
    private string _resultStatus;
    private string _searchHint;
    private ImageViewModel? _currentImage;
    private float _imageOpacity;
    private bool _hideIcons;

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

    private ICommand _copyOthersCommand;
    private ICommand _copyNegativePromptCommand;
    private ICommand _copyPathCommand;
    private ICommand _copyPromptCommand;
    private ICommand _copyParametersCommand;
    private ICommand _showInExplorerCommand;
    private ICommand _showInThumbnails;
    private ICommand _deleteCommand;
    private ICommand _favoriteCommand;
    private long _fileSize;
    private ICommand _copySeedCommand;
    private ICommand _copyHashCommand;
    private ICommand _ratingCommand;
    private ICommand _nsfwCommand;
    private ICommand _removeEntryCommand;
    private ICommand _moveCommand;
    private ICommand _copyCommand;
    private int _thumbnailSize;
    private ObservableCollection<Control> _albumMenuItems;
    private ViewMode _viewMode;
    private int _pageSize;
    private ICommand _expandToFolderCommand;
    private ObservableCollection<Control> _selectionAlbumMenuItems;
    private ObservableCollection<Control> _openWithMenuItems;

    public ThumbnailViewModel()
    {
        _images = new ObservableCollection<ImageEntry>();
        _currentImage = new ImageViewModel();
        _imageOpacity = 1;
        _isEmpty = true;
        //_resultStatus = "Type anything to begin";
        _thumbnailSize = 128;
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

    public bool IsImage => SelectedImageEntry != null && SelectedImageEntry.EntryType == EntryType.File;


    public ImageEntry? SelectedImageEntry
    {
        get => _selectedImage;
        set
        {
            SetField(ref _selectedImage, value);
            OnPropertyChanged(nameof(IsImage));
        }
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

            SetPagingEnabled(value);

            SetField(ref _page, value);
        }
    }

    public void SetPagingEnabled(int value)
    {

        PrevPageEnabled = value > 1;
        NextPageEnabled = value < _pages;
        FirstPageEnabled = value > 1;
        LastPageEnabled = value < _pages;
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetField(ref _isEmpty, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }


    public int Pages
    {
        get => _pages;
        set => SetField(ref _pages, value);
    }

    public string Results
    {
        get => _results;
        set
        {
            SetField(ref _results, value);
        }
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

    public ICommand CopyPromptCommand
    {
        get => _copyPromptCommand;
        set => SetField(ref _copyPromptCommand, value);
    }

    public ICommand CopyPathCommand
    {
        get => _copyPathCommand;
        set => SetField(ref _copyPathCommand, value);
    }

    public ICommand ShowInExplorerCommand
    {
        get => _showInExplorerCommand;
        set => SetField(ref _showInExplorerCommand, value);
    }

    public ICommand ExpandToFolderCommand
    {
        get => _expandToFolderCommand;
        set => SetField(ref _expandToFolderCommand, value);
    }

    public ICommand DeleteCommand
    {
        get => _deleteCommand;
        set => SetField(ref _deleteCommand, value);
    }

    public ICommand FavoriteCommand
    {
        get => _favoriteCommand;
        set => SetField(ref _favoriteCommand, value);
    }


    public ICommand CopyNegativePromptCommand
    {
        get => _copyNegativePromptCommand;
        set => SetField(ref _copyNegativePromptCommand, value);
    }


    public ICommand CopyOthersCommand
    {
        get => _copyOthersCommand;
        set => SetField(ref _copyOthersCommand, value);
    }


    public ICommand CopyParametersCommand
    {
        get => _copyParametersCommand;
        set => SetField(ref _copyParametersCommand, value);
    }

    public long FileSize
    {
        get => _fileSize;
        set => SetField(ref _fileSize, value);
    }

    public ICommand CopySeedCommand
    {
        get => _copySeedCommand;
        set => SetField(ref _copySeedCommand, value);
    }

    public ICommand CopyHashCommand
    {
        get => _copyHashCommand;
        set => SetField(ref _copyHashCommand, value);
    }

    public ICommand RatingCommand
    {
        get => _ratingCommand;
        set => SetField(ref _ratingCommand, value);
    }

    public ICommand NSFWCommand
    {
        get => _nsfwCommand;
        set => SetField(ref _nsfwCommand, value);
    }

    public ICommand RemoveEntryCommand
    {
        get => _removeEntryCommand;
        set => SetField(ref _removeEntryCommand, value);
    }

    public ICommand CopyCommand
    {
        get => _copyCommand;
        set => SetField(ref _copyCommand, value);
    }

    public ICommand MoveCommand
    {
        get => _moveCommand;
        set => SetField(ref _moveCommand, value);
    }

    public int ThumbnailSize
    {
        get => _thumbnailSize;
        set => SetField(ref _thumbnailSize, value);
    }

    public ViewMode ViewMode
    {
        get => _viewMode;
        set => SetField(ref _viewMode, value);
    }

    public ObservableCollection<Control> AlbumMenuItems
    {
        get => _albumMenuItems;
        set => SetField(ref _albumMenuItems, value);
    }

    public ObservableCollection<Control> SelectionAlbumMenuItems
    {
        get => _selectionAlbumMenuItems;
        set => SetField(ref _selectionAlbumMenuItems, value);
    }

    public ObservableCollection<Control> OpenWithMenuItems
    {
        get => _openWithMenuItems;
        set => SetField(ref _openWithMenuItems, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => SetField(ref _pageSize, value);
    }

    public ICommand RescanCommand { get; set; }

}
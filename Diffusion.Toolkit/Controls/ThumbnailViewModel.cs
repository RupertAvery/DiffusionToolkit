using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Controls;

public class ThumbnailViewModel : BaseNotify
{
    private ObservableCollection<ImageEntry>? _images;

    private int _imageCount;
    private bool _isEmpty;
    private int _pages;
    private ImageViewModel? _currentImage;
    private float _imageOpacity;

    private string _modeName;
    private ICommand _showDropDown;
    private ICommand _hideDropDown;

    private ICommand _showInThumbnails;
    private int _thumbnailSize;

    public ThumbnailViewModel()
    {
        _images = new ObservableCollection<ImageEntry>();
        _currentImage = new ImageViewModel();
        _imageOpacity = 1;
        _isEmpty = true;
        //_resultStatus = "Type anything to begin";
        _thumbnailSize = 128;
    }

    public MainModel MainModel => ServiceLocator.MainModel;

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
        get;
        set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(IsImage));
        }
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

    public bool NextPageEnabled
    {
        get;
        private set => SetField(ref field, value);
    }

    public bool PrevPageEnabled
    {
        get;
        private set => SetField(ref field, value);
    }

    public bool FirstPageEnabled
    {
        get;
        private set => SetField(ref field, value);
    }

    public bool LastPageEnabled
    {
        get;
        private set => SetField(ref field, value);
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

            SetPagingEnabled(value);

            SetField(ref field, value);
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
        get;
        set => SetField(ref field, value);
    }


    public int Pages
    {
        get => _pages;
        set => SetField(ref _pages, value);
    }

    public string Results
    {
        get;
        set { SetField(ref field, value); }
    }

    public string ResultStatus
    {
        get;
        set => SetField(ref field, value);
    }

    public string SearchHint
    {
        get;
        set => SetField(ref field, value);
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

    public ICommand PrevPage
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand NextPage
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand FirstPage
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand LastPage
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

    public ICommand CopyPromptCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CopyPathCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ShowInExplorerCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ExpandToFolderCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand DeleteCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand FavoriteCommand
    {
        get;
        set => SetField(ref field, value);
    }


    public ICommand CopyNegativePromptCommand
    {
        get;
        set => SetField(ref field, value);
    }


    public ICommand CopyOthersCommand
    {
        get;
        set => SetField(ref field, value);
    }


    public ICommand CopyParametersCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public long FileSize
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CopySeedCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CopyHashCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand RatingCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand NSFWCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand RemoveEntryCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CopyCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand MoveCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public int ThumbnailSize
    {
        get => _thumbnailSize;
        set => SetField(ref _thumbnailSize, value);
    }


    public ViewMode ViewMode
    {
        get;
        set => SetField(ref field, value);
    }

    public int PageSize
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand RescanCommand { get; set; }
    public ICommand PermanentlyDeleteCommand { get; set; }

    public int ThumbnailSpacing
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand RescanFolderCommand { get; set; }
    public ICommand ScanFolderCommand { get; set; }
}
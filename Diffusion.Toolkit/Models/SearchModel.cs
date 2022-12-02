using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Diffusion.IO;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit.Models;

public class SearchModel : BaseNotify
{
    private ObservableCollection<ImageEntry>? _images;
    private ImageEntry? _selectedImage;
    private BitmapSource? _image;
    //public object _rowLock = new object();
    private int _totalFiles;
    private int _currentPosition;
    private int _totalFilesScan;
    private int _currentPositionScan;
    private ICommand _searchCommand;
    private string _searchText;

    public SearchModel()
    {
        Images = new ObservableCollection<ImageEntry>();
        IsEmpty = true;
        Status = "Ready";
        //BindingOperations.EnableCollectionSynchronization(Images, _rowLock);
    }

    public ObservableCollection<ImageEntry>? Images
    {
        get => _images;
        set => SetField(ref _images, value);
    }

    public ImageEntry? SelectedImageEntry
    {
        get => _selectedImage;
        set => SetField(ref _selectedImage, value);
    }


    public BitmapSource? SelectedImage
    {
        get => _image;
        set => SetField(ref _image, value);
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


    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public ICommand SearchCommand
    {
        get => _searchCommand;
        set => SetField(ref _searchCommand, value);
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


    public ICommand CopyParameters
    {
        get => _copyParameters;
        set => SetField(ref _copyParameters, value);
    }


    private ICommand _copyOthersCommand;
    private ICommand _copyNegativePromptCommand;
    private ICommand _copyPathCommand;
    private ICommand _copyPromptCommand;
    private ICommand _copyParameters;
    private int _imageCount;
    private string _status;
    private int _page;
    private bool _isEmpty;
    private int _pages;
    private string _results;


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
}
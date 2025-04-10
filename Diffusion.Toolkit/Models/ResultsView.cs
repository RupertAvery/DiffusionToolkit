using System.Collections.ObjectModel;
using System.Windows.Input;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Models;

public class ResultsView : BaseNotify
{
    private ObservableCollection<ImageEntry> _images;
    private int _page;
    private int _pages;
    private bool _isEmpty;
    private string _results;
    private string _sortBy;
    private string _sortDirection;
    private string _resultStatus;

    private ICommand _pageChangedCommand;
    private ICommand _copyFiles;
    private ICommand _openCommand;
    private bool _isBusy;
    private bool _isPromptsBusy;

    public ResultsView()
    {
        SortBy = "Date";
        SortDirection = "DESC";
        ResultStatus = "Select a prompt";
    }

    public ObservableCollection<ImageEntry>? Images
    {
        get => _images;
        set => SetField(ref _images, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }

    public bool IsPromptsBusy
    {
        get => _isPromptsBusy;
        set => SetField(ref _isPromptsBusy, value);
    }

    public MainModel MainModel => ServiceLocator.MainModel;

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

    public string SortBy
    {
        get => _sortBy;
        set => SetField(ref _sortBy, value);
    }

    public string SortDirection
    {
        get => _sortDirection;
        set => SetField(ref _sortDirection, value);
    }

    public string ResultStatus
    {
        get => _resultStatus;
        set => SetField(ref _resultStatus, value);
    }

    public ICommand PageChangedCommand
    {
        get => _pageChangedCommand;
        set => SetField(ref _pageChangedCommand, value);
    }


    public ICommand OpenCommand
    {
        get => _openCommand;
        set => SetField(ref _openCommand, value);
    }



    public ICommand CopyFiles
    {
        get => _copyFiles;
        set => SetField(ref _copyFiles, value);
    }

}
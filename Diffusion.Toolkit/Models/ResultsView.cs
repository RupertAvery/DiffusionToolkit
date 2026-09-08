using System.Collections.ObjectModel;
using System.Windows.Input;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Models;

public class ResultsView : BaseNotify
{
    private ObservableCollection<ImageEntry> _images;
    private int _pages;

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
        get;
        set => SetField(ref field, value);
    }

    public bool IsPromptsBusy
    {
        get;
        set => SetField(ref field, value);
    }

    public MainModel MainModel => ServiceLocator.MainModel;

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
        set => SetField(ref field, value);
    }

    public string SortBy
    {
        get;
        set => SetField(ref field, value);
    }

    public string SortDirection
    {
        get;
        set => SetField(ref field, value);
    }

    public string ResultStatus
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand PageChangedCommand
    {
        get;
        set => SetField(ref field, value);
    }


    public ICommand OpenCommand
    {
        get;
        set => SetField(ref field, value);
    }


    public ICommand CopyFiles
    {
        get;
        set => SetField(ref field, value);
    }
}
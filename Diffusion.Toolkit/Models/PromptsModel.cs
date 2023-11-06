using System.Collections.ObjectModel;
using System.Windows.Input;
using Diffusion.Database;
using MdXaml.LinkActions;

namespace Diffusion.Toolkit.Models;

public class ResultsView : BaseNotify
{
    private readonly MainModel _mainModel;
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

    public ResultsView(MainModel mainModel)
    {
        _mainModel = mainModel;
        SortBy = "Date";
        SortDirection = "DESC";
        ResultStatus = "Select a prompt";
    }

    public ObservableCollection<ImageEntry>? Images
    {
        get => _images;
        set => SetField(ref _images, value);
    }

    public MainModel MainModel => _mainModel;

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

public class PromptsModel : BaseNotify
{
    private ObservableCollection<UsedPrompt> _prompts;
    private ObservableCollection<UsedPrompt> _negativePrompts;
    private string _promptQuery;
    private int _promptDistance;
    private bool _fullTextPrompt;
    private ResultsView _promptsResults;
    private ResultsView _negativePromptsResults;
    private UsedPrompt? _selectedPrompt;
    private MainModel _mainModel;

    public PromptsModel()
    {
    }

    public PromptsModel(MainModel mainModel)
    {
        _promptsResults = new ResultsView(mainModel);
        _negativePromptsResults = new ResultsView(mainModel);
        _mainModel = mainModel;
    }

    public MainModel MainModel => _mainModel;
    public ObservableCollection<UsedPrompt> Prompts
    {
        get => _prompts;
        set => SetField(ref _prompts, value);
    }

    public ResultsView PromptsResults
    {
        get => _promptsResults;
        set => SetField(ref _promptsResults, value);
    }

    public ObservableCollection<UsedPrompt> NegativePrompts
    {
        get => _negativePrompts;
        set => SetField(ref _negativePrompts, value);
    }

    public ResultsView NegativePromptsResults
    {
        get => _negativePromptsResults;
        set => SetField(ref _negativePromptsResults, value);
    }

    public string PromptQuery
    {
        get => _promptQuery;
        set => SetField(ref _promptQuery, value);
    }

    public bool FullTextPrompt
    {
        get => _fullTextPrompt;
        set => SetField(ref _fullTextPrompt, value);
    }

    public int PromptDistance
    {
        get => _promptDistance;
        set => SetField(ref _promptDistance, value);
    }

    public string NegativePromptQuery
    {
        get => _promptQuery;
        set => SetField(ref _promptQuery, value);
    }

    public bool NegativeFullTextPrompt
    {
        get => _fullTextPrompt;
        set => SetField(ref _fullTextPrompt, value);
    }

    public int NegativePromptDistance
    {
        get => _promptDistance;
        set => SetField(ref _promptDistance, value);
    }

    public UsedPrompt? SelectedPrompt
    {
        get => _selectedPrompt;
        set => SetField(ref _selectedPrompt, value);
    }
}
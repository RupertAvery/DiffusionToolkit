using System.Collections.ObjectModel;
using Diffusion.Database;
using Diffusion.Toolkit.Services;
using MdXaml.LinkActions;

namespace Diffusion.Toolkit.Models;

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
    private bool _isBusy;

    
    public PromptsModel()
    {
        _promptsResults = new ResultsView();
        _negativePromptsResults = new ResultsView();
    }

    public MainModel MainModel => ServiceLocator.MainModel;
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

    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }
}
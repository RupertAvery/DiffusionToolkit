using System.Collections.ObjectModel;
using Diffusion.Database;
using Diffusion.Toolkit.Services;
using MdXaml.LinkActions;

namespace Diffusion.Toolkit.Models;

public class PromptsModel : BaseNotify
{
    private string _promptQuery;
    private int _promptDistance;
    private bool _fullTextPrompt;
    private ResultsView _promptsResults;
    private ResultsView _negativePromptsResults;


    public PromptsModel()
    {
        _promptsResults = new ResultsView();
        _negativePromptsResults = new ResultsView();
    }

    public MainModel MainModel => ServiceLocator.MainModel;

    public ObservableCollection<UsedPrompt> Prompts
    {
        get;
        set => SetField(ref field, value);
    }

    public ResultsView PromptsResults
    {
        get => _promptsResults;
        set => SetField(ref _promptsResults, value);
    }

    public ObservableCollection<UsedPrompt> NegativePrompts
    {
        get;
        set => SetField(ref field, value);
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
        get;
        set => SetField(ref field, value);
    }

    public bool IsBusy
    {
        get;
        set => SetField(ref field, value);
    }
}
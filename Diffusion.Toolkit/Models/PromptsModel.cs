using System.Collections.ObjectModel;
using Diffusion.Database;

namespace Diffusion.Toolkit.Models;

public class PromptsModel : BaseNotify
{
    private ObservableCollection<UsedPrompt> _prompts;
    private ObservableCollection<UsedPrompt> _negativePrompts;
    private string _promptQuery;
    private int _promptDistance;
    private bool _fullTextPrompt;

    public ObservableCollection<UsedPrompt> Prompts
    {
        get => _prompts;
        set => SetField(ref _prompts, value);
    }

    public ObservableCollection<UsedPrompt> NegativePrompts
    {
        get => _negativePrompts;
        set => SetField(ref _negativePrompts, value);
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
}
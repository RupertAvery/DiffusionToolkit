using Diffusion.Toolkit.Configuration;

namespace Diffusion.Toolkit.Models;

public class MetadataSection : BaseNotify
{
    private AccordionState _promptState;
    private AccordionState _negativePromptState;
    private AccordionState _seedState;
    private AccordionState _samplerState;
    private AccordionState _pathState;
    private AccordionState _albumState;
    private AccordionState _othersState;
    private AccordionState _modelState;
    private AccordionState _dateState;
    private AccordionState _workflowState;

    public AccordionState PromptState
    {
        get => _promptState;
        set => SetField(ref _promptState, value);
    }

    public AccordionState NegativePromptState
    {
        get => _negativePromptState;
        set => SetField(ref _negativePromptState, value);
    }

    public AccordionState SeedState
    {
        get => _seedState;
        set => SetField(ref _seedState, value);
    }

    public AccordionState SamplerState
    {
        get => _samplerState;
        set => SetField(ref _samplerState, value);
    }

    public AccordionState OthersState
    {
        get => _othersState;
        set => SetField(ref _othersState, value);
    }

    public AccordionState ModelState
    {
        get => _modelState;
        set => SetField(ref _modelState, value);
    }

    public AccordionState PathState
    {
        get => _pathState;
        set => SetField(ref _pathState, value);
    }

    public AccordionState DateState
    {
        get => _dateState;
        set => SetField(ref _dateState, value);
    }

    public AccordionState AlbumState
    {
        get => _albumState;
        set => SetField(ref _albumState, value);
    }

    public AccordionState WorkflowState
    {
        get => _workflowState; 
        set => SetField(ref _workflowState, value);
    }
}
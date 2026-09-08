using Diffusion.Toolkit.Configuration;

namespace Diffusion.Toolkit.Models;

public class MetadataSection : BaseNotify
{
    public AccordionState PromptState
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState NegativePromptState
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState SeedState
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState SamplerState
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState OthersState
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState ModelState
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState PathState
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState DateState
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState AlbumState
    {
        get;
        set => SetField(ref field, value);
    }

    public AccordionState WorkflowState
    {
        get;
        set => SetField(ref field, value);
    }
}
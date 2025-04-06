namespace Diffusion.Toolkit.Configuration;

public class MetadataSectionSettings : SettingsContainer
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

    public void Attach(SettingsContainer settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }

    public AccordionState PromptState
    {
        get => _promptState;
        set => UpdateValue(ref _promptState, value);
    }

    public AccordionState NegativePromptState
    {
        get => _negativePromptState;
        set => UpdateValue(ref _negativePromptState, value);
    }

    public AccordionState SeedState
    {
        get => _seedState;
        set => UpdateValue(ref _seedState, value);
    }

    public AccordionState SamplerState
    {
        get => _samplerState;
        set => UpdateValue(ref _samplerState, value);
    }

    public AccordionState PathState
    {
        get => _pathState;
        set => UpdateValue(ref _pathState, value);
    }

    public AccordionState AlbumState
    {
        get => _albumState;
        set => UpdateValue(ref _albumState, value);
    }

    public AccordionState OthersState
    {
        get => _othersState;
        set => UpdateValue(ref _othersState, value);
    }

    public AccordionState ModelState
    {
        get => _modelState;
        set => UpdateValue(ref _modelState, value);
    }

    public AccordionState DateState
    {
        get => _dateState;
        set => UpdateValue(ref _dateState, value);
    }

}
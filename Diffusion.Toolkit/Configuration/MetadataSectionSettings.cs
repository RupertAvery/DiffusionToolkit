namespace Diffusion.Toolkit.Configuration;

public class MetadataSectionSettings : SettingsContainer
{
    public void Attach(SettingsContainer settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }

    public AccordionState PromptState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState NegativePromptState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState SeedState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState SamplerState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState PathState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState AlbumState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState OthersState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState ModelState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public AccordionState DateState
    {
        get;
        set => UpdateValue(ref field, value);
    }
}
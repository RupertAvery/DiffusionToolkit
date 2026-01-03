namespace Diffusion.Toolkit.Configuration;

public class AccordionSetting : SettingsContainer
{
    public void Attach(SettingsContainer settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }

    public AccordionState AccordionState
    {
        get;
        set => UpdateValue(ref field, value);
    }

    public double ContainerHeight
    {
        get;
        set => UpdateValue(ref field, value);
    }
}
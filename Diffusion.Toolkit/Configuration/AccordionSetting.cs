namespace Diffusion.Toolkit.Configuration;

public class AccordionSetting : SettingsContainer
{
    private AccordionState _accordionState;
    private double _containerHeight;

    public void Attach(SettingsContainer settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }
    public AccordionState AccordionState
    {
        get => _accordionState;
        set => UpdateValue(ref _accordionState, value);
    }

    public double ContainerHeight
    {
        get => _containerHeight;
        set => UpdateValue(ref _containerHeight, value);
    }
}
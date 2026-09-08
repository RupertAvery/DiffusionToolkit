namespace Diffusion.Toolkit.Configuration;

public class SettingChangedEventArgs
{
    public string SettingName { get; set; }
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}
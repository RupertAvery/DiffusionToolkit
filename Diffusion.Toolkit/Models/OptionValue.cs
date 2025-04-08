namespace Diffusion.Toolkit.Models;

public class OptionValue
{
    public string Name { get; }
    public string Value { get; }

    public OptionValue(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
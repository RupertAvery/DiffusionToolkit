namespace Diffusion.Toolkit.Models;

public class Langauge
{
    public string Name { get; set; }
    public string Culture { get; set; }

    public Langauge(string name, string culture)
    {
        Name = name;
        Culture = culture;
    }
}
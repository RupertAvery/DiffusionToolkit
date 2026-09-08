namespace Diffusion.Toolkit.Models;

public class ExternalApplicationModel : BaseNotify
{
    public string Name
    {
        get;
        set => SetField(ref field, value);
    }

    public string Path
    {
        get;
        set => SetField(ref field, value);
    }

    public string CommandLineArgs
    {
        get;
        set => SetField(ref field, value);
    }

    public string Shortcut
    {
        get;
        set => SetField(ref field, value);
    }
}
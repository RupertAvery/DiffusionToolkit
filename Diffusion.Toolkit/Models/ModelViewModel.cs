namespace Diffusion.Toolkit.Models;

public class ModelViewModel : BaseNotify
{
    public bool IsTicked
    {
        get;
        set => SetField(ref field, value);
    }

    public string Name { get; set; }
    public string Hash { get; set; }
    public string Hashv2 { get; set; }
    public string SHA256 { get; set; }
    public int ImageCount { get; set; }
}
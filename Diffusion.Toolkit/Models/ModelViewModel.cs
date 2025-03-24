namespace Diffusion.Toolkit.Models;

public class ModelViewModel : BaseNotify
{
    private bool _isTicked;

    public bool IsTicked
    {
        get => _isTicked;
        set => SetField(ref _isTicked, value);
    }

    public string Name { get; set; }
    public string Hash { get; set; }
    public string Hashv2 { get; set; }
    public string SHA256 { get; set; }
    public int ImageCount { get; set; }
}
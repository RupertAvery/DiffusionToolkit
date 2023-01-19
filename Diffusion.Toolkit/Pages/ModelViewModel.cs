namespace Diffusion.Toolkit.Pages;

public class ModelViewModel : BaseNotify
{
    private string _sha256;
    private string _hash;
    public string Path { get; set; }
    public string Filename { get; set; }

    public string Hash
    {
        get => _hash;
        set
        {
            if (SetField(ref _hash, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string SHA256
    {
        get => _sha256;
        set => SetField(ref _sha256, value);
    }

    public string DisplayName => $"{Filename} ({Hash.ToLower()})";
}
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Pages;

public class ModelViewModel : BaseNotify
{
    public string Path { get; set; }
    public string Filename { get; set; }

    public string Hash
    {
        get;
        set
        {
            if (SetField(ref field, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string SHA256
    {
        get;
        set => SetField(ref field, value);
    }

    public string DisplayName => $"{Filename}";
}
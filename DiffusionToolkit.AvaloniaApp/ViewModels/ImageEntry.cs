using Avalonia.Media.Imaging;
using Diffusion.Database;
using ReactiveUI;
using System.Threading.Tasks;

namespace DiffusionToolkit.AvaloniaApp.ViewModels;

public class ImageEntry : ViewModelBase
{
    public int? Rating { get; set; }
    public bool NSFW { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Filename { get; set; }
    public string Path { get; set; }
    public bool ForDeletion { get; set; }
}
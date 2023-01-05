using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit;

public class PreviewModel : BaseNotify
{
  
    private ImageViewModel? _currentImage;
    private bool _nsfwBlur;
    private bool _fitToPreview;

    public PreviewModel()
    {
        _currentImage = new ImageViewModel();
    }
        
    public ImageViewModel? CurrentImage
    {
        get => _currentImage;
        set => SetField(ref _currentImage, value);
    }
        
    public bool NSFWBlur
    {
        get => _nsfwBlur;
        set => SetField(ref _nsfwBlur, value);
    }


}
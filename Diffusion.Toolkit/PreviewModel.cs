using System.Windows.Input;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit;

public class PreviewModel : BaseNotify
{
  
    private ImageViewModel? _currentImage;
    private bool _nsfwBlur;
    private bool _fitToPreview;
    private bool _slideShowActive;

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

    public bool SlideShowActive
    {
        get => _slideShowActive;
        set => SetField(ref _slideShowActive, value);
    }

    public ICommand Close { get; set;  }
    public ICommand ToggleFitToPreview { get; set; }
    public ICommand ToggleActualSize { get; set; }
    public ICommand ToggleAutoAdvance { get; set; }
    public ICommand StartStopSlideShow { get; set; }
    public ICommand ToggleInfo { get; set; }
    public ICommand ToggleFullScreen { get; set; }

    public MainModel MainModel => ServiceLocator.MainModel;
}
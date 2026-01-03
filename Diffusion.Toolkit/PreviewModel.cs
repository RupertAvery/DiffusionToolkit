using System.Windows.Input;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit;

public class PreviewModel : BaseNotify
{
  
    private ImageViewModel? _currentImage;
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
        get;
        set => SetField(ref field, value);
    }

    public bool SlideShowActive
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand Close { get; set;  }
    public ICommand ToggleFitToPreview { get; set; }
    public ICommand ToggleActualSize { get; set; }
    public ICommand ToggleTagsCommand { get; set; }
    public ICommand ToggleAutoAdvance { get; set; }
    public ICommand StartStopSlideShow { get; set; }
    public ICommand ToggleInfo { get; set; }
    public ICommand ToggleFullScreen { get; set; }

    public ICommand OpenWithCommand { get; set; }

    public bool IsTopHover
    {
        get;
        set => SetField(ref field, value);
    }

    public MainModel MainModel => ServiceLocator.MainModel;
}
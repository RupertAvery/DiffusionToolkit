using Avalonia.Media.Imaging;
using Diffusion.IO;
using DiffusionToolkit.AvaloniaApp.Controls.Metadata;
using DiffusionToolkit.AvaloniaApp.ViewModels;
using ReactiveUI;
using System.Windows.Input;

namespace DiffusionToolkit.AvaloniaApp;

public class PreviewWindowViewModel : ViewModelBase
{
    private MetadataViewModel? _metadata;
    private Bitmap? _previewImage;
    private bool _isMetadataVisible;

    public Bitmap? PreviewImage
    {
        get => _previewImage;
        set => this.RaiseAndSetIfChanged(ref _previewImage, value);
    }

    public MetadataViewModel? Metadata
    {
        get => _metadata;
        set => this.RaiseAndSetIfChanged(ref _metadata, value);
    }
    
    public bool IsMetadataVisible
    {
        get => _isMetadataVisible;
        set => this.RaiseAndSetIfChanged(ref _isMetadataVisible, value);
    }

    public ICommand CloseCommand { get; set; }

    public void ToggleMetadata()
    {
        IsMetadataVisible = !IsMetadataVisible;
    }
}
using System;
using Avalonia.Media.Imaging;
using DiffusionToolkit.AvaloniaApp.ViewModels;
using ReactiveUI;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public class ThumbnailViewModel : ViewModelBase, IDisposable
{
    public object Source { get; set; }
    public string Path { get; set; }

    private Bitmap _thumbnailImage;
    private bool _isCurrent;
    private bool _isSelected;
    private bool _forDeletion;

    public Bitmap ThumbnailImage
    {
        get => _thumbnailImage;
        set => this.RaiseAndSetIfChanged(ref _thumbnailImage, value);
    }

    public bool IsCurrent
    {
        get => _isCurrent;
        set => this.RaiseAndSetIfChanged(ref _isCurrent, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public int? Rating { get; set; }
    public bool NSFW { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Filename { get; set; }

    public bool ForDeletion
    {
        get => _forDeletion;
        set => this.RaiseAndSetIfChanged(ref _forDeletion , value);
    }

    public void Dispose()
    {
        _thumbnailImage?.Dispose();
    }
}
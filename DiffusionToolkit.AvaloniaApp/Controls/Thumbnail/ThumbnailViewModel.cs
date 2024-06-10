using System;
using Avalonia.Media.Imaging;
using DiffusionToolkit.AvaloniaApp.ViewModels;
using ReactiveUI;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public enum ThumbnailStatus
{
    New,
    Loading,
    Loaded,
    Error
}

public class ThumbnailViewModel : ViewModelBase, IDisposable
{
    private Bitmap _thumbnailImage;
    private bool _isCurrent;
    private bool _isSelected;
    private bool _forDeletion;
    private bool _isDeselected;
    private int? _rating;
    private bool _nsfw;
    private bool _favorite;

    public int Id { get; set; }
    public object Source { get; set; }
    public string Path { get; set; }

    public Bitmap ThumbnailImage
    {
        get => _thumbnailImage;
        set => this.RaiseAndSetIfChanged(ref _thumbnailImage, value);
    }

    public bool IsCurrent
    {
        get => _isCurrent;
        set
        {
            this.RaiseAndSetIfChanged(ref _isCurrent, value);
            this.RaiseAndSetIfChanged(ref _isDeselected, !_isSelected && _isCurrent, nameof(IsDeselected));
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _isSelected, value);
            this.RaiseAndSetIfChanged(ref _isDeselected, !_isSelected && _isCurrent, nameof(IsDeselected));
        }
    }

    public bool IsDeselected
    {
        get => _isDeselected;
    }

    public int? Rating
    {
        get => _rating;
        set => this.RaiseAndSetIfChanged(ref _rating, value);
    }

    public bool NSFW
    {
        get => _nsfw;
        set => this.RaiseAndSetIfChanged(ref _nsfw, value);
    }

    public bool Favorite
    {
        get => _favorite;
        set => this.RaiseAndSetIfChanged(ref _favorite, value);
    }

    public int Width { get; set; }
    public int Height { get; set; }
    public string Filename { get; set; }

    public bool ForDeletion
    {
        get => _forDeletion;
        set => this.RaiseAndSetIfChanged(ref _forDeletion, value);
    }

    public ThumbnailStatus Status { get; set; }

    public void Dispose()
    {
        _thumbnailImage?.Dispose();
    }
}


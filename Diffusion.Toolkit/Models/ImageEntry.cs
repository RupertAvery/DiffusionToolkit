using System;
using System.Windows.Media.Imaging;
using Diffusion.IO;
using Diffusion.Toolkit.Thumbnails;

namespace Diffusion.Toolkit;

public class ImageEntry : BaseNotify
{
    private BitmapSource? _thumbnail;
    private string? _fileName;
    private int _id;
    private bool _forDeletion;
    private bool _favorite;
    private int? _rating;
    private long _requestId;
    private bool _nsfw;

    public ImageEntry(long requestId)
    {
        _requestId = requestId;
    }

    public int Id

    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public bool ForDeletion
    {
        get => _forDeletion;
        set => SetField(ref _forDeletion, value);
    }

    public bool Favorite
    {
        get => _favorite;
        set => SetField(ref _favorite, value);
    }

    public int? Rating
    {
        get => _rating;
        set => SetField(ref _rating, value);
    }

    public bool NSFW
    {
        get => _nsfw;
        set => SetField(ref _nsfw, value);
    }

    public string? FileName
    {
        get => _fileName;
        set => SetField(ref _fileName, value);
    }

    public BitmapSource? Thumbnail
    {
        get
        {
            if (_thumbnail == null)
            {
                var job = new ThumbnailJob()
                {
                    RequestId = _requestId,
                    Path = Path, 
                    Height = Height,
                    Width = Width
                };

                _ = ThumbnailLoader.Instance.QueueAsync(job, (d) =>
                {
                    _thumbnail = d;
                    OnPropertyChanged();
                });
            }
            return _thumbnail;
        }
        //set => SetField(ref _thumbnail, value);
    }

    public int Height { get; set; }
    public int Width { get; set; }
    public string Path { get; set; }
    public DateTime CreatedDate { get; set; }
}
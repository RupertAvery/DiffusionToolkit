using System;
using System.Windows.Media.Imaging;
using Diffusion.Toolkit.Thumbnails;

namespace Diffusion.Toolkit;


public enum EntryType
{
    File,
    Folder,
    Album
}

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
    private string _name;
    private bool _isAlbum;
    private bool _isFolder;
    private EntryType _entryType;
    private string? _score;

    public ImageEntry(long requestId)
    {
        _requestId = requestId;
    }

    public int Id

    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public EntryType EntryType
    {
        get => _entryType;
        set => SetField(ref _entryType, value);
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
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

    public string? Score
    {
        get => _score;
        set => SetField(ref _score, value);
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
        get => _thumbnail;
        set => SetField(ref _thumbnail, value);
    }

    public void LoadThumbnail()
    {
        var job = new ThumbnailJob()
        {
            RequestId = _requestId,
            EntryType = _entryType,
            Path = Path,
            Height = Height,
            Width = Width
        };

        //await ThumbnailLoader.Instance.QueueAsync(job, (d) =>
        //{
        //    Thumbnail = d;
        //    OnPropertyChanged(nameof(Thumbnail));
        //});


        //Task.Run(() =>
        //{
        //    Thumbnail = ThumbnailLoader.Instance.GetThumbnailDirect(Path, Width, Height);
        //    OnPropertyChanged(nameof(Thumbnail));
        //});

        _ = ThumbnailLoader.Instance.QueueAsync(job, (d) =>
        {
            Thumbnail = d;
            OnPropertyChanged(nameof(Thumbnail));
        });
    }

    public int Height { get; set; }
    public int Width { get; set; }
    public string Path { get; set; }
    public DateTime CreatedDate { get; set; }
    public int AlbumCount { get; set; }
}
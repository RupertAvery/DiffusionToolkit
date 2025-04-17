using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Diffusion.Toolkit.Models;

public class ImageEntry : BaseNotify
{
    private BitmapSource? _thumbnail;
    private string? _fileName;
    private int _id;
    private bool _forDeletion;
    private bool _favorite;
    private int? _rating;
    private bool _nsfw;
    private string _name;
    private bool _isAlbum;
    private bool _isFolder;
    private EntryType _entryType;
    private string? _score;
    private int _albumCount;
    private IEnumerable<string> _albums;
    private bool _unavailable;
    private bool _hasError;
    private bool _isEmpty;
    private string _path;
    private int _width;
    private int _height;
    private double _thumbnailHeight;
    private double _thumbnailWidth;

    public ImageEntry(long batchId)
    {
        BatchId = batchId;
        LoadState = LoadState.Unloaded;
    }

    public long BatchId { get; set; }

    public int Id

    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public EntryType EntryType
    {
        get => _entryType;
        set
        {
            var updated = SetField(ref _entryType, value);
            // Force rebinding of template selector
            if (updated) OnPropertyChanged(nameof(Self));
        }
    }

    public ImageEntry Self => this;

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

    public LoadState LoadState { get; set; }

    public Dispatcher? Dispatcher { get; set; }

    public bool Unavailable
    {
        get => _unavailable;
        set => SetField(ref _unavailable, value);
    }

    public int Height
    {
        get => _height;
        set => SetField(ref _height, value);
    }

    public int Width
    {
        get => _width;
        set => SetField(ref _width, value);
    }

    public double ThumbnailHeight
    {
        get => _thumbnailHeight;
        set => SetField(ref _thumbnailHeight, value);
    }

    public double ThumbnailWidth
    {
        get => _thumbnailWidth;
        set => SetField(ref _thumbnailWidth, value);
    }

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    public DateTime CreatedDate { get; set; }

    public int AlbumCount
    {
        get => _albumCount;
        set => SetField(ref _albumCount, value);
    }

    public IEnumerable<string> Albums
    {
        get => _albums;
        set => SetField(ref _albums, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => SetField(ref _hasError, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetField(ref _isEmpty, value);
    }
}
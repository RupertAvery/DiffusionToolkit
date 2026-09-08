using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Diffusion.Common;

namespace Diffusion.Toolkit.Models;

public class ImageEntry : BaseNotify
{
    private bool _isAlbum;
    private bool _isFolder;
    private string _sizeFormatted;

    public ImageEntry(long batchId)
    {
        BatchId = batchId;
        LoadState = LoadState.Unloaded;
    }

    public long BatchId { get; set; }

    public int Id

    {
        get;
        set => SetField(ref field, value);
    }

    public EntryType EntryType
    {
        get;
        set
        {
            var updated = SetField(ref field, value);
            // Force rebinding of template selector
            if (updated) OnPropertyChanged(nameof(Self));
        }
    }

    public ImageEntry Self => this;

    public string Name
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ForDeletion
    {
        get;
        set => SetField(ref field, value);
    }

    public bool Favorite
    {
        get;
        set => SetField(ref field, value);
    }

    public int? Rating
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Score
    {
        get;
        set => SetField(ref field, value);
    }

    public bool NSFW
    {
        get;
        set => SetField(ref field, value);
    }

    public string? FileName
    {
        get;
        set => SetField(ref field, value);
    }

    public BitmapSource? Thumbnail
    {
        get;
        set => SetField(ref field, value);
    }

    public LoadState LoadState { get; set; }

    public Dispatcher? Dispatcher { get; set; }

    public bool Unavailable
    {
        get;
        set => SetField(ref field, value);
    }

    public int Height
    {
        get;
        set => SetField(ref field, value);
    }

    public int Width
    {
        get;
        set => SetField(ref field, value);
    }

    public double ThumbnailHeight
    {
        get;
        set => SetField(ref field, value);
    }

    public double ThumbnailWidth
    {
        get;
        set => SetField(ref field, value);
    }

    public string Path
    {
        get;
        set => SetField(ref field, value);
    }

    public DateTime CreatedDate { get; set; }

    public int AlbumCount
    {
        get;
        set => SetField(ref field, value);
    }

    public IEnumerable<string> Albums
    {
        get;
        set => SetField(ref field, value);
    }

    public bool HasError
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsEmpty
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsWatched
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsRecursive
    {
        get;
        set => SetField(ref field, value);
    }

    public int Count
    {
        get;
        set => SetField(ref field, value);
    }

    public long Size
    {
        get;
        set => SetField(ref field, value);
    }

    public ImageType Type
    {
        get;
        set => SetField(ref field, value);
    }

    public void Clear()
    {
        BatchId = 0;
        Id = 0;
        EntryType = EntryType.File;
        Name = "";
        Favorite = false;
        ForDeletion = false;
        Rating = null;
        Score = "";
        NSFW = false;
        FileName = "";
        Path = "";
        CreatedDate = DateTime.MinValue;
        AlbumCount = 0;
        Albums = Enumerable.Empty<string>();
        HasError = false;
        Unavailable = false;
        LoadState = LoadState.Loaded;
        Dispatcher = Dispatcher;
        Thumbnail = null;
        IsEmpty = true;
        IsRecursive = false;
        IsWatched = false;
        Count = 0;
        Size = 0;
        Type = ImageType.Image;
    }
}
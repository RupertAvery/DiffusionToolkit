using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Diffusion.Toolkit.Thumbnails;

namespace Diffusion.Toolkit;
public enum EntryType
{
    File,
    Folder,
    Album
}

public enum LoadState
{
    Unloaded,
    Loading,
    Loaded,
}

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

    public LoadState LoadState { get; set; }

    public Dispatcher? Dispatcher { get; set; }

    public void QueueLoadThumbnail()
    {
        LoadState = LoadState.Loading;

        var job = new ThumbnailJob()
        {
            BatchId = BatchId,
            EntryType = _entryType,
            Path = Path,
            Height = Height,
            Width = Width
        };

        _ = ThumbnailLoader.Instance.QueueAsync(job, (d) =>
        {
            LoadState = LoadState.Loaded;

            if (d.Success)
            {
                if (Dispatcher != null)
                {
                    Dispatcher.Invoke(() => { Thumbnail = d.Image; });
                }
                else
                {
                    Thumbnail = d.Image;
                }
            }
            else
            {
                if (Dispatcher != null)
                {
                    Dispatcher.Invoke(() => { Unavailable = true; });
                }
                else
                {
                    Unavailable = true;
                }
            }

            //Debug.WriteLine($"Finished job {job.RequestId}");
            //OnPropertyChanged(nameof(Thumbnail));
        });
    }

    public bool Unavailable
    {
        get => _unavailable;
        set => SetField(ref _unavailable, value);
    }

    public int Height { get; set; }
    public int Width { get; set; }
    public string Path { get; set; }
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
}
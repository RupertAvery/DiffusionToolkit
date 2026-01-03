using Diffusion.Database;
using System;

namespace Diffusion.Toolkit.Models;

public class AlbumModel : BaseNotify, IAlbumInfo
{
    public int Id { get; set; }

    public string Name
    {
        get;
        set => SetField(ref field, value);
    }

    public int Order { get; set; }
    public DateTime LastUpdated { get; set; }

    public int ImageCount
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsTicked
    {
        get;
        set => SetField(ref field, value);
    }
}
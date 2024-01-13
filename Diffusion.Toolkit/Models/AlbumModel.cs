using Diffusion.Database;
using System;

namespace Diffusion.Toolkit.Models;

public class AlbumModel : BaseNotify, IAlbumInfo
{
    private bool _isTicked;
    public int Id { get; set; }
    public string Name { get; set; }
    public int Order { get; set; }
    public DateTime LastUpdated { get; set; }
    public int ImageCount { get; set; }

    public bool IsTicked
    {
        get => _isTicked;
        set => SetField(ref _isTicked, value);
    }
}
using SQLite;

namespace Diffusion.Database;

public class AlbumImage
{
    [Indexed(Name = "IDX_AlbumImage", Order = 1, Unique = true)]
    public int AlbumId { get; set; }
    [Indexed(Name = "IDX_AlbumImage", Order = 2, Unique = true)]
    public int ImageId { get; set; }
}
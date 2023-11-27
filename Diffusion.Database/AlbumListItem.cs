namespace Diffusion.Database;

public interface IAlbumInfo
{
    int Id { get; set; }
    string Name { get; set; }
}

public class AlbumListItem : IAlbumInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Order { get; set; }
    public DateTime LastUpdated { get; set; }
    public int ImageCount { get; set; }
}
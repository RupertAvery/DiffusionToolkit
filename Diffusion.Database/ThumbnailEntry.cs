using SQLite;

namespace Diffusion.Database;

public class ThumbnailEntry
{
    [PrimaryKey]
    public int Id { get; set; }
    public int Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Path { get; set; }
    public byte[] Data { get; set; }
    public DateTime CreatedDate { get; set; }
}
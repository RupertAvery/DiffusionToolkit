namespace DiffusionToolkit.AvaloniaApp.Thumbnails;

public class ThumbnailJob
{
    public long RequestId { get; set; }
    //public EntryType EntryType { get; set; }
    public int Id { get; set; }
    public int Size { get; set; }
    public string Path { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
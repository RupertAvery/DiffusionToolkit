using Diffusion.Common;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Thumbnails;

public class ThumbnailJob
{
    public long BatchId { get; set; }
    public EntryType EntryType { get; set; }
    public string Path { get; set; }
    public ImageType Type { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Rebuild { get; set; }
}
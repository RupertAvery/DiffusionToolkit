using System.Text.Json.Serialization;

namespace Diffusion.Database;


public class CountSize
{
    public int Total { get; set; }
    public long Size { get; set; }

    public void Deconstruct(out int total, out long size)
    {
        total = Total;
        size = Size;
    }
}
namespace Diffusion.Common;

public class Model
{
    public string Path { get; set; }
    public string Filename { get; set; }
    public string Hash { get; set; }
    public string? SHA256 { get; set; }
    public bool IsLocal { get; set; }
}

public enum ImageType
{
    Image = 0,
    Video = 1
}
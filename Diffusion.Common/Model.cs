namespace Diffusion.Common;

public class Model
{
    public string Path { get; set; }
    public string Filename { get; set; }
    public string Hash { get; set; }
    public string? SHA256 { get; set; }
    public bool IsLocal { get; set; }
}


public class ModelView
{
    public string? Name { get; set; }
    public string Path { get; set; }
    public string Hash { get; set; }
    public string Hashv2 { get; set; }
    public string SHA256 { get; set; }
    public int ImageCount { get; set; }
}
namespace Diffusion.Civitai.Models;

public class ModelImage
{
    public int Id { get; set; }
    public string Url { get; set; }
    public string Nsfw { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Hash { get; set; }
    public object Meta { get; set; }
}
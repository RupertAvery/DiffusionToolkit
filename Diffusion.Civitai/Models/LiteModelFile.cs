namespace Diffusion.Civitai.Models;

public class LiteModelFile
{
    public int Id { get; set; }
    public double SizeKB { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public FileMetadata Metadata { get; set; }
    public Hashes Hashes { get; set; }
    public string DownloadUrl { get; set; }
    public bool Primary { get; set; }
}
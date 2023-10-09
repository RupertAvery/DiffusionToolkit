namespace Diffusion.Civitai.Models;

public class LiteModelVersion
{
    public int Id { get; set; }
    public string Name { get; set; }
    //public string Description { get; set; }
    //public DateTime CreatedAt { get; set; }
    public string DownloadUrl { get; set; }
    public List<string> TrainedWords { get; set; }
    public List<LiteModelFile> Files { get; set; }
    //public List<ModelImage> Images { get; set; }
    //public Stats Stats { get; set; }
}
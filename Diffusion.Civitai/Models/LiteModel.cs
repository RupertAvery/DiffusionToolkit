namespace Diffusion.Civitai.Models;

public class LiteModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Creator Creator { get; set; }
    public List<LiteModelVersion> ModelVersions { get; set; }
}
namespace Diffusion.Civitai.Models;

public class LiteModelCollection
{
    public LiteModelCollection()
    {
        Models = new List<LiteModel>();
    }

    public double Date { get; set; }
    public List<LiteModel> Models { get; set; }
}
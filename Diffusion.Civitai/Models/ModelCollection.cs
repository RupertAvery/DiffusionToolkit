namespace Diffusion.Civitai.Models;

public class ModelCollection
{
    public ModelCollection()
    {
        Models = new List<Model>();
    }

    public double Date { get; set; }
    public List<Model> Models { get; set; }
}
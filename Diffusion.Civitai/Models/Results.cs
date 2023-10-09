namespace Diffusion.Civitai.Models;

public class Results<T>
{
    public T[] Items { get; set; }
    public ResultsMetadata Metadata { get; set; }
}
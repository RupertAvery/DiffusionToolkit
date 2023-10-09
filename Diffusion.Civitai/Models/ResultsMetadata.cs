namespace Diffusion.Civitai.Models;

public class ResultsMetadata
{
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string NextPage { get; set; }
}
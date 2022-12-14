using SQLite;

namespace Diffusion.Database;

public class Image
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Path { get; set; }
    public string? Prompt { get; set; }
    public string? NegativePrompt { get; set; }
    public int Steps { get; set; }
    public string Sampler { get; set; }

    public decimal CFGScale { get; set; }
    public long Seed { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string ModelHash { get; set; }
    public int BatchSize { get; set; }
    public int BatchPos { get; set; }
    //public string OtherParameters { get; set; }
    //public string Parameters { get; set; }
    public DateTime CreatedDate { get; set; }

    
    // These columns shouldn't be overwritten when batch updating 
    // Consider moving these out?
    public string? CustomTags { get; set; }
    public int? Rating { get; set; }
    public bool Favorite { get; set; }
    public bool ForDeletion { get; set; }
    public bool NSFW { get; set; }


    public decimal? AestheticScore { get; set; }
    public string? HyperNetwork { get; set; }
    public decimal? HyperNetworkStrength { get; set; }
    public int? ClipSkip { get; set; }
    public int? ENSD { get; set; }
    public long FileSize { get; set; }
}
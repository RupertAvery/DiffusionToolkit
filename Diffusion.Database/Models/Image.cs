using Diffusion.Common;
using SQLite;

namespace Diffusion.Database.Models;

[AttributeUsage(AttributeTargets.Property)]
public class UserDefinedAttribute : Attribute {

}

public class Image
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int RootFolderId { get; set; }
    public int FolderId { get; set; }
    public string Path { get; set; }
    public string FileName { get; set; }
    public string? Prompt { get; set; }
    public string? NegativePrompt { get; set; }
    public int Steps { get; set; }
    public string? Sampler { get; set; }

    public decimal CFGScale { get; set; }
    public long Seed { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? ModelHash { get; set; }
    public string? Model { get; set; }
    public int BatchSize { get; set; }
    public int BatchPos { get; set; }
    //public string OtherParameters { get; set; }
    //public string Parameters { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    // These columns shouldn't be overwritten when batch updating 
    // Consider moving these out?
    [UserDefined]
    public string? CustomTags { get; set; }
    [UserDefined]
    public int? Rating { get; set; }
    [UserDefined]
    public bool Favorite { get; set; }
    [UserDefined]
    public bool ForDeletion { get; set; }
    [UserDefined]
    public bool NSFW { get; set; }
    [UserDefined]
    public bool Unavailable { get; set; }

    public decimal? AestheticScore { get; set; }
    public string? HyperNetwork { get; set; }
    public decimal? HyperNetworkStrength { get; set; }
    public int? ClipSkip { get; set; }
    public int? ENSD { get; set; }
    public long FileSize { get; set; }
    public bool NoMetadata { get; set; }
    public string? Workflow { get; set; }
    public string? WorkflowId { get; set; }
    public bool HasError { get; set; }
    public string? Hash { get; set; }
    [UserDefined]
    public DateTime? ViewedDate { get; set; }
    [UserDefined]
    public DateTime? TouchedDate { get; set; }
    public ImageType Type { get; set; }
}


public class ImageView 
{
    public int Id { get; set; }
    public bool Favorite { get; set; }
    public bool ForDeletion { get; set; }
    public int? Rating { get; set; }
    public float AestheticScore { get; set; }
    public string Path { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool NSFW { get; set; }
    public int  AlbumCount { get; set; }
    public bool HasError { get; set; }
}
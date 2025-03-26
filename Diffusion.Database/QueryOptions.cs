namespace Diffusion.Database;

public class QueryOptions
{
    public string Query { get; set; }
    //public IReadOnlyCollection<int> FolderIds { get; set; }
    public IReadOnlyCollection<int> AlbumIds { get; set; }

    public IReadOnlyCollection<ModelInfo> Models { get; set; }
    public SearchView SearchView { get; set; }
    public bool HideDeleted { get; set; }
    public bool HideUnavailable { get; set; }
    public bool HideNSFW { get; set; }
    public bool SearchNodes { get; set; }
    public ComfyQueryOptions ComfyQueryOptions { get; set; }
    public string? Folder { get; set; }
    public bool UseFilter { get; set; }
    public Filter Filter { get; set; }
    public bool SearchRawData { get; set; }
    public bool SearchAllProperties { get; set; }
}


public class ModelInfo
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public string HashV2 { get; set; }
}


public class CountSize
{
    public int Total { get; set; }
    public long Size { get; set; }

    public void Deconstruct(out int total, out long size)
    {
        total = Total;
        size = Size;
    }
}
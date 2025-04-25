using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Diffusion.Common.Query;

public class QueryOptions 
{
    public QueryOptions()
    {
        Filter = new Filter();
        ComfyQueryOptions = new ComfyQueryOptions();
        SearchView = SearchView.Search;
    }

    public string? Query { get; set; }
    public Filter Filter { get; set; }
    [JsonIgnore]
    public bool HasQuery => Filter.IsEmpty && !string.IsNullOrEmpty(Query);

    [JsonIgnore] public bool HasFilter => !Filter.IsEmpty;
    public bool HideDeleted { get; set; }
    public bool HideUnavailable { get; set; }
    public bool HideNSFW { get; set; }
    public IReadOnlyCollection<int> AlbumIds { get; set; }
    public IReadOnlyCollection<ModelInfo> Models { get; set; }
    public SearchView SearchView { get; set; }
    public bool SearchNodes { get; set; }
    public ComfyQueryOptions ComfyQueryOptions { get; set; }
    public string? Folder { get; set; }
    public bool SearchRawData { get; set; }
    public bool SearchAllProperties { get; set; }

    [JsonIgnore]
    public bool IsEmpty => Filter.IsEmpty && string.IsNullOrEmpty(Query);
}
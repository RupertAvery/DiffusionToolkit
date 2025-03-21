using System.Linq;

namespace Diffusion.Database;

public class QueryOptions
{
    public string Query { get; set; }
    public IReadOnlyCollection<int> FolderIds { get; set; }
    public IReadOnlyCollection<int> AlbumIds { get; set; }
    public bool HideDeleted { get; set; }
    public bool HideUnavailable { get; set; }
    public bool HideNSFW { get; set; }
    public bool SearchNodes { get; set; }
    public ComfyQueryOptions ComfyQueryOptions { get; set; }
}

public class ComfyQueryOptions
{
    public bool SearchAllProperties { get; set; }
    public IEnumerable<string> SearchProperties { get; set; }
}

public static class ComfyUIQueryBuilder
{


    public static (string Query, IEnumerable<object> Bindings) Parse(string? prompt, ComfyQueryOptions options)
    {
        var conditions = new List<KeyValuePair<string, object>>();

        if (prompt is not null)
        {
            ParsePrompt(prompt, conditions);
        }

        var whereClause = string.Join(" AND ", conditions.Select(c => c.Key));

        var bindings = conditions.SelectMany(c =>
        {
            return c.Value switch
            {
                IEnumerable<object> orConditions => orConditions.Select(o => o),
                _ => new[] { c.Value }
            };
        }).Where(o => o != null);

        var properties = string.Join(" OR ", options.SearchProperties.Select(p => $"cmfyp.Name = '{p}'"));

        return (
            "SELECT m1.Id from Image m1 " +
            "INNER JOIN Node cmfyn ON m1.Id = cmfyn.ImageId " +
            "INNER JOIN NodeProperty cmfyp ON cmfyp.NodeId = cmfyn.Id " +
            "WHERE " +
            (options.SearchAllProperties ? "" : $"( {properties} ) AND ") +
            $"{whereClause}",
            bindings
        );

        //"p.Name LIKE 'lora__02'
        //"p.Value = @ComfyUIPrompt";
        //"AND n.Name = 'Lora Loader Stack (rgthree)'
    }

    private static void ParsePrompt(string prompt, List<KeyValuePair<string, object>> conditions)
    {
        if (prompt.Trim().Length == 0)
        {
            conditions.Add(new KeyValuePair<string, object>("(cmfyp.Value LIKE ? OR cmfyp.Value IS NULL)", "%%"));
            return;
        }

        var tokens = CSVParser.Parse(prompt);

        foreach (var token in tokens)
        {
            conditions.Add(new KeyValuePair<string, object>("(cmfyp.Value LIKE ?)", $"%{token.Trim()}%"));
        }
    }

    public static (string Query, IEnumerable<object> Bindings) Filter(Filter filter)
    {
        var queries = new List<string>();
        var bindings = new List<object>();

        foreach (var node in filter.NodeFilters.Where(d => d is { IsActive: true, Property.Length: > 0, Value.Length: > 0 }))
        {
            var parsedNode = ParseNode(node);
            queries.Add(parsedNode.Query);
            bindings.AddRange(parsedNode.Bindings);
        }

        var query = string.Join(" UNION ", queries);

        return (query, bindings);
    }

    private static (string Query, IEnumerable<object> Bindings) ParseNode(NodeFilter node)
    {
        var conditions = new List<KeyValuePair<string, object>>();

        var tokens = CSVParser.Parse(node.Value);

        foreach (var token in tokens)
        {
            conditions.Add(new KeyValuePair<string, object>("(cmfyp.Value LIKE ?)", $"%{token.Trim()}%"));
        }

        var whereClause = string.Join(" AND ", conditions.Select(c => c.Key));

        var bindings = conditions.SelectMany(c =>
        {
            return c.Value switch
            {
                IEnumerable<object> orConditions => orConditions.Select(o => o),
                _ => new[] { c.Value }
            };
        }).Where(o => o != null);

        return (
            "SELECT m1.Id from Image m1 " +
            "INNER JOIN Node cmfyn ON m1.Id = cmfyn.ImageId " +
            "INNER JOIN NodeProperty cmfyp ON cmfyp.NodeId = cmfyn.Id " +
            "WHERE " +
            $"cmfyp.Name = '{node.Property}' " +
            $"AND {whereClause}",
            bindings
        );
    }
}
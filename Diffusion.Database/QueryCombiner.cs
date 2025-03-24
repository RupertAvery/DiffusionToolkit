using System.Drawing;

namespace Diffusion.Database;

public static class QueryCombiner
{
    public static string GetInitialWhereClause(string imageAlias, QueryOptions options)
    {

        var whereClauses = new List<string>();

        if (options.HideNSFW)
        {
            whereClauses.Add($"({imageAlias}.NSFW = 0 OR {imageAlias}.NSFW IS NULL)");
        }

        if (options.SearchView != SearchView.Deleted && options.HideDeleted)
        {
            whereClauses.Add($"({imageAlias}.ForDeletion = 0)");
        }

        if (options.HideUnavailable)
        {
            whereClauses.Add($"({imageAlias}.Unavailable = 0)");
        }

        var whereExpression = string.Join(" AND ", whereClauses);

        return whereClauses.Any() ? $"{whereExpression}" : "";
    }

    public static (string Query, IEnumerable<object> Bindings) Parse(QueryOptions options)
    {
        var q = QueryBuilder.Parse(options.Query);

        var where1Clause = q.WhereClause is { Length: > 0 } ? $" WHERE {q.WhereClause}" : "";

        var query = $"SELECT m1.Id FROM Image m1 {string.Join(' ', q.Joins)} {where1Clause}";

        var bindings = q.Bindings;

        if (options is { SearchNodes: true, Query.Length: > 0 })
        {
            var p = ComfyUIQueryBuilder.Parse(q.TextPrompt, options.ComfyQueryOptions);

            query += " UNION " +
                     $"{p.Query}";

            bindings = bindings.Concat(p.Bindings);
        }

        ApplyFilters(ref query, ref bindings, options);
        
        return (query, bindings);
    }


    public static (string Query, IEnumerable<object> Bindings, IEnumerable<string> Joins) Filter(Filter filter, QueryOptions options)
    {
        if (options.SearchView == SearchView.Deleted)
        {
            filter.UseForDeletion = false;
        }

        var q = QueryBuilder.Filter(filter);

        var where1Clause = q.WhereClause is { Length: > 0 } ? $" WHERE {q.WhereClause}" : "";

        var query = $"SELECT m1.Id FROM Image m1 {string.Join(' ', q.Joins)} {where1Clause}";

        var bindings = q.Bindings;
        var joins = q.Joins;

        if (filter.NodeFilters != null && filter.NodeFilters.Any(d => d is { IsActive: true, Property.Length: > 0, Value.Length: > 0 }))
        {
            var p = ComfyUIQueryBuilder.Filter(filter);

            query = (where1Clause.Length > 0 ? query + " INTERSECT " : "") +
                     $"SELECT Id FROM ({p.Query})";

            bindings = bindings.Concat(p.Bindings);
        }

        ApplyFilters(ref query, ref bindings, options);

        return (query, bindings, joins);
    }

    private static void ApplyFilters(ref string query, ref IEnumerable<object> bindings, QueryOptions options)
    {
        var filters = new List<string>();

        switch (options.SearchView)
        {
            case SearchView.Favorites:
                filters.Add($"SELECT m1.Id FROM Image m1 WHERE m1.Favorite = 1");
                break;
            case SearchView.Deleted:
                filters.Add($"SELECT m1.Id FROM Image m1 WHERE m1.ForDeletion = 1");
                break;
        }

        if (options.AlbumIds is { Count: > 0 })
        {
            var albumIds = string.Join(",", options.AlbumIds.Select(a => "?"));
            filters.Add($"SELECT DISTINCT m1.Id FROM Image m1 INNER JOIN AlbumImage ai ON ai.ImageId = m1.Id INNER JOIN Album a ON a.Id = ai.AlbumId WHERE a.Id IN ({albumIds})");
            bindings = bindings.Concat(options.AlbumIds.Cast<object>());
        }

        if (options.FolderIds is { Count: > 0 })
        {
            var folderIds = string.Join(",", options.FolderIds.Select(a => "?"));
            filters.Add($"SELECT m1.Id FROM Image m1 INNER JOIN Folder f ON f.Id = m1.FolderId WHERE f.Id IN ({folderIds})");
            bindings = bindings.Concat(options.AlbumIds.Cast<object>());
        }
        
        if (options.SearchView == SearchView.Folder)
        {
            filters.Add($"SELECT m1.Id FROM Image m1 INNER JOIN Folder f ON f.Id = m1.FolderId WHERE f.Path = ?");
            bindings = bindings.Concat(new[]{ (object)options.Folder! });
        }

        if (filters.Any())
        {
            query = $"SELECT Id FROM ({query}) INTERSECT " + string.Join(" INTERSECT ", filters);
        }
    }

}
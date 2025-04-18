using System.Linq;
using Diffusion.Common;

namespace Diffusion.Database;

public static class ComfyUIQueryBuilder
{


    public static (string Query, IEnumerable<object> Bindings) Parse(string? prompt, QueryOptions options)
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

        string? properties = null;

        if (options.SearchNodes)
        {
            var searchProperties = options.ComfyQueryOptions.SearchProperties;
            properties = searchProperties != null && searchProperties.Any() ? string.Join(" OR ", searchProperties.Select(p => $"(cmfyp.Name = '{p}')")) : "";
        }

        return (
            "SELECT cmfyn.ImageId AS Id FROM Node cmfyn " +
            "INNER JOIN NodeProperty cmfyp ON cmfyp.NodeId = cmfyn.Id " +
            "WHERE " +
            (properties is { Length: > 0 } ? $"( {properties} ) AND " : "") +
            $"{whereClause}",
            bindings
        );
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

        var index = 0;

        foreach (var node in filter.NodeFilters.Where(d => d is { IsActive: true, Property.Length: > 0, Value.Length: > 0 }))
        {
            var parsedNode = ParseNode(node);
            var operation = index == 0 ? "" : parsedNode.Operation;
            queries.Add($"{operation} {parsedNode.Query}");
            bindings.AddRange(parsedNode.Bindings);
            index++;
        }

        var query = string.Join("", queries);

        return (query, bindings);
    }

    private static (string Operation, string Query, IEnumerable<object> Bindings) ParseNode(NodeFilter node)
    {
        var conditions = new List<KeyValuePair<string, object>>();

        //var tokens = CSVParser.Parse(node.Value);
        var operation = node.Operation;

        var comparison = node.Comparison;

        var value = node.Value;
        var escape = "";

        if (value.Contains("%"))
        {
            value = value.Replace("%", "`%");
            escape = "ESCAPE '`'";
        }

        var toper = "=";

        switch (comparison)
        {
            case NodeComparison.Contains:
                value = $"%{value}%";
                toper = "LIKE";
                break;
            case NodeComparison.StartsWith:
                value = $"{value}%";
                toper = "LIKE";
                break;
            case NodeComparison.EndsWith:
                value = $"%{value}";
                toper = "LIKE";
                break;
            case NodeComparison.Equals:
                value = $"{value}";
                toper = "=";
                break;
        }

        conditions.Add(new KeyValuePair<string, object>($"(cmfyp.Value {toper} ? {escape})", value));


        var whereClause = string.Join(" AND ", conditions.Select(c => c.Key));

        var bindings = conditions.SelectMany(c =>
        {
            return c.Value switch
            {
                IEnumerable<object> orConditions => orConditions.Select(o => o),
                _ => new[] { c.Value }
            };
        }).Where(o => o != null);

        var oper = node.Property.Contains("*") ? "LIKE" : "=";

        var prop = node.Property.Replace("*", "%").Trim();

        return (
            operation.ToString(),
            "SELECT DISTINCT cmfyn.ImageId AS Id FROM Node cmfyn  " +
            "INNER JOIN NodeProperty cmfyp ON cmfyp.NodeId = cmfyn.Id " +
            "WHERE " +
            $"cmfyp.Name {oper} '{prop}' " +
            $"AND ({whereClause})",
            bindings
        );
    }
}
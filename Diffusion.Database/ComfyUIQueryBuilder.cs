namespace Diffusion.Database;

public class QueryOptions
{
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
    

    public static (string Query, IEnumerable<object> Bindings) Parse(string prompt, ComfyQueryOptions options)
    {
        var conditions = new List<KeyValuePair<string, object>>();

        ParsePrompt(prompt, conditions);

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
            "SELECT cmfyimg.Id from Image cmfyimg " +
            "INNER JOIN Node cmfyn ON cmfyimg.Id = cmfyn.ImageId " +
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
}
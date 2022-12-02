using System.Text.RegularExpressions;

namespace Diffusion.Database;

public static class QueryBuilder
{
    private static readonly Regex SeedRegex = new Regex("\\bseed:\\s*(?<start>\\d+)(?:-(?<end>\\S+))?\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex StepsRegex = new Regex("\\bsteps:\\s*(\\d+)(?:\\|(\\d+))*\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SamplerRegex = new Regex("\\bsampler:\\s*(\\S+)(?:\\|(\\S+))*\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HashRegex = new Regex("\\b(?:model_hash|model hash):\\s*([0123456789abcdef]+)(?:\\|([0123456789abcdef]+))*\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CfgRegex = new Regex("\\b(?:cfg|cfg_scale|cfg scale):\\s*(\\d+(?:\\.\\d+)?)(?:\\|(\\d+(?:\\.\\d+)?))*\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SizeRegex = new Regex("\\bsize:\\s*((?:(?<width>\\d+|\\?)x(?<height>\\d+|\\?))|(?:(?<width>\\d+|\\?):(?<height>\\d+|\\?)))[\\b]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NumericRegex = new Regex("\\d+");

    public static (string, IEnumerable<object>) Parse(string prompt)
    {
        var conditions = new List<KeyValuePair<string, object>>();

        var seedMatch = SeedRegex.Match(prompt);
        if (seedMatch.Success)
        {
            prompt = SeedRegex.Replace(prompt, String.Empty);
            if (seedMatch.Groups["end"].Success)
            {
                conditions.Add(new KeyValuePair<string, object>("(Seed BETWEEN ? AND ?)", new object[] { long.Parse(seedMatch.Groups["start"].Value), long.Parse(seedMatch.Groups["end"].Value) }));
            }
            else
            {
                conditions.Add(new KeyValuePair<string, object>("(Seed = ?)", long.Parse(seedMatch.Groups["start"].Value)));
            }
        }

        var stepsMatch = StepsRegex.Match(prompt);
        if (stepsMatch.Success)
        {
            prompt = StepsRegex.Replace(prompt, String.Empty);

            var orConditions = new List<KeyValuePair<string, object>>();

            for (var i = 1; i < stepsMatch.Groups.Count; i++)
            {
                if (stepsMatch.Groups[i].Value.Length > 0)
                {
                    orConditions.Add(new KeyValuePair<string, object>("(Steps = ?)", int.Parse(stepsMatch.Groups[i].Value)));
                }
            }

            var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
            var values = orConditions.Select(c => c.Value);

            conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
        }

        var samplerMatch = SamplerRegex.Match(prompt);
        if (samplerMatch.Success)
        {
            prompt = SamplerRegex.Replace(prompt, String.Empty);
            var orConditions = new List<KeyValuePair<string, object>>();

            for (var i = 1; i < samplerMatch.Groups.Count; i++)
            {
                if (samplerMatch.Groups[i].Value.Length > 0)
                {
                    orConditions.Add(new KeyValuePair<string, object>("(LOWER(Sampler) = LOWER(?))", samplerMatch.Groups[i].Value.Replace("_", " ")));
                }
            }

            var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
            var values = orConditions.Select(c => c.Value);

            conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
        }

        var hashMatch = HashRegex.Match(prompt);
        if (hashMatch.Success)
        {
            prompt = HashRegex.Replace(prompt, String.Empty);
            var orConditions = new List<KeyValuePair<string, object>>();

            for (var i = 1; i < hashMatch.Groups.Count; i++)
            {
                if (hashMatch.Groups[i].Value.Length > 0)
                {
                    orConditions.Add(new KeyValuePair<string, object>("(ModelHash = ?)", hashMatch.Groups[i].Value));
                }
            }

            var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
            var values = orConditions.Select(c => c.Value);

            conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
        }

        var cfgMatch = CfgRegex.Match(prompt);
        if (cfgMatch.Success)
        {
            prompt = CfgRegex.Replace(prompt, String.Empty);
            var orConditions = new List<KeyValuePair<string, object>>();

            for (var i = 1; i < cfgMatch.Groups.Count; i++)
            {
                if (cfgMatch.Groups[i].Value.Length > 0)
                {
                    orConditions.Add(new KeyValuePair<string, object>("(CFGScale = ?)", float.Parse(cfgMatch.Groups[i].Value)));
                }
            }

            var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
            var values = orConditions.Select(c => c.Value);

            conditions.Add(new KeyValuePair<string, object>($"({keys})", values));

        }

        var sizeMatch = SizeRegex.Match(prompt);
        if (sizeMatch.Success)
        {
            prompt = SizeRegex.Replace(prompt, String.Empty);

            var height = sizeMatch.Groups["height"].Value;
            if (NumericRegex.IsMatch(height))
            {
                conditions.Add(new KeyValuePair<string, object>("(Height = ?)", int.Parse(height)));
            }

            var width = sizeMatch.Groups["width"].Value;
            if (NumericRegex.IsMatch(width))
            {
                conditions.Add(new KeyValuePair<string, object>("(Width = ?)", int.Parse(width)));
            }

        }

        var tokens = CSVParser.Parse(prompt);


        foreach (var token in tokens)
        {
            conditions.Add(new KeyValuePair<string, object>("(Prompt LIKE ?)", $"%{token.Trim()}%"));
        }

        return (string.Join(" AND ", conditions.Select(c => c.Key)),
            conditions.SelectMany(c =>
            {
                return c.Value switch
                {
                    IEnumerable<object> orConditions => orConditions.Select(o => o),
                    _ => new[] { c.Value }
                };
            }));
    }
}
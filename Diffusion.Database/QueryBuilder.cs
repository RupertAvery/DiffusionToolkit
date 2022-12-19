using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Diffusion.Database;

public static class QueryBuilder
{
    public static readonly Regex DayRegex = new Regex("\\d+ day(?:s)? ago", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static readonly Regex MonthRegex = new Regex("(?:a|1|2|3) week(?:s)? ago", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex DateFormatRegex = new Regex("\\d{1,2}[-/]\\d{1,2}[-/]\\d{4}|\\d{4}[-/]\\d{1,2}[-/]\\d{1,2}", RegexOptions.Compiled | RegexOptions.IgnoreCase);


    private static readonly Regex PathRegex = new Regex("\\bpath:\\s*(?:(?<criteria>starts with|contains|ends with)\\s+)?(?:\"(?<value>[^\"]+)\"|(?<value>\\S+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DateRegex = new Regex("\\bdate:\\s*(?:(?<prep1>between|before|since|from)\\s+)?(?<date1>today|yesterday|\\d+ day(?:s)? ago|(?:a|1|2|3) week(?:s)? ago|(?:a|\\d{1,2}) month(?:s)? ago|\\d{1,2}[-/]\\d{1,2}[-/]\\d{4}|\\d{4}[-/]\\d{1,2}[-/]\\d{1,2})(?:\\s+(?<prep2>and|up to|to)\\s+(?<date2>today|yesterday|\\d+ day(?:s)? ago|(?:a|1|2|3) week(?:s)? ago|(?:a|\\d{1,2}) month(?:s)? ago|\\d{1,2}[-/]\\d{1,2}[-/]\\d{4}|\\d{4}[-/]\\d{1,2}[-/]\\d{1,2}))?\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SeedRegex = new Regex("\\bseed:\\s*(?<start>\\d+)(?:\\s*-\\s*(?<end>\\S+))?\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex StepsRegex = new Regex("\\bsteps:\\s*(\\d+)(?:\\|(\\d+))*\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SamplerRegex = new Regex("sampler:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HashRegex = new Regex("\\b(?:model_hash|model hash):\\s*([0-9a-f]+)(?:\\s*\\|\\s*([0-9a-f]+))*\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CfgRegex = new Regex("\\b(?:cfg|cfg_scale|cfg scale):\\s*(\\d+(?:\\.\\d+)?)(?:\\s*\\|\\s*(\\d+(?:\\.\\d+)?))*\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SizeRegex = new Regex("\\bsize:\\s*((?:(?<width>\\d+|\\?)\\s*x\\s*(?<height>\\d+|\\?))|(?:(?<width>\\d+|\\?)\\s*:\\s*(?<height>\\d+|\\?)))[\\b]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NumericRegex = new Regex("\\d+");


    private static readonly Regex AestheticScoreRegex = new Regex("\\baesthetic_score:\\s*(?<operator><|>|<=|>=|<>)?\\s*(?<value>\\d+(?:\\.\\d+)?)\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HypernetRegex = new Regex("\\bhypernet:\\s*(\\S+)(?:\\s*\\|\\s*(\\S+))*\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HypernetStrRegex = new Regex("\\bhypernet strength:\\s*(?<operator><|>|<=|>=|<>)?\\s*(?<value>\\d+(?:\\.\\d+)?)\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex RatingRegex = new Regex("\\brating:\\s*(?:(?<value>none)|(?<operator><|>|<=|>=|<>)?\\s*(?<value>\\d+))\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ForeDeletionRegex = new Regex("\\b(?:for deletion|delete|to delete):\\s*(?<value>(?:true|false))?\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex FavoriteRegex = new Regex("\\b(?:favorite|fave):\\s*(?<value>(?:true|false))?\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex NSFWRegex = new Regex("\\b(?:nsfw):\\s*(?<value>(?:true|false))?\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex NegativePromptRegex = new Regex("\\b(?:negative prompt|negative_prompt|-):\\s*(?<value>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static List<string> Samplers { get; set; }

    public static bool HideNFSW { get; set; }

    public static (string, IEnumerable<object>) Parse(string prompt)
    {
        var conditions = new List<KeyValuePair<string, object>>();

        ParsePath(ref prompt, conditions);
        ParseDate(ref prompt, conditions);
        ParseSeed(ref prompt, conditions);
        ParseSteps(ref prompt, conditions);
        ParseSampler(ref prompt, conditions);
        ParseHash(ref prompt, conditions);
        ParseCFG(ref prompt, conditions);
        ParseSize(ref prompt, conditions);
        ParseAestheticScore(ref prompt, conditions);
        ParseRating(ref prompt, conditions);
        ParseHypernet(ref prompt, conditions);
        ParseHypernetStrength(ref prompt, conditions);
        ParseFavorite(ref prompt, conditions);
        ParseForDeletion(ref prompt, conditions);
        ParseNSFW(ref prompt, conditions);

        ParseNegativePrompt(ref prompt, conditions);
        ParsePrompt(ref prompt, conditions);

        return (string.Join(" AND ", conditions.Select(c => c.Key)),
            conditions.SelectMany(c =>
            {
                return c.Value switch
                {
                    IEnumerable<object> orConditions => orConditions.Select(o => o),
                    _ => new[] { c.Value }
                };
            }).Where(o => o != null));
    }

    private static void ParsePath(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = PathRegex.Match(prompt);
        if (match.Success)
        {
            prompt = PathRegex.Replace(prompt, String.Empty);

            var value = match.Groups["value"].Value;
            if (match.Groups["criteria"].Success)
            {
                if (value.Any(t => t is '*' or '?'))
                {
                    throw new Exception("Invalid path. Do not use *, ? if criteria 'starts with', 'contains', or 'ends with' is specified");
                }

                switch (match.Groups["criteria"].Value.ToLower())
                {
                    case "starts with":
                        value = $"{value}**";
                        break;
                    case "contains":
                        value = $"**{value}**";
                        break;
                    case "ends with":
                        value = $"**{value}";
                        break;
                }
            }

            conditions.Add(new KeyValuePair<string, object>("(Path GLOB ?)", value));

        }
    }

    private static void ParseForDeletion(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = ForeDeletionRegex.Match(prompt);
        if (match.Success)
        {
            prompt = ForeDeletionRegex.Replace(prompt, String.Empty);

            var value = true;

            if (match.Groups["value"].Success)
            {
                value = match.Groups["value"].Value.ToLower() == "true";
            }

            conditions.Add(new KeyValuePair<string, object>("(ForDeletion = ?)", value));

        }
    }

    private static void ParseNSFW(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = NSFWRegex.Match(prompt);
        if (match.Success)
        {
            prompt = NSFWRegex.Replace(prompt, String.Empty);

            var value = true;

            if (match.Groups["value"].Success)
            {
                value = match.Groups["value"].Value.ToLower() == "true";
            }

            conditions.Add(new KeyValuePair<string, object>("(NSFW = ?)", value));
            return;
        }

        if (HideNFSW)
        {
            conditions.Add(new KeyValuePair<string, object>("(NSFW = ? OR NSFW IS NULL)", false));
        }

        //return false;
    }


    private static void ParseFavorite(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = FavoriteRegex.Match(prompt);
        if (match.Success)
        {
            prompt = FavoriteRegex.Replace(prompt, String.Empty);

            var value = true;

            if (match.Groups["value"].Success)
            {
                value = match.Groups["value"].Value.ToLower() == "true";
            }

            conditions.Add(new KeyValuePair<string, object>("(Favorite = ?)", value));

        }
    }

    private static void ParseHypernet(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = HypernetRegex.Match(prompt);
        if (match.Success)
        {
            prompt = HypernetRegex.Replace(prompt, String.Empty);
            var orConditions = new List<KeyValuePair<string, object>>();

            for (var i = 1; i < match.Groups.Count; i++)
            {
                if (match.Groups[i].Value.Length > 0)
                {
                    orConditions.Add(new KeyValuePair<string, object>("(HyperNetwork = ?)", match.Groups[i].Value));
                }
            }

            var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
            var values = orConditions.Select(c => c.Value);

            conditions.Add(new KeyValuePair<string, object>($"({keys})", values));

        }
    }


    private static void ParseHypernetStrength(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = HypernetStrRegex.Match(prompt);
        if (match.Success)
        {
            prompt = HypernetStrRegex.Replace(prompt, String.Empty);

            var oper = "=";

            if (match.Groups["operator"].Success)
            {
                oper = match.Groups["operator"].Value;
            }

            conditions.Add(new KeyValuePair<string, object>($"(HyperNetworkStrength {oper} ?)", decimal.Parse(match.Groups["value"].Value)));
        }
    }



    private static void ParseAestheticScore(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = AestheticScoreRegex.Match(prompt);
        if (match.Success)
        {
            prompt = AestheticScoreRegex.Replace(prompt, String.Empty);

            var oper = "=";

            if (match.Groups["operator"].Success)
            {
                oper = match.Groups["operator"].Value;
            }

            conditions.Add(new KeyValuePair<string, object>($"(AestheticScore {oper} ?)", decimal.Parse(match.Groups["value"].Value)));
        }
    }


    private static void ParseRating(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = RatingRegex.Match(prompt);
        if (match.Success)
        {
            prompt = RatingRegex.Replace(prompt, String.Empty);

            var oper = "=";

            if (match.Groups["operator"].Success)
            {
                oper = match.Groups["operator"].Value;
            }

            var value = match.Groups["value"].Value;

            if (value.ToLower() == "none")
            {
                conditions.Add(new KeyValuePair<string, object>($"(Rating IS NULL)", null));
            }
            else
            {
                conditions.Add(new KeyValuePair<string, object>($"(Rating {oper} ?)", int.Parse(value)));
            }

        }
    }

    private static void ParseNegativePrompt(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = NegativePromptRegex.Match(prompt);
        if (match.Success)
        {
            prompt = NegativePromptRegex.Replace(prompt, String.Empty);

            var value = match.Groups["value"].Value;

            var tokens = CSVParser.Parse(value);

            foreach (var token in tokens)
            {
                conditions.Add(new KeyValuePair<string, object>("(NegativePrompt LIKE ?)", $"%{token.Trim()}%"));
            }

        }


    }

    private static void ParsePrompt(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        if (prompt.Trim().Length == 0)
        {
            conditions.Add(new KeyValuePair<string, object>("(Prompt LIKE ? OR Prompt IS NULL)", "%%"));
            return;
        }

        var tokens = CSVParser.Parse(prompt);

        foreach (var token in tokens)
        {
            conditions.Add(new KeyValuePair<string, object>("(Prompt LIKE ?)", $"%{token.Trim()}%"));
        }
    }

    private static void ParseSize(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = SizeRegex.Match(prompt);
        if (match.Success)
        {
            prompt = SizeRegex.Replace(prompt, String.Empty);

            var height = match.Groups["height"].Value;
            if (NumericRegex.IsMatch(height))
            {
                conditions.Add(new KeyValuePair<string, object>("(Height = ?)", int.Parse(height)));
            }

            var width = match.Groups["width"].Value;
            if (NumericRegex.IsMatch(width))
            {
                conditions.Add(new KeyValuePair<string, object>("(Width = ?)", int.Parse(width)));
            }

        }
    }

    private static void ParseCFG(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = CfgRegex.Match(prompt);
        if (match.Success)
        {
            prompt = CfgRegex.Replace(prompt, String.Empty);
            var orConditions = new List<KeyValuePair<string, object>>();

            for (var i = 1; i < match.Groups.Count; i++)
            {
                if (match.Groups[i].Value.Length > 0)
                {
                    orConditions.Add(new KeyValuePair<string, object>("(CFGScale = ?)", float.Parse(match.Groups[i].Value)));
                }
            }

            var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
            var values = orConditions.Select(c => c.Value);

            conditions.Add(new KeyValuePair<string, object>($"({keys})", values));

        }
    }

    private static void ParseHash(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = HashRegex.Match(prompt);
        if (match.Success)
        {
            prompt = HashRegex.Replace(prompt, String.Empty);
            var orConditions = new List<KeyValuePair<string, object>>();

            for (var i = 1; i < match.Groups.Count; i++)
            {
                if (match.Groups[i].Value.Length > 0)
                {
                    orConditions.Add(new KeyValuePair<string, object>("(ModelHash = ?)", match.Groups[i].Value));
                }
            }

            var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
            var values = orConditions.Select(c => c.Value);

            conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
        }
    }

    private static void ParseSampler(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = SamplerRegex.Match(prompt);
        if (match.Success)
        {
            var start = match.Index;
            var current = start + match.Length;

            var normalizedSamplers = Samplers.Select(s => s.Trim().ToLower()).Distinct().OrderByDescending(s => s.Length).ToList();

            var samplerList = new List<string>();
            var exit = false;

            while (!exit && current < prompt.Length)
            {
                while (prompt[current] == ' ' || prompt[current] == '|')
                {
                    current++;
                }

                bool matched = false;

                foreach (var sampler in normalizedSamplers)
                {

                    if (prompt.Length - current >= sampler.Length && prompt.Substring(current, sampler.Length).ToLower() == sampler)
                    {
                        samplerList.Add(sampler);
                        current += sampler.Length;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    exit = true;
                }
            }

            prompt = prompt.Replace(prompt.Substring(start, current - start), string.Empty);

            var orConditions = new List<KeyValuePair<string, object>>();

            foreach (var sampler in samplerList)
            {
                orConditions.Add(new KeyValuePair<string, object>("(LOWER(Sampler) = LOWER(?))", sampler));
            }

            var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
            var values = orConditions.Select(c => c.Value);

            conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
        }
    }

    private static void ParseSteps(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = StepsRegex.Match(prompt);
        if (match.Success)
        {
            prompt = StepsRegex.Replace(prompt, String.Empty);

            var orConditions = new List<KeyValuePair<string, object>>();

            for (var i = 1; i < match.Groups.Count; i++)
            {
                if (match.Groups[i].Value.Length > 0)
                {
                    orConditions.Add(new KeyValuePair<string, object>("(Steps = ?)", int.Parse(match.Groups[i].Value)));
                }
            }

            var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
            var values = orConditions.Select(c => c.Value);

            conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
        }

    }

    private static void ParseSeed(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = SeedRegex.Match(prompt);
        if (match.Success)
        {
            prompt = SeedRegex.Replace(prompt, String.Empty);
            if (match.Groups["end"].Success)
            {
                conditions.Add(new KeyValuePair<string, object>("(Seed BETWEEN ? AND ?)", new object[] { long.Parse(match.Groups["start"].Value), long.Parse(match.Groups["end"].Value) }));
            }
            else
            {
                conditions.Add(new KeyValuePair<string, object>("(Seed = ?)", long.Parse(match.Groups["start"].Value)));
            }
        }
    }

    private static void ParseDate(ref string prompt, List<KeyValuePair<string, object>> conditions)
    {
        var match = DateRegex.Match(prompt);
        if (match.Success)
        {
            prompt = DateRegex.Replace(prompt, String.Empty);

            var date1 = match.Groups["date1"].Value;
            var date2 = match.Groups["date2"].Value;

            var prep1 = match.Groups["prep1"].Value;
            var prep2 = match.Groups["prep2"].Value;

            var date = ParseDate(date1);

            var q = "(CreatedDate BETWEEN ? AND ?)";

            var start = date.Date;
            var end = date.Date;

            if (!string.IsNullOrEmpty(prep1))
            {
                switch (prep1.ToLower())
                {
                    case "between":
                        if (!string.IsNullOrEmpty(prep2) && prep2.ToLower() == "and")
                        {
                            if (!string.IsNullOrEmpty(date2))
                            {
                                var endDate = ParseDate(date2);
                                end = endDate.Date;
                            }
                            else
                                throw new Exception("Expected: end date");
                        }
                        else
                            throw new Exception("Expected: and");
                        break;
                    case "from":
                        if (!string.IsNullOrEmpty(prep2) && prep2.ToLower() == "to")
                        {
                            if (!string.IsNullOrEmpty(date2))
                            {
                                var endDate = ParseDate(date2);
                                end = endDate.Date;
                            }
                            else
                                throw new Exception("Expected: end date");
                        }
                        else
                            throw new Exception("Expected: to");
                        break;
                    case "before":
                        if (string.IsNullOrEmpty(prep2) && string.IsNullOrEmpty(date2))
                        {
                            end = start;
                            start = DateTime.UnixEpoch;
                        }
                        else
                            throw new Exception($"Unexpected: {prep2}");
                        break;
                    case "since":
                        if (string.IsNullOrEmpty(prep2) && string.IsNullOrEmpty(date2))
                        {
                            end = DateTime.Now;
                        }
                        else
                            throw new Exception($"Unexpected: {prep2}");
                        break;
                }
            }

            end = end.AddDays(1).Subtract(TimeSpan.FromSeconds(1));

            if (start > end)
            {
                (start, end) = (end, start);
            }

            conditions.Add(new KeyValuePair<string, object>(q, new object[] { start, end }));
        }
    }

    private static DateTime ParseDate(string text)
    {
        switch (text.ToLower())
        {
            case "today":
                return DateTime.Now;
            case "yesterday":
                return DateTime.Now.Subtract(TimeSpan.FromDays(1));
            default:
                var date = text.ToLower();
                var dateMatch = DateFormatRegex.Match(date);
                if (dateMatch.Success)
                {
                    return DateTime.Parse(date);
                }
                throw new Exception($"Unknown date format {text}");
        }
    }
}
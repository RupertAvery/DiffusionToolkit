using System.Globalization;
using System.Text.RegularExpressions;

namespace Diffusion.Database;

public static partial class FilterBuilder
{
    public static readonly Regex DayRegex = new Regex("\\d+ day(?:s)? ago", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static readonly Regex MonthRegex = new Regex("(?:a|1|2|3) week(?:s)? ago", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex DateFormatRegex = new Regex("\\d{1,2}[-/]\\d{1,2}[-/]\\d{4}|\\d{4}[-/]\\d{1,2}[-/]\\d{1,2}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex AlbumRegex = new Regex("\\balbum:\\s*(?:\"(?<value>[^\"]+)\"|(?<value>\\S+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex FolderRegex = new Regex("\\bfolder:\\s*(?:\"(?<value>[^\"]+)\"|(?<value>\\S+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PathRegex = new Regex("\\bpath:\\s*(?:(?<criteria>starts with|contains|ends with)\\s+)?(?:\"(?<value>[^\"]+)\"|(?<value>\\S+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DateRegex = new Regex("\\bdate:\\s*(?:(?<prep1>between|before|since|from)\\s+)?(?<date1>today|yesterday|\\d+ day(?:s)? ago|(?:a|1|2|3) week(?:s)? ago|(?:a|\\d{1,2}) month(?:s)? ago|\\d{1,2}[-/]\\d{1,2}[-/]\\d{4}|\\d{4}[-/]\\d{1,2}[-/]\\d{1,2})(?:\\s+(?<prep2>and|up to|to)\\s+(?<date2>today|yesterday|\\d+ day(?:s)? ago|(?:a|1|2|3) week(?:s)? ago|(?:a|\\d{1,2}) month(?:s)? ago|\\d{1,2}[-/]\\d{1,2}[-/]\\d{4}|\\d{4}[-/]\\d{1,2}[-/]\\d{1,2}))?\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SeedRegex = new Regex("\\bseed:\\s*(?<start>[0-9?*]+)(?:\\s*-\\s*(?<end>\\S+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex StepsRegex = new Regex("\\bsteps:\\s*(\\d+)(?:\\|(\\d+))*\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SamplerRegex = new Regex("sampler:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ModelNameRegex = new Regex("\\bmodel:\\s*(?:\"(?<value>[^\"]+)\"|(?<value>\\S+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ModelNameOrHashRegex = new Regex("\\bmodel_or_hash:\\s*(?:\"(?<name>[^\"]*)\"\\|(?<hash>[0-9a-f]*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
    private static readonly Regex InAlbumRegex = new Regex("\\bin_album:\\s*(?<value>(?:true|false))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex NSFWRegex = new Regex("\\b(?:nsfw):\\s*(?<value>(?:true|false))?\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NoMetadataRegex = new Regex("\\b(?:nometa|nometadata):\\s*(?<value>(?:true|false))?\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex NegativePromptRegex = new Regex("\\b(?:negative prompt|negative_prompt|negative):\\s*(?<value>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);


    public static Filter ParseFilter(string? prompt)
    {
        var filter = new Filter();

        if (prompt == null) return filter;

        ParseAlbum(ref prompt, filter);
        ParseFolder(ref prompt, filter);
        ParsePath(ref prompt, filter);
        ParseDate(ref prompt, filter);
        ParseSeed(ref prompt, filter);
        ParseSteps(ref prompt, filter);
        //ParseSampler(ref prompt, filter);
        //ParseHash(ref prompt, filter);

        //ParseModelName(ref prompt, filter);
        //ParseModelNameOrHash(ref prompt, filter);

        //ParseCFG(ref prompt, filter);
        //ParseSize(ref prompt, filter);
        //ParseAestheticScore(ref prompt, filter);
        ParseRating(ref prompt, filter);
        //ParseHypernet(ref prompt, filter);
        //ParseHypernetStrength(ref prompt, filter);
        //ParseFavorite(ref prompt, filter);
        ParseForDeletion(ref prompt, filter);
        ParseNSFW(ref prompt, filter);
        //ParseInAlbum(ref prompt, filter);
        //ParseNoMetadata(ref prompt, filter);

        ParseNegativePrompt(ref prompt, filter);
        ParsePrompt(ref prompt, filter);

        return filter;
    }

    private static void ParseAlbum(ref string prompt, Filter filter)
    {
        var match = AlbumRegex.Match(prompt);

        if (match.Success)
        {
            prompt = AlbumRegex.Replace(prompt, String.Empty);

            filter.Album = match.Groups["value"].Value;
            //filter.UseAlbum = true;
        }
    }

    private static void ParseFolder(ref string prompt, Filter filter)
    {
        var match = FolderRegex.Match(prompt);

        if (match.Success)
        {
            prompt = FolderRegex.Replace(prompt, String.Empty);

            filter.Folder = match.Groups["value"].Value;
            //filter.UseFolder = true;
        }
    }

    private static void ParsePath(ref string prompt, Filter filter)
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
                        value = $"{value}*";
                        break;
                    case "contains":
                        value = $"*{value}*";
                        break;
                    case "ends with":
                        value = $"*{value}";
                        break;
                }
            }

            filter.Path = value;
            filter.UsePath = true;
        }
    }

    private static void ParseForDeletion(ref string prompt, Filter filter)
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

            filter.ForDeletion = value;
            filter.UseForDeletion = true;
        }
    }

    private static void ParseNSFW(ref string prompt, Filter filter)
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

            filter.NSFW = value;
            filter.UseNSFW = true;

            return;
        }

        if (QueryBuilder.HideNSFW)
        {
            filter.NSFW = false;
            filter.UseNSFW = true;

            return;
        }

    }

    private static void ParseInAlbum(ref string prompt, Filter filter)
    {
        var match = InAlbumRegex.Match(prompt);
        if (match.Success)
        {
            prompt = InAlbumRegex.Replace(prompt, String.Empty);

            var value = true;

            if (match.Groups["value"].Success)
            {
                value = match.Groups["value"].Value.ToLower() == "true";
            }

            if (value)
            {
                //conditions.Add(new KeyValuePair<string, object>("(AlbumImage.ImageId IS NOT NULL)", null));
                //joins.Add("LEFT OUTER JOIN AlbumImage ON AlbumImage.ImageId = Image.Id");
            }
            else
            {
                //conditions.Add(new KeyValuePair<string, object>("(AlbumImage.ImageId IS NULL)", null));
                //joins.Add("LEFT OUTER JOIN AlbumImage ON AlbumImage.ImageId = Image.Id");
            }
        }



        //return false;
    }

    private static void ParseNoMetadata(ref string prompt, Filter filter)
    {
        var match = NoMetadataRegex.Match(prompt);

        if (match.Success)
        {
            prompt = NoMetadataRegex.Replace(prompt, String.Empty);

            var value = true;

            if (match.Groups["value"].Success)
            {
                value = match.Groups["value"].Value.ToLower() == "true";
            }

            filter.NoMetadata = value;
            filter.UseNoMetadata = true;
        }
    }

    private static void ParseFavorite(ref string prompt, Filter filter)
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

            filter.Favorite = value;
            filter.UseFavorite = true;
        }
    }

    private static void ParseHypernet(ref string prompt, Filter filter)
    {
        var match = HypernetRegex.Match(prompt);
        if (match.Success)
        {
            prompt = HypernetRegex.Replace(prompt, String.Empty);

            filter.HyperNet = match.Groups[0].Value;
            filter.UseHyperNet = true;
        }
    }


    private static void ParseHypernetStrength(ref string prompt, Filter filter)
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

            filter.HyperNetStr = double.Parse(match.Groups["value"].Value);
            filter.HyperNetStrOp = oper;
            filter.UseHyperNetStr = true;
        }
    }



    private static void ParseAestheticScore(ref string prompt, Filter filter)
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

            filter.AestheticScore = double.Parse(match.Groups["value"].Value);
            filter.AestheticScoreOp = oper;
            filter.UseAestheticScore = true;
        }
    }


    private static void ParseRating(ref string prompt, Filter filter)
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
                filter.Rating = null;
                filter.UseRating = true;
            }
            else
            {
                filter.Rating = int.Parse(value, CultureInfo.InvariantCulture);
                filter.UseRating = true;
            }

        }
    }

    private static void ParseNegativePrompt(ref string prompt, Filter filter)
    {
        var match = NegativePromptRegex.Match(prompt);
        if (match.Success)
        {
            prompt = NegativePromptRegex.Replace(prompt, String.Empty);

            var value = match.Groups["value"].Value;

            filter.NegativePrompt = value;
            filter.UseNegativePrompt = true;
        }
    }

    private static void ParsePrompt(ref string prompt, Filter filter)
    {
        if (prompt.Trim().Length == 0)
        {
            filter.Prompt = null;
            return;
        }

        filter.Prompt = prompt;
        filter.UsePrompt = true;
    }

    private static void ParseSize(ref string prompt, Filter filter)
    {
        var match = SizeRegex.Match(prompt);
        if (match.Success)
        {
            prompt = SizeRegex.Replace(prompt, String.Empty);

            var height = match.Groups["height"].Value;
            if (NumericRegex.IsMatch(height))
            {
                filter.Height = height;
                filter.UseSize = true;
            }

            var width = match.Groups["width"].Value;
            if (NumericRegex.IsMatch(width))
            {
                filter.Width = width;
                filter.UseSize = true;
            }

        }
    }

    private static void ParseCFG(ref string prompt, Filter filter)
    {
        throw new NotImplementedException();
        var match = CfgRegex.Match(prompt);
        if (match.Success)
        {
            prompt = CfgRegex.Replace(prompt, String.Empty);

            filter.CFGScale = match.Groups[0].Value;
            filter.UseCFGScale = true;
        }
    }

    private static void ParseHash(ref string prompt, Filter filter)
    {
        throw new NotImplementedException();
        var match = HashRegex.Match(prompt);
        if (match.Success)
        {
            prompt = HashRegex.Replace(prompt, String.Empty);

            filter.ModelHash = match.Groups[0].Value;
            filter.UseModelHash = true;
        }
    }

    private static void ParseModelName(ref string prompt, Filter filter)
    {
        throw new NotImplementedException();
        var match = ModelNameRegex.Match(prompt);
        if (match.Success)
        {
            prompt = ModelNameRegex.Replace(prompt, String.Empty);
            filter.ModelName = match.Groups[1].Value;
            filter.UseModelName = true;
        }
    }


    //private static void ParseModelNameOrHash(ref string prompt, Filter filter)
    //{
    //    var match = ModelNameOrHashRegex.Match(prompt);
    //    if (match.Success)
    //    {
    //        prompt = ModelNameOrHashRegex.Replace(prompt, String.Empty);
    //    }
    //}

    private static void ParseSampler(ref string prompt, Filter filter)
    {
        throw new NotImplementedException();

        //var match = SamplerRegex.Match(prompt);
        //if (match.Success)
        //{
        //    var start = match.Index;
        //    var current = start + match.Length;

        //    var normalizedSamplers = Samplers.Select(s => s.Trim().ToLower()).Distinct().OrderByDescending(s => s.Length).ToList();

        //    var samplerList = new List<string>();
        //    var exit = false;

        //    while (!exit && current < prompt.Length)
        //    {
        //        while (prompt[current] == ' ' || prompt[current] == '|')
        //        {
        //            current++;
        //        }

        //        bool matched = false;

        //        foreach (var sampler in normalizedSamplers)
        //        {

        //            if (prompt.Length - current >= sampler.Length && prompt.Substring(current, sampler.Length).ToLower() == sampler)
        //            {
        //                samplerList.Add(sampler);
        //                current += sampler.Length;
        //                matched = true;
        //                break;
        //            }
        //        }

        //        if (!matched)
        //        {
        //            exit = true;
        //        }
        //    }

        //    prompt = prompt.Replace(prompt.Substring(start, current - start), string.Empty);

        //    var orConditions = new List<KeyValuePair<string, object>>();

        //    foreach (var sampler in samplerList)
        //    {
        //        orConditions.Add(new KeyValuePair<string, object>("(LOWER(Sampler) = LOWER(?))", sampler));
        //    }

        //    var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
        //    var values = orConditions.Select(c => c.Value);

        //    conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
        //}
    }

    private static void ParseSteps(ref string prompt, Filter filter)
    {
        var match = StepsRegex.Match(prompt);
        if (match.Success)
        {
            prompt = StepsRegex.Replace(prompt, String.Empty);

            filter.Steps = match.Groups[0].Value;
            filter.UseSteps = true;
        }

    }

    private static void ParseSeed(ref string prompt, Filter filter)
    {
        var match = SeedRegex.Match(prompt);
        if (match.Success)
        {
            prompt = SeedRegex.Replace(prompt, String.Empty);

            filter.UseSeed  = true;

            if (match.Groups["end"].Success)
            {
                filter.SeedEnd = match.Groups["end"].Value;
                filter.SeedStart = match.Groups["start"].Value;
            }
            else
            {
                filter.SeedStart = match.Groups["start"].Value;
            }
        }
    }

    private static void ParseDate(ref string prompt, Filter filter)
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

            filter.Start = start;
            filter.End = end;
            filter.UseCreationDate = true;
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
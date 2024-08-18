using System.Globalization;

namespace Diffusion.Database
{
    public static partial class QueryBuilder
    {
        private static ICollection<Common.Model> _models;

        public static void SetModels(ICollection<Common.Model> models)
        {
            _models = models;
        }

        public static (string WhereClause, IEnumerable<object> Bindings, IEnumerable<string> Joins) Filter(Filter filter)
        {
            var conditions = new List<KeyValuePair<string, object>>();
            var joins = new List<string>();

            if (HideNSFW && !filter.UseNSFW)
            {
                conditions.Add(new KeyValuePair<string, object>("(NSFW = ? OR NSFW IS NULL)", false));
                filter.UseNSFW = false;
            }

            if (HideDeleted && !filter.UseForDeletion)
            {
                conditions.Add(new KeyValuePair<string, object>("(ForDeletion = ?)", false));
                filter.UseForDeletion = false;
            }

            if (HideUnavailable && !filter.UseUnavailable)
            {
                conditions.Add(new KeyValuePair<string, object>("(Unavailable = ?)", false));
                filter.UseUnavailable = false;
            }

            FilterAlbum(filter, conditions, joins);
            FilterFolder(filter, conditions, joins);
            FilterPath(filter, conditions);
            FilterDate(filter, conditions);
            FilterSeed(filter, conditions);
            FilterSteps(filter, conditions);
            FilterSampler(filter, conditions);
            FilterHash(filter, conditions);
            FilterModelName(filter, conditions);
            FilterCFG(filter, conditions);
            FilterSize(filter, conditions);
            FilterAestheticScore(filter, conditions);
            FilterRating(filter, conditions);
            FilterHypernet(filter, conditions);
            FilterHypernetStrength(filter, conditions);
            FilterFavorite(filter, conditions);
            FilterForDeletion(filter, conditions);
            FilterNSFW(filter, conditions);
            FilterNoMetadata(filter, conditions);
            FilterInAlbum(filter, conditions);
            FilterUnavailable(filter, conditions);

            FilterNegativePrompt(filter, conditions);
            FilterPrompt(filter, conditions);

            FilterNegativePromptEx(filter, conditions);
            FilterPromptEx(filter, conditions);


            return (string.Join(" AND ", conditions.Select(c => c.Key)),
                conditions.SelectMany(c =>
                {
                    return c.Value switch
                    {
                        IEnumerable<object> orConditions => orConditions.Select(o => o),
                        _ => new[] { c.Value }
                    };
                }).Where(o => o != null),
                joins);
        }


        private static void FilterFolder(Filter filter, List<KeyValuePair<string, object>> conditions, List<string> joins)
        {
            if (filter.UseFolder)
            {
                var value = filter.Folder;
                conditions.Add(new KeyValuePair<string, object>("(Folder.Path = ?)", value));
                joins.Add("INNER JOIN Folder ON Folder.Id = m1.FolderId");
            }
        }


        private static void FilterAlbum(Filter filter, List<KeyValuePair<string, object>> conditions, List<string> joins)
        {

            if (filter.UseAlbum)
            {
                var value = filter.Album;
                conditions.Add(new KeyValuePair<string, object>("(Album.Name = ?)", value));
                joins.Add("INNER JOIN AlbumImage ON m1.Id = AlbumImage.ImageId");
                joins.Add("INNER JOIN Album ON AlbumImage.AlbumId = Album.Id");
            }
        }

        private static void FilterPath(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UsePath)
            {
                var value = filter.Path;

                conditions.Add(new KeyValuePair<string, object>("(m1.Path LIKE ?)", value.Replace("*", "%")));

            }
        }

        private static void FilterForDeletion(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseForDeletion)
            {
                var value = filter.ForDeletion;

                conditions.Add(new KeyValuePair<string, object>("(ForDeletion = ?)", value));

            }
        }

        private static void FilterNSFW(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseNSFW)
            {
                var value = filter.NSFW;

                conditions.Add(new KeyValuePair<string, object>("(NSFW = ?)", value));
                return;
            }

            if (HideNSFW)
            {
                conditions.Add(new KeyValuePair<string, object>("(NSFW = ? OR NSFW IS NULL)", false));
            }

            //return false;
        }

        private static void FilterNoMetadata(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseNoMetadata)
            {
                var value = filter.NoMetadata;

                conditions.Add(new KeyValuePair<string, object>("(NoMetadata = ?)", value));
            }
        }

        private static void FilterFavorite(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseFavorite)
            {
                var value = filter.Favorite;

                conditions.Add(new KeyValuePair<string, object>("(Favorite = ?)", value));

            }
        }

        private static void FilterHypernet(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseHyperNet)
            {
                var match = HypernetRegex.Match("hypernet: " + filter.HyperNet);

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


        private static void FilterHypernetStrength(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseHyperNetStr)
            {
                var oper = filter.HyperNetStrOp;

                conditions.Add(new KeyValuePair<string, object>($"(HyperNetworkStrength {oper} ?)", filter.HyperNetStr));
            }
        }

        private static void FilterAestheticScore(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseAestheticScore)
            {
                if (filter.NoAestheticScore)
                {
                    conditions.Add(new KeyValuePair<string, object>($"(AestheticScore IS NULL)", null));
                }
                else
                {
                    var oper = filter.AestheticScoreOp;
                    if (oper is { Length: > 0 } && filter.AestheticScore.HasValue)
                    {
                        conditions.Add(new KeyValuePair<string, object>($"(AestheticScore {oper} ?)", filter.AestheticScore));
                    }
                }
            }
        }


        private static void FilterRating(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseRating)
            {
                if (filter.Unrated)
                {
                    conditions.Add(new KeyValuePair<string, object>($"(Rating IS NULL)", null));
                }
                else
                {
                    var oper = filter.RatingOp;
                    if (oper is { Length: > 0 } && filter.Rating.HasValue)
                    {
                        conditions.Add(new KeyValuePair<string, object>($"(Rating {oper} ?)", filter.Rating));
                    }
                }
            }
        }

        private static void FilterNegativePrompt(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseNegativePrompt)
            {
                var value = filter.NegativePrompt;

                var tokens = CSVParser.Parse(value);

                foreach (var token in tokens)
                {
                    conditions.Add(new KeyValuePair<string, object>("(NegativePrompt LIKE ?)", $"%{token.Trim()}%"));
                }

            }
        }

        private static void FilterNegativePromptEx(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseNegativePromptEx)
            {
                var value = filter.NegativePromptEx;

                var tokens = CSVParser.Parse(value);

                foreach (var token in tokens)
                {
                    conditions.Add(new KeyValuePair<string, object>("(NegativePrompt NOT LIKE ?)", $"%{token.Trim()}%"));
                }

            }
        }

        private static void FilterPrompt(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UsePrompt)
            {
                if (filter.Prompt.Trim().Length == 0)
                {
                    conditions.Add(new KeyValuePair<string, object>("(Prompt LIKE ? OR Prompt IS NULL)", "%%"));
                    return;
                }

                var tokens = CSVParser.Parse(filter.Prompt);

                foreach (var token in tokens)
                {
                    conditions.Add(new KeyValuePair<string, object>("(Prompt LIKE ?)", $"%{token.Trim()}%"));
                }
            }
        }

        private static void FilterPromptEx(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UsePromptEx)
            {
                //if (filter.PromptEx.Trim().Length == 0)
                //{
                //    conditions.Add(new KeyValuePair<string, object>("(Prompt NOT LIKE ? OR Prompt IS NULL)", "%%"));
                //    return;
                //}

                var tokens = CSVParser.Parse(filter.PromptEx);

                foreach (var token in tokens)
                {
                    conditions.Add(new KeyValuePair<string, object>("(Prompt NOT LIKE ?)", $"%{token.Trim()}%"));
                }
            }
        }

        private static void FilterSize(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseSize)
            {
                var height = filter.Height;
                if (NumericRegex.IsMatch(height))
                {
                    conditions.Add(new KeyValuePair<string, object>("(Height = ?)", int.Parse(height, CultureInfo.InvariantCulture)));
                }

                var width = filter.Width;
                if (NumericRegex.IsMatch(width))
                {
                    conditions.Add(new KeyValuePair<string, object>("(Width = ?)", int.Parse(width, CultureInfo.InvariantCulture)));
                }

            }
        }

        private static void FilterCFG(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseCFGScale)
            {
                var orConditions = new List<KeyValuePair<string, object>>();

                var match = CfgRegex.Match("cfg: " + filter.CFGScale);

                for (var i = 1; i < match.Groups.Count; i++)
                {
                    if (match.Groups[i].Value.Length > 0)
                    {
                        orConditions.Add(new KeyValuePair<string, object>("(CFGScale = ?)", float.Parse(match.Groups[i].Value, CultureInfo.InvariantCulture)));
                    }
                }

                var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
                var values = orConditions.Select(c => c.Value);

                conditions.Add(new KeyValuePair<string, object>($"({keys})", values));

            }
        }

        private static void FilterHash(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseModelHash)
            {
                var orConditions = new List<KeyValuePair<string, object>>();

                var match = HashRegex.Match("model_hash: " + filter.ModelHash);

                for (var i = 1; i < match.Groups.Count; i++)
                {
                    if (match.Groups[i].Value.Length > 0)
                    {
                        orConditions.Add(new KeyValuePair<string, object>("(ModelHash = ? COLLATE NOCASE)", match.Groups[i].Value));
                    }
                }

                var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
                var values = orConditions.Select(c => c.Value);

                conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
            }
        }

        private static void FilterModelName(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseModelName && !string.IsNullOrEmpty(filter.ModelName) && _models != null && _models.Any())
            {
                var orConditions = new List<KeyValuePair<string, object>>();

                var names = filter.ModelName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var name in names)
                {
                    foreach (var model in _models)
                    {
                        if (model.Filename.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            orConditions.Add(new KeyValuePair<string, object>("(ModelHash = ? COLLATE NOCASE)", model.Hash));
                            if (!string.IsNullOrEmpty(model.SHA256))
                            {
                                orConditions.Add(new KeyValuePair<string, object>("(ModelHash = ? COLLATE NOCASE)", model.SHA256.Substring(0, 10)));
                            }
                        }
                    }

                    orConditions.Add(new KeyValuePair<string, object>("(Model LIKE ?)", name.Replace("*", "%")));
                }

                if (orConditions.Any())
                {
                    var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
                    var values = orConditions.Select(c => c.Value);

                    conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
                }
                else
                {
                    conditions.Add(new KeyValuePair<string, object>($"(0 == 1)", null));
                }
            }
        }

        private static void FilterSampler(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseSampler && !string.IsNullOrEmpty(filter.Sampler))
            {
                var samplerList = filter.Sampler.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

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

        private static void FilterSteps(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseSteps)
            {
                var match = StepsRegex.Match("steps: " + filter.Steps);


                var orConditions = new List<KeyValuePair<string, object>>();

                for (var i = 1; i < match.Groups.Count; i++)
                {
                    if (match.Groups[i].Value.Length > 0)
                    {
                        orConditions.Add(new KeyValuePair<string, object>("(Steps = ?)", int.Parse(match.Groups[i].Value, CultureInfo.InvariantCulture)));
                    }
                }

                var keys = string.Join(" OR ", orConditions.Select(c => c.Key));
                var values = orConditions.Select(c => c.Value);

                conditions.Add(new KeyValuePair<string, object>($"({keys})", values));
            }

        }

        private static void FilterSeed(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseSeed)
            {
                if (!string.IsNullOrEmpty(filter.SeedEnd))
                {
                    conditions.Add(new KeyValuePair<string, object>("(Seed BETWEEN ? AND ?)", new object[] { filter.SeedStart, filter.SeedEnd }));
                }
                else
                {

                    if (filter.SeedStart.Contains("?") || filter.SeedStart.Contains("*"))
                    {
                        conditions.Add(new KeyValuePair<string, object>("(Seed LIKE ?)", filter.SeedStart.Replace("*", "%")));
                        return;
                    }

                    conditions.Add(new KeyValuePair<string, object>("(Seed = ?)", filter.SeedStart));
                }
            }
        }

        private static void FilterInAlbum(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseInAlbum)
            {
                var value = filter.InAlbum;

                if (value)
                {
                    conditions.Add(new KeyValuePair<string, object>("(SELECT COUNT(1) FROM AlbumImage WHERE ImageId = m1.Id) > 0", null));
                }
                else
                {
                    conditions.Add(new KeyValuePair<string, object>("(SELECT COUNT(1) FROM AlbumImage WHERE ImageId = m1.Id) = 0", null));
                }
            }
        }

        private static void FilterUnavailable(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseUnavailable)
            {
                var value = filter.Unavailable;

                if (value)
                {
                    conditions.Add(new KeyValuePair<string, object>("(Unavailable = 1)", null));
                }
                else
                {
                    conditions.Add(new KeyValuePair<string, object>("(Unavailable = 0 OR Unavailable IS NULL)", null));
                }
            }
        }

        private static void FilterDate(Filter filter, List<KeyValuePair<string, object>> conditions)
        {
            if (filter.UseCreationDate)
            {
                var q = "(CreatedDate BETWEEN ? AND ?)";

                var start = filter.Start;
                var end = filter.End;

                //if (!string.IsNullOrEmpty(prep1))
                //{
                //    switch (prep1.ToLower())
                //    {
                //        case "between":
                //            if (!string.IsNullOrEmpty(prep2) && prep2.ToLower() == "and")
                //            {
                //                if (!string.IsNullOrEmpty(date2))
                //                {
                //                    var endDate = FilterDate(date2);
                //                    end = endDate.Date;
                //                }
                //                else
                //                    throw new Exception("Expected: end date");
                //            }
                //            else
                //                throw new Exception("Expected: and");
                //            break;
                //        case "from":
                //            if (!string.IsNullOrEmpty(prep2) && prep2.ToLower() == "to")
                //            {
                //                if (!string.IsNullOrEmpty(date2))
                //                {
                //                    var endDate = FilterDate(date2);
                //                    end = endDate.Date;
                //                }
                //                else
                //                    throw new Exception("Expected: end date");
                //            }
                //            else
                //                throw new Exception("Expected: to");
                //            break;
                //        case "before":
                //            if (string.IsNullOrEmpty(prep2) && string.IsNullOrEmpty(date2))
                //            {
                //                end = start;
                //                start = DateTime.UnixEpoch;
                //            }
                //            else
                //                throw new Exception($"Unexpected: {prep2}");
                //            break;
                //        case "since":
                //            if (string.IsNullOrEmpty(prep2) && string.IsNullOrEmpty(date2))
                //            {
                //                end = DateTime.Now;
                //            }
                //            else
                //                throw new Exception($"Unexpected: {prep2}");
                //            break;
                //    }
                //}
                if (!start.HasValue)
                {
                    start = DateTime.Now;
                }

                if (end.HasValue)
                {
                    end = end.Value.AddDays(1).Subtract(TimeSpan.FromSeconds(1));
                }
                else
                {
                    end = DateTime.Now;
                }


                if (start > end)
                {
                    (start, end) = (end, start);
                }

                conditions.Add(new KeyValuePair<string, object>(q, new object[] { start, end }));
            }
        }

        //private static DateTime FilterDate(string text)
        //{
        //    switch (text.ToLower())
        //    {
        //        case "today":
        //            return DateTime.Now;
        //        case "yesterday":
        //            return DateTime.Now.Subtract(TimeSpan.FromDays(1));
        //        default:
        //            var date = text.ToLower();
        //            var dateMatch = DateFormatRegex.Match(date);
        //            if (dateMatch.Success)
        //            {
        //                return DateTime.Filter(date);
        //            }
        //            throw new Exception($"Unknown date format {text}");
        //    }
        //}
    }
}

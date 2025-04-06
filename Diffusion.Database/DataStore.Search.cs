using Diffusion.Common;
using Diffusion.Database.Models;
using System;

namespace Diffusion.Database
{
    public class Paging
    {
        public int PageSize { get; set; }
        public int Offset { get; set; }
    }

    public class Sorting
    {
        public string SortBy { get; set; }
        public string SortDirection { get; set; }

        public Sorting(string sortBy, string sortDirection)
        {
            SortBy = sortBy switch
            {
                "Date Created" => nameof(Image.CreatedDate),
                "Date Modified" => nameof(Image.ModifiedDate),
                "Rating" => nameof(Image.Rating),
                "Aesthetic Score" => nameof(Image.AestheticScore),
                "Prompt" => nameof(Image.Prompt),
                "Random" => "RANDOM()",
                "Name" => nameof(Image.FileName),
                _ => nameof(Image.CreatedDate),
            };

            SortDirection = sortDirection switch
            {
                "A-Z" => "ASC",
                "Z-A" => "DESC",
                _ => "DESC",
            };
        }

        public void Deconstruct(out string sortBy, out string sortDir)
        {
            sortBy = SortBy;
            sortDir = SortDirection;
        }
    }

    public partial class DataStore
    {
        public IEnumerable<ModelView> GetImageModels()
        {
            using var db = OpenConnection();

            string whereClause = GetInitialWhereClause();

            var query = $"SELECT Model AS Name, ModelHash AS Hash, COUNT(*) AS ImageCount FROM Image {whereClause} GROUP BY Model, ModelHash";

            var models = db.Query<ModelView>(query);

            db.Close();

            return models;
        }

        public int GetTotal()
        {
            using var db = OpenConnection();

            var query = "SELECT COUNT(*) FROM Image";

            var count = db.ExecuteScalar<int>(query + GetInitialWhereClause());

            db.Close();

            return count;
        }

        public long CountFileSize(QueryOptions queryOptions)
        {
            using var db = OpenConnection();

            var q = QueryCombiner.Parse(queryOptions);

            var size = db.ExecuteScalar<long>($"SELECT SUM(FileSize) FROM Image main INNER JOIN ({q.Query}) sub on sub.Id = main.Id", q.Bindings.ToArray());

            db.Close();

            return size;
        }

        public long CountPromptFileSize(string prompt)
        {
            using var db = OpenConnection();

            //if (string.IsNullOrEmpty(prompt))
            //{
            //    var query = $"SELECT SUM(FileSize) FROM Image";

            //    if (QueryBuilder.HideNFSW)
            //    {
            //        query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
            //    }

            //    var allcount = db.ExecuteScalar<long>(query);
            //    return allcount;
            //}

            var q = QueryBuilder.QueryPrompt(prompt);

            var size = db.ExecuteScalar<long>($"SELECT SUM(FileSize) FROM Image m1 {string.Join(' ', q.Joins)} WHERE {q.WhereClause}", q.Bindings.ToArray());

            db.Close();

            return size;
        }




        private string GetInitialWhereClause(bool forceDeleted = false)
        {

            var whereClauses = new List<string>();

            if (QueryBuilder.HideNSFW)
            {
                whereClauses.Add("(NSFW = 0 OR NSFW IS NULL)");
            }

            if (forceDeleted)
            {
                whereClauses.Add("ForDeletion = 1");
            }
            else if (QueryBuilder.HideDeleted)
            {
                whereClauses.Add("ForDeletion = 0");
            }

            if (QueryBuilder.HideUnavailable)
            {
                whereClauses.Add("(Unavailable = 0)");
            }

            var whereExpression = string.Join(" AND ", whereClauses);

            return whereClauses.Any() ? $" WHERE {whereExpression}" : "";
        }

        public CountSize CountAndSize(QueryOptions options)
        {
            var db = OpenReadonlyConnection();

            var q = QueryCombiner.Parse(options);

            var whereClause = QueryCombiner.GetInitialWhereClause("main", options);

            var join = $"INNER JOIN ({q.Query}) sub ON main.Id = sub.Id";

            var where = whereClause.Length > 0 ? $"WHERE {whereClause}" : "";

            var countSize = db.Query<CountSize>($"SELECT COUNT(*) AS Total, COALESCE(SUM(FileSize),0) AS Size FROM Image main {join} {where}", q.Bindings.ToArray());

            return countSize[0];
        }

        public int CountPrompt(string prompt)
        {
            using var db = OpenConnection();

            //if (string.IsNullOrEmpty(prompt))
            //{
            //    var query = "SELECT COUNT(*) FROM Image";

            //    if (QueryBuilder.HideNFSW)
            //    {
            //        query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
            //    }

            //    var allcount = db.ExecuteScalar<int>(query);
            //    return allcount;
            //}

            var q = QueryBuilder.QueryPrompt(prompt);

            var count = db.ExecuteScalar<int>($"SELECT COUNT(*) FROM Image m1 {string.Join(' ', q.Joins)} WHERE {q.WhereClause}", q.Bindings.ToArray());

            db.Close();

            return count;
        }

        public CountSize CountAndFileSizeEx(QueryOptions options)
        {
            var db = OpenReadonlyConnection();

            var q = QueryCombiner.ParseEx(options);

            var whereClause = QueryCombiner.GetInitialWhereClause("main", options);

            var join = $"INNER JOIN ({q.Query}) sub ON main.Id = sub.Id";

            var where = whereClause.Length > 0 ? $"WHERE {whereClause}" : "";

            var countSize = db.Query<CountSize>($"SELECT COUNT(*) AS Total, SUM(FileSize) AS Size FROM Image main {join} {where}", q.Bindings.ToArray());

            return countSize[0];
        }

        public long CountFileSize(Filter filter)
        {
            using var db = OpenConnection();

            //if (filter.IsEmpty)
            //{
            //    var query = $"SELECT SUM(FileSize) FROM Image";

            //    var allcount = db.ExecuteScalar<long>(query + GetInitialWhereClause(filter.UseForDeletion && filter.ForDeletion));

            //    return allcount;
            //}

            var q = QueryBuilder.Filter(filter);

            var size = db.ExecuteScalar<long>($"SELECT SUM(FileSize) FROM Image m1 {string.Join(' ', q.Joins)} WHERE {q.WhereClause}", q.Bindings.ToArray());

            db.Close();

            return size;
        }

        public int Count(Filter filter)
        {
            using var db = OpenConnection();

            if (filter.IsEmpty)
            {
                var query = "SELECT COUNT(*) FROM Image";

                var allcount = db.ExecuteScalar<int>(query + GetInitialWhereClause());

                return allcount;
            }

            var q = QueryBuilder.Filter(filter);

            string raw_query = $"SELECT COUNT(*) FROM Image m1 {string.Join(' ', q.Joins)} WHERE {q.Item1}";
            var count = db.ExecuteScalar<int>(raw_query, q.Item2.ToArray());

            db.Close();

            return count;
        }



        public Image GetImage(int id)
        {
            using var db = OpenConnection();


            var image = db.FindWithQuery<Image>($"SELECT Image.* FROM Image WHERE Id = ?", id);

            db.Close();

            return image;
        }

        public IEnumerable<Album> GetImageAlbums(int id)
        {
            using var db = OpenConnection();

            var albums = db.Query<Album>($"SELECT Album.* FROM Image INNER JOIN AlbumImage ON AlbumImage.ImageId = Image.Id INNER JOIN Album ON AlbumImage.AlbumId = Album.Id WHERE Image.Id = ?", id);

            db.Close();

            return albums;
        }

        public IEnumerable<Album> GetImageAlbums(IEnumerable<int> ids)
        {
            using var db = OpenConnection();

            InsertIds(db, "SelectedIds", ids);

            var albums = db.Query<Album>($"SELECT DISTINCT Album.* FROM Image INNER JOIN AlbumImage ON AlbumImage.ImageId = Image.Id INNER JOIN Album ON AlbumImage.AlbumId = Album.Id WHERE Image.Id IN (SELECT Id FROM SelectedIds)");

            db.Close();

            return albums;
        }


        public IEnumerable<Image> QueryAll()
        {
            using var db = OpenConnection();

            var query = $"SELECT Image.* FROM Image";

            var images = db.Query<Image>(query + GetInitialWhereClause());

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }


        public IEnumerable<ImageView> SearchPrompt(string? prompt, int pageSize, int offset, string sortBy, string sortDirection)
        {
            using var db = OpenConnection();

            var sortField = sortBy switch
            {
                "Date Created" => nameof(Image.CreatedDate),
                "Rating" => nameof(Image.Rating),
                "Aesthetic Score" => nameof(Image.AestheticScore),
                "Prompt" => nameof(Image.Prompt),
                "Random" => "RANDOM()",
                _ => nameof(Image.CreatedDate),
            };

            var sortDir = sortDirection switch
            {
                "A-Z" => "ASC",
                "Z-A" => "DESC",
                _ => "DESC",
            };

            //if (string.IsNullOrEmpty(prompt))
            //{
            //    var query = "SELECT Image.* FROM Image ";

            //    if (QueryBuilder.HideNFSW)
            //    {
            //        query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
            //    }

            //    var allimages = db.Query<Image>($"{query} ORDER BY {sortField} {sortDir} LIMIT ? OFFSET ?", pageSize, offset);

            //    foreach (var image in allimages)
            //    {
            //        yield return image;
            //    }

            //    db.Close();

            //    yield break;
            //}

            //SELECT foo, bar, baz, quux FROM table
            //WHERE oid NOT IN(SELECT oid FROM table
            //ORDER BY title ASC LIMIT 50 )
            //ORDER BY title ASC LIMIT 10

            var q = QueryBuilder.QueryPrompt(prompt);

            var images = db.Query<ImageView>($"SELECT m1.* FROM Image m1 {string.Join(' ', q.Joins)} WHERE {q.WhereClause} ORDER BY {sortField} {sortDir} LIMIT ? OFFSET ?", q.Bindings.Concat(new object[] { pageSize, offset }).ToArray());

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        public IEnumerable<ImageView> Search(QueryOptions queryOptions, Sorting sorting, Paging? paging = null)
        {
            var db = OpenReadonlyConnection();

            var q = QueryCombiner.Parse(queryOptions);

            var whereClause = QueryCombiner.GetInitialWhereClause("main", queryOptions);

            var join = $"INNER JOIN ({q.Query}) sub ON main.Id = sub.Id";

            var where = whereClause.Length > 0 ? $"WHERE {whereClause}" : "";

            var bindings = q.Bindings;

            var page = "";

            if (paging != null)
            {
                page = " LIMIT ? OFFSET ?";
                bindings = bindings.Concat(new object[] { paging.PageSize, paging.Offset });
            }

            var (sortField, sortDir) = sorting;

            var images = db.Query<ImageView>($"SELECT main.Id, Path, {columns}, (SELECT COUNT(1) FROM AlbumImage WHERE ImageId = main.Id) AS AlbumCount FROM Image main {join} {where} ORDER BY {sortField} {sortDir} {page}", bindings.ToArray());

            foreach (var image in images)
            {
                yield return image;
            }

            //db.Close();
        }


        const string allColumns = "FolderId, FileName, Prompt, NegativePrompt, Steps, Sampler, " +
                      "CFGScale, Seed, Width, Height, ModelHash, Model, BatchSize, BatchPos, CreatedDate, ModifiedDate, " +
                      "CustomTags, Rating, Favorite, ForDeletion, NSFW, " +
                      "AestheticScore, HyperNetwork, HyperNetworkStrength, ClipSkip, ENSD, FileSize, NoMetadata, HasError";

        const string columns = "Favorite, ForDeletion, Rating, AestheticScore, CreatedDate, NSFW, HasError";

        public IEnumerable<ImageView> SearchEx(QueryOptions options, Sorting sorting, Paging? paging = null)
        {
            var db = OpenReadonlyConnection();

            var q = QueryCombiner.ParseEx(options);

            var whereClause = QueryCombiner.GetInitialWhereClause("main", options);

            var join = $"INNER JOIN ({q.Query}) sub ON main.Id = sub.Id";

            var where = whereClause.Length > 0 ? $"WHERE {whereClause}" : "";

            var bindings = q.Bindings;

            var page = "";

            if (paging != null)
            {
                page = " LIMIT ? OFFSET ?";
                bindings = bindings.Concat(new object[] { paging.PageSize, paging.Offset });
            }

            var (sortField, sortDir) = sorting;

            var images = db.Query<ImageView>($"SELECT main.Id, Path, {columns}, (SELECT COUNT(1) FROM AlbumImage WHERE ImageId = main.Id) AS AlbumCount FROM Image main {join} {where} ORDER BY {sortField} {sortDir} {page}", bindings.ToArray());

            foreach (var image in images)
            {
                yield return image;
            }

            //db.Close();
        }

        public IEnumerable<ImageView> Search(Filter filter, QueryOptions options, Sorting sorting, Paging? paging = null)
        {
            var db = OpenReadonlyConnection();

            var q = QueryCombiner.Filter(filter, options);

            var whereClause = QueryCombiner.GetInitialWhereClause("main", options);

            var join = $"INNER JOIN ({q.Query}) sub ON main.Id = sub.Id";

            var where = whereClause.Length > 0 ? $"WHERE {whereClause}" : "";

            var bindings = q.Bindings;

            var page = "";

            if (paging != null)
            {
                page = " LIMIT ? OFFSET ?";
                bindings = bindings.Concat(new object[] { paging.PageSize, paging.Offset });
            }

            var (sortField, sortDir) = sorting;

            var images = db.Query<ImageView>($"SELECT main.Id, Path, {columns}, (SELECT COUNT(1) FROM AlbumImage WHERE ImageId = main.Id) AS AlbumCount FROM Image main {join} {where} ORDER BY {sortField} {sortDir} {page}", bindings.ToArray());

            foreach (var image in images)
            {
                yield return image;
            }

            //db.Close();
        }

        private string lastPrompt;
        private List<UsedPrompt>? allResults;

        public IEnumerable<UsedPrompt> SearchPrompts(string? prompt, bool fullText, int distance)
        {
            IEnumerable<UsedPrompt> results = new List<UsedPrompt>();

            using var db = OpenConnection();

            if (string.IsNullOrEmpty(prompt))
            {
                var query = "SELECT Prompt, COUNT(*) AS Usage FROM Image";

                if (QueryBuilder.HideNSFW)
                {
                    query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
                }

                query += " GROUP BY Prompt ORDER BY Usage DESC";

                results = db.Query<UsedPrompt>(query);

                foreach (var result in results)
                {
                    yield return result;
                }

            }
            else
            {

                if (fullText)
                {
                    if (allResults == null || lastPrompt != prompt)
                    {
                        var query = "SELECT Prompt, COUNT(*) AS Usage FROM Image";

                        if (QueryBuilder.HideNSFW)
                        {
                            query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
                        }

                        query += " GROUP BY Prompt ORDER BY Usage DESC";

                        allResults = db.Query<UsedPrompt>(query)
                            .ToList();
                    }

                    // TODO: Try converting the prompt into a list of numbers (vocabulary), then perform hamming on the numbers instead of the whole prompt
                    if (distance > 0)
                    {
                        foreach (var result in allResults.Where(r => r.Prompt != null && r.Prompt.Length >= prompt.Length))
                        {
                            if (HammingDistance(prompt, result.Prompt) <= distance)
                            {
                                yield return result;
                            }
                        }

                    }
                    else
                    {
                        var query = "SELECT Prompt, COUNT(*) AS Usage FROM Image WHERE TRIM(Prompt) = ?";

                        if (QueryBuilder.HideNSFW)
                        {
                            query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
                        }

                        query += " GROUP BY Prompt ORDER BY Usage DESC";

                        results = db.Query<UsedPrompt>(query, new[] { prompt.Trim() });

                        foreach (var result in results)
                        {
                            yield return result;
                        }

                    }
                }
                else
                {
                    var q = QueryBuilder.Parse(prompt);

                    var query = $"SELECT Prompt, COUNT(*) AS Usage FROM Image {string.Join(' ', q.Joins)} WHERE {q.WhereClause}";

                    if (QueryBuilder.HideNSFW)
                    {
                        query += " AND (NSFW = 0 OR NSFW IS NULL)";
                    }

                    query += " GROUP BY Prompt ORDER BY Usage DESC";

                    results = db.Query<UsedPrompt>(query, q.Bindings.ToArray());

                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }

            }


            lastPrompt = prompt;

            db.Close();

        }

        public IEnumerable<UsedPrompt> SearchNegativePrompts(string prompt, bool fullText, int distance)
        {
            IEnumerable<UsedPrompt> results = new List<UsedPrompt>();

            using var db = OpenConnection();

            if (string.IsNullOrEmpty(prompt))
            {
                results = db.Query<UsedPrompt>("SELECT NegativePrompt AS Prompt, COUNT(*) AS Usage FROM Image GROUP BY NegativePrompt ORDER BY Usage DESC");

                foreach (var result in results)
                {
                    yield return result;
                }

            }
            else
            {

                if (fullText)
                {
                    if (allResults == null || lastPrompt != prompt)
                    {
                        allResults = db.Query<UsedPrompt>($"SELECT NegativePrompt AS Prompt, COUNT(*) AS Usage FROM Image GROUP BY NegativePrompt ORDER BY Usage DESC")
                            .ToList();
                    }

                    // TODO: Try converting the prompt into a list of numbers (vocabulary), then perform hamming on the numbers instead of the whole prompt
                    if (distance > 0)
                    {
                        foreach (var result in allResults.Where(r => r.Prompt.Length >= prompt.Length))
                        {
                            if (HammingDistance(prompt, result.Prompt) <= distance)
                            {
                                yield return result;
                            }
                        }

                    }
                    else
                    {
                        results = db.Query<UsedPrompt>($"SELECT NegativePrompt AS Prompt, COUNT(*) AS Usage FROM Image WHERE TRIM(Prompt) = ? GROUP BY NegativePrompt ORDER BY Usage DESC", new[] { prompt.Trim() });

                        foreach (var result in results)
                        {
                            yield return result;
                        }

                    }
                }
                else
                {
                    var q = QueryBuilder.Parse(prompt);

                    results = db.Query<UsedPrompt>($"SELECT NegativePrompt AS Prompt, COUNT(*) AS Usage  FROM Image {string.Join(' ', q.Joins)} WHERE {q.WhereClause} GROUP BY NegativePrompt ORDER BY Usage DESC", q.Bindings.ToArray());

                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }

            }


            lastPrompt = prompt;

            db.Close();
        }


        private static int HammingDistance(String str1,
            String str2)
        {
            int i = 0, count = 0;
            while (i < str1.Length && i < str2.Length)
            {
                if (str1[i] != str2[i])
                    count++;
                i++;
            }
            return count;
        }

    }
}

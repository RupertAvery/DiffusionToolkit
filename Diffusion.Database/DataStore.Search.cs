using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SQLite.SQLite3;
using static System.Net.Mime.MediaTypeNames;

namespace Diffusion.Database
{
    public partial class DataStore
    {

        public int GetTotal()
        {
            using var db = OpenConnection();

            var query = "SELECT COUNT(*) FROM Image";

            var count = db.ExecuteScalar<int>(query);

            db.Close();

            return count;
        }


        public long CountFileSize(string prompt)
        {
            using var db = OpenConnection();


            if (string.IsNullOrEmpty(prompt))
            {
                var query = $"SELECT SUM(FileSize) FROM Image";

                if (QueryBuilder.HideNFSW)
                {
                    query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
                }

                var allcount = db.ExecuteScalar<int>(query);
                return allcount;
            }

            var q = QueryBuilder.Parse(prompt);

            var size = db.ExecuteScalar<long>($"SELECT SUM(FileSize) FROM Image {string.Join(' ', q.Joins)} WHERE {q.WhereClause}", q.Bindings.ToArray());

            db.Close();

            return size;
        }


        public int Count(string prompt)
        {
            using var db = OpenConnection();

            if (string.IsNullOrEmpty(prompt))
            {
                var query = "SELECT COUNT(*) FROM Image";
                
                if (QueryBuilder.HideNFSW)
                {
                    query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
                }

                var allcount = db.ExecuteScalar<int>(query);
                return allcount;
            }

            var q = QueryBuilder.Parse(prompt);

            var count = db.ExecuteScalar<int>($"SELECT COUNT(*) FROM Image {string.Join(' ', q.Joins)} WHERE {q.WhereClause}", q.Bindings.ToArray());

            db.Close();

            return count;
        }



        public long CountFileSize(Filter filter)
        {
            using var db = OpenConnection();

            if (filter.IsEmpty)
            {
                var query = $"SELECT SUM(FileSize) FROM Image";

                if (QueryBuilder.HideNFSW)
                {
                    query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
                }

                var allcount = db.ExecuteScalar<int>(query);

                return allcount;
            }

            var q = QueryBuilder.Filter(filter);

            var size = db.ExecuteScalar<long>($"SELECT SUM(FileSize) FROM Image {string.Join(' ', q.Joins)} WHERE {q.WhereClause}", q.Bindings.ToArray());

            db.Close();

            return size;
        }


        public int Count(Filter filter)
        {
            using var db = OpenConnection();

            if (filter.IsEmpty)
            {
                var query = "SELECT COUNT(*) FROM Image";

                if (QueryBuilder.HideNFSW)
                {
                    query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
                }

                var allcount = db.ExecuteScalar<int>(query);

                return allcount;
            }

            var q = QueryBuilder.Filter(filter);

            var count = db.ExecuteScalar<int>($"SELECT COUNT(*) FROM Image WHERE {q.Item1}", q.Item2.ToArray());

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


        public IEnumerable<Image> QueryAll()
        {
            using var db = OpenConnection();

            var images = db.Query<Image>($"SELECT Image.* FROM Image");

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }


        public IEnumerable<Image> Query(string? prompt)
        {
            using var db = OpenConnection();

            if (string.IsNullOrEmpty(prompt))
            {
                throw new Exception("Query prompt cannot be empty!");
            }

            var q = QueryBuilder.Parse(prompt);

            var images = db.Query<Image>($"SELECT Image.* FROM Image {string.Join(' ', q.Joins)} WHERE {q.WhereClause}", q.Bindings.ToArray());

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        public IEnumerable<Image> Query(Filter filter)
        {
            using var db = OpenConnection();

            if (filter.IsEmpty)
            {
                throw new Exception("Query prompt cannot be empty!");
            }

            var q = QueryBuilder.Filter(filter);

            var images = db.Query<Image>($"SELECT Image.* FROM Image WHERE {q.Item1}", q.Item2.ToArray());

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        public IEnumerable<Image> Search(string? prompt, int pageSize, int offset, string sortBy, string sortDirection)
        {
            using var db = OpenConnection();
            
            var sortField = sortBy switch
            {
                "Date Created" => nameof(Image.CreatedDate),
                "Rating" => nameof(Image.Rating),
                "Aesthetic Score" => nameof(Image.AestheticScore),
                _ => nameof(Image.CreatedDate),
            };

            var sortDir = sortDirection switch
            {
                "A-Z" => "ASC",
                "Z-A" => "DESC",
                _ => "DESC",
            };

            if (string.IsNullOrEmpty(prompt))
            {
                var query = "SELECT Image.* FROM Image ";

                if (QueryBuilder.HideNFSW)
                {
                    query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
                }

                var allimages = db.Query<Image>($"{query} ORDER BY {sortField} {sortDir} LIMIT ? OFFSET ?", pageSize, offset);

                foreach (var image in allimages)
                {
                    yield return image;
                }

                db.Close();

                yield break;
            }

            //SELECT foo, bar, baz, quux FROM table
            //WHERE oid NOT IN(SELECT oid FROM table
            //ORDER BY title ASC LIMIT 50 )
            //ORDER BY title ASC LIMIT 10

            var q = QueryBuilder.Parse(prompt);

            var images = db.Query<Image>($"SELECT Image.* FROM Image {string.Join(' ', q.Joins)} WHERE {q.WhereClause} ORDER BY {sortField} {sortDir} LIMIT ? OFFSET ?", q.Bindings.Concat(new object[] { pageSize, offset }).ToArray());

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        public IEnumerable<Image> Search(Filter filter, int pageSize, int offset, string sortBy, string sortDirection)
        {
            using var db = OpenConnection();

            var sortField = sortBy switch
            {
                "Date Created" => nameof(Image.CreatedDate),
                "Rating" => nameof(Image.Rating),
                "Aesthetic Score" => nameof(Image.AestheticScore),
                _ => nameof(Image.CreatedDate),
            };

            var sortDir = sortDirection switch
            {
                "A-Z" => "ASC",
                "Z-A" => "DESC",
                _ => "DESC",
            };

            if (filter.IsEmpty)
            {
                var query = "SELECT Image.* FROM Image ";

                if (QueryBuilder.HideNFSW)
                {
                    query += " WHERE (NSFW = 0 OR NSFW IS NULL)";
                }

                var allimages = db.Query<Image>($"{query} ORDER BY {sortField} {sortDir}  LIMIT ? OFFSET ?", pageSize, offset);

                foreach (var image in allimages)
                {
                    yield return image;
                }

                db.Close();

                yield break;
            }

            var q = QueryBuilder.Filter(filter);

            var images = db.Query<Image>($"SELECT Image.* FROM Image WHERE {q.Item1} ORDER BY {sortField} {sortDir} LIMIT ? OFFSET ?", q.Item2.Concat(new object[] { pageSize, offset }).ToArray());

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        private string lastPrompt;
        private List<UsedPrompt>? allResults;

        public IEnumerable<UsedPrompt> SearchPrompts(string? prompt, bool fullText, int distance)
        {
            IEnumerable<UsedPrompt> results = new List<UsedPrompt>();

            using var db = OpenConnection();

            if (string.IsNullOrEmpty(prompt))
            {
                results = db.Query<UsedPrompt>("SELECT Prompt, COUNT(*) AS Usage FROM Image GROUP BY Prompt ORDER BY Usage DESC");

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
                        allResults = db.Query<UsedPrompt>($"SELECT Prompt, COUNT(*) AS Usage FROM Image GROUP BY Prompt ORDER BY Usage DESC")
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
                        results = db.Query<UsedPrompt>($"SELECT Prompt, COUNT(*) AS Usage FROM Image WHERE TRIM(Prompt) = ? GROUP BY Prompt ORDER BY Usage DESC", new[] { prompt.Trim() });

                        foreach (var result in results)
                        {
                            yield return result;
                        }

                    }
                }
                else
                {
                    var q = QueryBuilder.Parse(prompt);

                    results = db.Query<UsedPrompt>($"SELECT Prompt, COUNT(*) AS Usage  FROM Image {string.Join(' ', q.Joins)} WHERE {q.WhereClause} GROUP BY Prompt ORDER BY Usage DESC", q.Bindings.ToArray());

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

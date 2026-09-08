using Diffusion.Common;
using Diffusion.Common.Query;
using Diffusion.Database.Models;
using System;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;

namespace Diffusion.Database
{

    public partial class DataStore
    {
        public IEnumerable<Tag> GetTags()
        {
            using var db = OpenConnection();

            var query = $"SELECT Id, Name FROM Tag ORDER BY Name";

            var models = db.Query<Tag>(query);

            db.Close();

            return models;
        }

        public IEnumerable<TagCount> GetTagsWithCount()
        {
            using var db = OpenConnection();

            var query = $"SELECT Id, Name, (SELECT COUNT(1) FROM {nameof(ImageTag)} IT WHERE T.Id = IT.TagId) AS [Count] FROM Tag T ORDER BY Name";

            var models = db.Query<TagCount>(query);

            db.Close();

            return models;
        }


        public void CreateTag(string name)
        {
            using var db = OpenConnection();

            var command = db.CreateCommand("INSERT INTO Tag (Name) VALUES (?)", name);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }

            db.Close();
        }

        public void CreateTags(IEnumerable<string> names)
        {
            using var db = OpenConnection();

            lock (_lock)
            {
                db.BeginTransaction();
                try
                {
                    var command = db.CreateCommand("INSERT INTO Tag (Name) VALUES (@Name)");
                    foreach (var name in names)
                    {
                        command.Bind("@Name", name);
                        command.ExecuteNonQuery();
                    }
                    db.Commit();
                }
                catch (Exception e)
                {
                    db.Rollback();
                    throw;
                }
            }

            db.Close();
        }

        public void UpdateTag(int id, string name)
        {
            using var db = OpenConnection();

            var command = db.CreateCommand("UPDATE Tag SET Name = ? WHERE Id = ?", name, id);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }

            db.Close();
        }

        public void RemoveTag(int id)
        {
            using var db = OpenConnection();

            var command = db.CreateCommand("DELETE FROM Tag WHERE Id = ?", id);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }

            db.Close();
        }

        private class TagIdTemp
        {
            public int TagId { get; set; }
        }

        public IEnumerable<int> GetImageTags(int id)
        {
            using var db = OpenConnection();

            var query = $"SELECT TagId FROM ImageTag WHERE ImageId = {id}";

            var results = db.Query<TagIdTemp>(query);

            db.Close();

            return results.Select(d => d.TagId).ToList();
        }

        public void AddImageTag(int id, int tagId)
        {
            using var db = OpenConnection();

            var command = db.CreateCommand("INSERT OR IGNORE INTO ImageTag (ImageId, TagId) VALUES (?,?)", id, tagId);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }

            db.Close();
        }

        public void RemoveImageTag(int id, int tagId)
        {
            using var db = OpenConnection();

            var command = db.CreateCommand("DELETE FROM ImageTag WHERE ImageId = ? AND TagId = ?", id, tagId);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }

            db.Close();
        }

        public void AddImagesTag(IEnumerable<int> ids, int tagId)
        {
            using var db = OpenConnection();

            var values = new List<string>();

            foreach (var id in ids)
            {
                values.Add($"({id}, {tagId})");
            }

            var command = db.CreateCommand($"INSERT OR IGNORE INTO ImageTag (ImageId, TagId) VALUES {string.Join(", ", values)}");

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }

            db.Close();
        }

        public void RemoveImagesTag(IEnumerable<int> ids, int tagId)
        {
            using var db = OpenConnection();

            var values = new List<string>();

            foreach (var id in ids)
            {
                values.Add($"{id}");
            }

            var command = db.CreateCommand($"DELETE FROM ImageTag WHERE ImageId IN ({string.Join(", ", values)}) AND TagId = ?", tagId);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }

            db.Close();
        }

        public void RemoveImageTags(int id)
        {
            using var db = OpenConnection();

            var command = db.CreateCommand("DELETE FROM ImageTag SET WHERE ImageId = ?", id);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }

            db.Close();
        }
    }
}

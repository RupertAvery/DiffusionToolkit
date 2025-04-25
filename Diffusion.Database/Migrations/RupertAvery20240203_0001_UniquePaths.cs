using Diffusion.Database.Models;

namespace Diffusion.Database
{
    public partial class Migrations
    {

        [Migrate(MigrationType.Pre)]
        private string RupertAvery20240203_0001_UniquePaths()
        {
            var tableExists = _db.ExecuteScalar<int>("SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='Image'") == 1;

            if (!tableExists)
            {
                return null;
            }

            var dupePaths = _db.QueryScalars<string>("SELECT Path FROM Image GROUP BY Path HAVING COUNT(*) > 1");

            void RemoveImages(IEnumerable<int> ids)
            {
                _db.BeginTransaction();

                var albumQuery = "DELETE FROM AlbumImage WHERE ImageId = @Id";
                var albumCommand = _db.CreateCommand(albumQuery);

                var query = "DELETE FROM Image WHERE Id = @Id";
                var command = _db.CreateCommand(query);

                foreach (var id in ids)
                {
                    albumCommand.Bind("@Id", id);
                    albumCommand.ExecuteNonQuery();

                    command.Bind("@Id", id);
                    command.ExecuteNonQuery();
                }

                _db.Commit();
            }

            if (dupePaths.Any())
            {
                var dupeImages = _db.Query<Image>($"SELECT * FROM Image WHERE Path IN ({string.Join(",", dupePaths.Select(p => $"'{p.Replace("'", "''")}'"))})");
                var groups = dupeImages.GroupBy(image => image.Path);
                var ids = new List<int>();
                foreach (var group in groups)
                {
                    var lowest = group.MinBy(image => image.Id);
                    var dupes = group.Where(i => i.Id != lowest.Id);
                    ids.AddRange(dupes.Select(i => i.Id));
                }
                RemoveImages(ids);
            }

            return "DROP INDEX IF EXISTS 'Image_Path';";
        }

    }
}

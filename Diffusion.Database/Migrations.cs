using System.Reflection;
using System.Xml.Linq;
using Diffusion.Common;
using SQLite;

namespace Diffusion.Database;

public class Migrations
{
    private readonly SQLiteConnection _db;

    public Migrations(SQLiteConnection db)
    {
        _db = db;
        db.CreateTable<Migration>();
    }

    public void CreateBackup()
    {
        var path = Path.GetDirectoryName(_db.DatabasePath);
        var backupFilename = $"Backup-Migrations-{DateTime.Now:yyyyMMdd-hhmmss}.db";
        File.Copy(_db.DatabasePath, Path.Combine(path, backupFilename));
    }

    public void Update()
    {
        var existingMigrations = _db.Query<Migration>("SELECT Id, Name FROM Migration");

        var methods = typeof(Migrations).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

        var migrations = methods.Where(m => m.GetCustomAttributes<MigrateAttribute>().Any());

        var newMigrations = migrations.Where(m => !existingMigrations.Select(p => p.Name).Contains(m.Name)).OrderBy(m => m.Name).ToList();

        try
        {
            if (newMigrations.Any())
            {
                Logger.Log("Backing up database prior to migrations");

                CreateBackup();

                foreach (var methodInfo in newMigrations)
                {
                    var name = methodInfo.Name;

                    var migrate = methodInfo.GetCustomAttributes<MigrateAttribute>().FirstOrDefault();
                    if (migrate is { Name: { } })
                    {
                        name = migrate.Name;
                    }

                    var sql = (string)methodInfo.Invoke(this, null)!;

                    _db.BeginTransaction();

                    var statements = sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var statement in statements)
                    {
                        if (statement.Trim().Length > 0)
                        {
                            var command = _db.CreateCommand(statement);
                            command.ExecuteNonQuery();
                        }
                    }

                    _db.Execute("INSERT INTO Migration (Name) VALUES (?)", name);

                    _db.Commit();

                    Logger.Log($"Executed Migration {name}");
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log($"Error executing migration: {e.Message}");
            Logger.Log($"{e.StackTrace}");
        }
    }


    [Migrate]
    private string RupertAvery20231224_0001_CleanupOrphanedAlbumImageEntries()
    {
        return "DELETE FROM AlbumImage WHERE AlbumId IN (SELECT AlbumId FROM AlbumImage WHERE AlbumId NOT IN (SELECT Id FROM Album));" +
               "DELETE FROM AlbumImage WHERE ImageId NOT IN(SELECT Id FROM Image);";
    }

    [Migrate]
    private string RupertAvery20231224_0002_AlbumImageForeignKeys()
    {
        return @"DROP TABLE IF EXISTS ""AlbumImageTemp"";
CREATE TABLE IF NOT EXISTS ""AlbumImageTemp""(
    ""AlbumId""   integer,
    ""ImageId""   integer,
    CONSTRAINT ""FK_AlbumImage_AlbumId"" FOREIGN KEY(""AlbumId"") REFERENCES Album(""Id""),
    CONSTRAINT ""FK_AlbumImage_ImageId"" FOREIGN KEY(""ImageId"") REFERENCES Image(""Id"")
);
INSERT INTO AlbumImageTemp SELECT AlbumId, ImageId FROM AlbumImage;
DROP TABLE ""AlbumImage"";
ALTER TABLE ""AlbumImageTemp"" RENAME TO ""AlbumImage"";";
    }
}
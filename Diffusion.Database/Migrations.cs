using System.IO;
using System.Reflection;
using System.Runtime;
using Diffusion.Common;
using Diffusion.Database.Models;
using SQLite;
using static Dapper.SqlMapper;

namespace Diffusion.Database;

public class Migrations
{
    private readonly SQLiteConnection _db;
    private readonly object _settings;

    public Migrations(SQLiteConnection db, object settings)
    {
        _db = db;
        _settings = settings;
        db.CreateTable<Migration>();
    }

    public void CreateBackup()
    {
        var path = Path.GetDirectoryName(_db.DatabasePath);
        var backupFilename = $"Backup-Migrations-{DateTime.Now:yyyyMMdd-hhmmss}.db";
        File.Copy(_db.DatabasePath, Path.Combine(path, backupFilename));
    }

    private IReadOnlyCollection<MethodInfo> GetMigrations(MigrationType migrationType)
    {
        var existingMigrations = _db.Query<Migration>("SELECT Id, Name FROM Migration");

        var methods = typeof(Migrations).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

        var migrations = methods.Where(m => m.GetCustomAttributes<MigrateAttribute>().Any(a => a.MigrationType == migrationType));

        return migrations.Where(m => !existingMigrations.Select(p => p.Name).Contains(m.Name)).OrderBy(m => m.Name).ToList();
    }

    public bool RequiresMigration(MigrationType migrationType)
    {
        return GetMigrations(migrationType).Any();
    }

    public void Update(MigrationType migrationType)
    {
        var newMigrations = GetMigrations(migrationType);

        try
        {
            if (newMigrations.Any())
            {
                //Logger.Log("Backing up database prior to migrations");

                //CreateBackup();

                foreach (var methodInfo in newMigrations)
                {
                    var name = methodInfo.Name;

                    var migrate = methodInfo.GetCustomAttributes<MigrateAttribute>().First();

                    if (migrate is { Name: { } })
                    {
                        name = migrate.Name;
                    }

                    var sql = (string)methodInfo.Invoke(this, null)!;



                    if (sql != null)
                    {
                        if (!migrate.NoTransaction)
                        {
                            _db.BeginTransaction();
                        }

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

                        if (!migrate.NoTransaction)
                        {
                            _db.Commit();
                        }
                    }




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





    [Migrate]
    private string RupertAvery20240102_0001_LoadFileNamesFromPaths()
    {
        return "UPDATE Image SET FileName = path_basename(replace(Path, '\\','/'))";
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

    [Migrate(MigrationType.Post)]
    private string RupertAvery20240818_0001_SetUnavailable()
    {
        return "UPDATE Image SET Unavailable = 0";
    }

    //[Migrate(MigrationType.Pre, true)]
    //private string RupertAvery20250321_0001_EnableWAL()
    //{
    //    return "PRAGMA journal_mode=WAL";
    //}

    private class FolderTemp : Folder
    {
        public string ParentPath { get; set; }
    }

    [Migrate(MigrationType.Post)]
    private string RupertAvery20250405_0001_FixFolders()
    {
        var assembly = Assembly.GetEntryAssembly();

        var type = assembly.GetType("Diffusion.Toolkit.Configuration.Settings");

        var rootProperty = type.GetProperty("ImagePaths");

        var rootPaths = (List<string>)rootProperty.GetValue(_settings);

        var excludeProperty = type.GetProperty("ExcludePaths");

        var excludePaths = (List<string>)excludeProperty.GetValue(_settings);

        if (rootPaths != null && rootPaths.Any())
        {

            foreach (var path in rootPaths)
            {
                _db.ExecuteScalar<int>("INSERT OR IGNORE INTO Folder (ParentId, Path, Unavailable, Archived, IsRoot) VALUES (0, ?, 0, 0, 0) RETURNING Id", path);
            }

            var folders = _db.Query<FolderTemp>("SELECT Id, ParentId, Path, ImageCount, ScannedDate, Unavailable, Archived, IsRoot FROM Folder");

            foreach (var folder in folders)
            {
                folder.ParentPath = folder.Path.Substring(0, folder.Path.LastIndexOf('\\'));
            }

            foreach (var folder in folders)
            {
                var children = folders.Where(d => d.ParentPath == folder.Path);
                foreach (var child in children)
                {
                    child.ParentId = folder.Id;
                }
            }

            Logger.Log("Updating Folder ParentIds");

            foreach (var folder in folders)
            {
                _db.Execute("UPDATE Folder SET ParentId = ?, IsRoot = ?, Unavailable = 0, Archived = 0 WHERE Id = ?", folder.ParentId, folder.IsRoot, folder.Id);
            }

            var orphanedFolders = _db.Query<FolderTemp>("SELECT Id, ParentId, Path, ImageCount, ScannedDate, Unavailable, Archived, IsRoot FROM Folder f WHERE ParentId = 0 AND IsRoot = 0");

            Logger.Log($"Found {orphanedFolders.Count} orphaned folders");

            foreach (var folder in orphanedFolders)
            {
                folder.ParentPath = folder.Path.Substring(0, folder.Path.LastIndexOf('\\'));
            }

            var parents = orphanedFolders.Select(d => d.ParentPath).Distinct();

            var folderCache = _db.Query<Folder>("SELECT Id, ParentId, Path, ImageCount, ScannedDate, Unavailable, Archived, IsRoot FROM Folder").ToDictionary(d => d.Path);

            Logger.Log($"Creating parent folders");

            foreach (var folder in parents)
            {
                Logger.Log($"Creating {folder}");

                try
                {
                    if (DataStore.EnsureFolderExists(_db, folder, folderCache, out var folderId))
                    {
                        var children = orphanedFolders.Where(d => d.ParentPath == folder);

                        foreach (var child in children)
                        {
                            _db.Execute("UPDATE Folder SET ParentId = ? WHERE Id = ?", folderId, child.Id);
                        }
                    }
                    else
                    {
                        Logger.Log($"Root folder not found for {folder}");
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                }

            }

            var cleanup = @"DELETE FROM Folder WHERE Id IN 
(
     SELECT f.Id FROM Folder f 
     LEFT JOIN Image i ON f.Id = i.FolderId
     WHERE ParentId = 0
     GROUP BY f.Id
     HAVING COUNT(i.Id) = 0
)";

            var count = _db.Execute(cleanup);

            Logger.Log($"Removed {count} empty orphaned folders");


            if (excludePaths != null && excludePaths.Any())
            {

                Logger.Log($"Migrating excluded paths");

                _db.Execute("UPDATE Folder SET Excluded = 0");

                foreach (var folder in excludePaths)
                {
                    Logger.Log($"Excluding {folder}");

                    try
                    {
                        if (DataStore.EnsureFolderExists(_db, folder, folderCache, out var folderId))
                        {
                            Logger.Log($"Setting {folder} as Excluded");
                            _db.Execute("UPDATE Folder SET Excluded = 1 WHERE Id = ?", folderId);
                        }
                        else
                        {
                            Logger.Log($"Root folder not found for {folder}");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message);
                    }

                }
            }
        }


        // Return empty statement so that nothing happens
        return ";";
    }
}
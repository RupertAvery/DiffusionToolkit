using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Diffusion.Common;
using SQLite;
using static Dapper.SqlMapper;

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

    private IReadOnlyCollection<MethodInfo> GetMigrations()
    {
        var existingMigrations = _db.Query<Migration>("SELECT Id, Name FROM Migration");

        var methods = typeof(Migrations).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

        var migrations = methods.Where(m => m.GetCustomAttributes<MigrateAttribute>().Any());

        return migrations.Where(m => !existingMigrations.Select(p => p.Name).Contains(m.Name)).OrderBy(m => m.Name).ToList();
    }

    public bool RequiresMigration()
    {
        return GetMigrations().Any();
    }

    public void Update()
    {
        var newMigrations = GetMigrations();

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
    private string RupertAvery20240125_0001_MigrateFoldersToDB()
    {
        var settingsPath = Path.Combine(AppInfo.AppDir, "config.json");
        
        var isPortable = true;

        if (!File.Exists(settingsPath))
        {
            isPortable = false;
            settingsPath = Path.Combine(AppInfo.AppDataPath, "config.json");
        }

        var configuration = new Configuration<FolderSettings>(settingsPath, isPortable);

        configuration.Load(out var folderSettings);

        var sb = new StringBuilder();

        if (folderSettings.ImagePaths != null)
        {
            if (folderSettings.ImagePaths.Any())
            {
                sb.Append("INSERT INTO Folder (Path, IsRoot) VALUES ");
                foreach (var path in folderSettings.ImagePaths)
                {
                    sb.Append($"('{path.Replace("'", "''")}', 1),");
                }

                sb.Remove(sb.Length -1, 1);

                sb.AppendLine(" ON CONFLICT (Path) DO UPDATE SET IsRoot=1;");
            }
        }

        if (folderSettings.ExcludePaths != null)
        {
            if (folderSettings.ExcludePaths.Any())
            {
                sb.Append("INSERT INTO ExcludeFolder (Path) VALUES ");
                foreach (var path in folderSettings.ExcludePaths)
                {
                    sb.Append($"('{path.Replace("'", "''")}'),");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.AppendLine(";");
            }
        }

        if (sb.Length == 0)
        {
            sb.Append("SELECT 1");
        }


        return sb.ToString();
    }

    private class FolderSettings
    {
        public List<string> ImagePaths
        {
            get;
            set;
        }

        public List<string> ExcludePaths
        {
            get;
            set;
        }
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
}
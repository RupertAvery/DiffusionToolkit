using System.Reflection;
using Diffusion.Common;
using Diffusion.Database.Models;
using SQLite;

namespace Diffusion.Database;

public partial class Migrations
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
}
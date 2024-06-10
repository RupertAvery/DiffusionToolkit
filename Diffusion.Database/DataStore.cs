using SQLite;
using System.Runtime.InteropServices;
using Diffusion.Common;

namespace Diffusion.Database;

public class MigrationEventArgs
{
    public MigrationType MigrationType { get; set; }
}

public class ThumbnailEntry
{
    [PrimaryKey]
    public int Id { get; set; }
    public int Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Path { get; set; }
    public byte[] Data { get; set; }
}

public partial class DataStore
{
    private readonly string _extensionsPath;
    private readonly string _altExtensionsPath;
    public string DatabasePath { get; }
    public bool RescanRequired { get; set; }

    public SQLiteConnection OpenConnection()
    {
        var db = new SQLiteConnection(DatabasePath);
        return db;
    }

    public DataStore(string databasePath)
    {
        DatabasePath = databasePath;
    }

    public DataStore(string databasePath, string extensionsPath, string altExtensionsPath)
    {
        _extensionsPath = extensionsPath;
        _altExtensionsPath = altExtensionsPath;
        DatabasePath = databasePath;
    }

    public event EventHandler<MigrationEventArgs> BeforeMigrate;
    public event EventHandler<MigrationEventArgs> AfterMigrate;

    public async Task Create()
    {
        var databaseDir = Path.GetDirectoryName(DatabasePath);

        if (!Directory.Exists(databaseDir))
        {
            Directory.CreateDirectory(databaseDir);
        }

        using var db = OpenConnection();

        db.EnableLoadExtension(true);

        var ext = ".dll";


        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ext = ".dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            ext = ".so";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ext = ".dylib";
        }

        var path = Path.Combine(_extensionsPath, $"path0{ext}");

        if (File.Exists(path))
        {
            db.LoadExtension(path);
        }
        else
        {
            Logger.Log($"Failed to load SQLite extension {path}... Checking alternate path...");
            path = Path.Combine(_altExtensionsPath, $"path0{ext}");

            if (File.Exists(path))
            {
                db.LoadExtension(path);
            }
            else
            {
                Logger.Log($"Failed to load SQLite extension {path}... aborting! Errors may occur in the database!");
            }
        }

        var migrations = new Migrations(db);

        if (migrations.RequiresMigration(MigrationType.Pre))
        {
            BeforeMigrate?.Invoke(this, new MigrationEventArgs { MigrationType = MigrationType.Pre });
            try
            {
                await Task.Run(() =>
                {
                    migrations.Update(MigrationType.Pre);
                });
            }
            finally
            {
                AfterMigrate?.Invoke(this, new MigrationEventArgs { MigrationType = MigrationType.Pre });
            }
        }

        db.CreateTable<Image>();
        db.CreateIndex<Image>(image => image.FolderId);
        db.CreateIndex<Image>(image => image.Path, true);
        db.CreateIndex<Image>(image => image.FileName);
        db.CreateIndex<Image>(image => image.ModelHash);
        db.CreateIndex<Image>(image => image.Model);
        db.CreateIndex<Image>(image => image.Seed);
        db.CreateIndex<Image>(image => image.Sampler);
        db.CreateIndex<Image>(image => image.Height);
        db.CreateIndex<Image>(image => image.Width);
        db.CreateIndex<Image>(image => image.CFGScale);
        db.CreateIndex<Image>(image => image.Steps);
        db.CreateIndex<Image>(image => image.AestheticScore);
        db.CreateIndex<Image>(image => image.Favorite);
        db.CreateIndex<Image>(image => image.Rating);
        db.CreateIndex<Image>(image => image.ForDeletion);
        db.CreateIndex<Image>(image => image.NSFW);
        db.CreateIndex<Image>(image => image.CreatedDate);
        db.CreateIndex<Image>(image => image.HyperNetwork);
        db.CreateIndex<Image>(image => image.HyperNetworkStrength);
        db.CreateIndex<Image>(image => image.FileSize);
        db.CreateIndex<Image>(image => image.ModifiedDate);

        db.CreateTable<Album>();
        db.CreateIndex<Album>(album => album.Name, true);
        db.CreateIndex<Album>(album => album.LastUpdated);

        CreateAlbumImageTable(db);
        db.CreateTable<AlbumImage>();
        db.CreateIndex<AlbumImage>(album => album.AlbumId);
        db.CreateIndex<AlbumImage>(album => album.ImageId);

        db.CreateTable<Folder>();
        db.CreateIndex<Folder>(folder => folder.ParentId);

        if (migrations.RequiresMigration(MigrationType.Post))
        {
            BeforeMigrate?.Invoke(this, new MigrationEventArgs { MigrationType = MigrationType.Post });
            try
            {
                await Task.Run(() =>
                {
                    migrations.Update(MigrationType.Post);
                });
            }
            finally
            {
                AfterMigrate?.Invoke(this, new MigrationEventArgs { MigrationType = MigrationType.Post });
            }
        }

        db.Close();
    }

    void CreateAlbumImageTable(SQLiteConnection db)
    {
        var sql = @"
        CREATE TABLE IF NOT EXISTS ""AlbumImage""(
            ""AlbumId""   integer,
            ""ImageId""   integer,
            CONSTRAINT ""FK_AlbumImage_AlbumId"" FOREIGN KEY(""AlbumId"") REFERENCES Album(""Id""),
            CONSTRAINT ""FK_AlbumImage_ImageId"" FOREIGN KEY(""ImageId"") REFERENCES Image(""Id"")
        );
        ";
        var command = db.CreateCommand(sql);
        command.ExecuteNonQuery();
    }


    public void Close(SQLiteConnection connection)
    {
        connection.Close();
    }

    public void RebuildIndexes()
    {
        using var db = OpenConnection();

        db.DropIndex<Image>(image => image.Path);
        db.DropIndex<Image>(image => image.ModelHash);
        db.DropIndex<Image>(image => image.Seed);
        db.DropIndex<Image>(image => image.Sampler);
        db.DropIndex<Image>(image => image.Height);
        db.DropIndex<Image>(image => image.Width);
        db.DropIndex<Image>(image => image.CFGScale);
        db.DropIndex<Image>(image => image.Steps);
        db.DropIndex<Image>(image => image.AestheticScore);
        db.DropIndex<Image>(image => image.Favorite);
        db.DropIndex<Image>(image => image.CreatedDate);
        db.DropIndex<Image>(image => image.ForDeletion);
        db.DropIndex<Image>(image => image.NSFW);
        db.DropIndex<Image>(image => image.HyperNetwork);
        db.DropIndex<Image>(image => image.HyperNetworkStrength);
        db.DropIndex<Image>(image => image.FileSize);

        db.CreateIndex<Image>(image => image.Path);
        db.CreateIndex<Image>(image => image.ModelHash);
        db.CreateIndex<Image>(image => image.Seed);
        db.CreateIndex<Image>(image => image.Sampler);
        db.CreateIndex<Image>(image => image.Height);
        db.CreateIndex<Image>(image => image.Width);
        db.CreateIndex<Image>(image => image.CFGScale);
        db.CreateIndex<Image>(image => image.Steps);
        db.CreateIndex<Image>(image => image.AestheticScore);
        db.CreateIndex<Image>(image => image.Favorite);
        db.CreateIndex<Image>(image => image.CreatedDate);
        db.CreateIndex<Image>(image => image.ForDeletion);
        db.CreateIndex<Image>(image => image.NSFW);
        db.CreateIndex<Image>(image => image.HyperNetwork);
        db.CreateIndex<Image>(image => image.HyperNetworkStrength);
        db.CreateIndex<Image>(image => image.FileSize);
        db.Close();

    }


    public void CreateBackup()
    {
        var path = Path.GetDirectoryName(DatabasePath);
        var backupFilename = $"Backup-{DateTime.Now:yyyyMMdd-hhmmss}.db";
        File.Copy(DatabasePath, Path.Combine(path, backupFilename));
    }

    public bool TryRestoreBackup(string path)
    {
        try
        {
            File.Copy(path, DatabasePath, true);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}

public class ImagePath
{
    public int Id { get; set; }
    public int FolderId { get; set; }
    public string Path { get; set; }
}

public class UsedPrompt
{
    public string? Prompt { get; set; }
    public int Usage { get; set; }
}

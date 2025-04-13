using System.Diagnostics;
using Diffusion.Database.Models;
using SQLite;

namespace Diffusion.Database;

public partial class DataStore
{
    private readonly object _lock = new object();
    public static int _instances;


    public string DatabasePath { get; }
    public bool RescanRequired { get; set; }

    private SQLiteConnection? _readOnlyConnection;

    public SQLiteConnection OpenReadonlyConnection()
    {
        if (_readOnlyConnection == null)
        {
            _readOnlyConnection = new SQLiteConnection(DatabasePath, SQLiteOpenFlags.ReadOnly);
            _readOnlyConnection.Execute("pragma cache_size=-1000000");
        }

        return _readOnlyConnection;
    }

    public SQLiteConnection OpenConnection()
    {
        var db = new SQLiteConnection(DatabasePath);
        db.Trace = true;
        db.Tracer = (args) =>
        {
            Debug.WriteLine(args);
        };
        return db;
    }

    public DataStore(string databasePath)
    {
        DatabasePath = databasePath;
    }

    public async Task Create(object settings, Func<object> notify, Action<object> complete)
    {
        var databaseDir = Path.GetDirectoryName(DatabasePath);

        if (!Directory.Exists(databaseDir))
        {
            Directory.CreateDirectory(databaseDir);
        }

        using var db = OpenConnection();

        db.EnableLoadExtension(true);

        if (!File.Exists("extensions\\path0.dll"))
        {
            throw new FileNotFoundException("Failed to load SQLite extensions", "path0.dll");
        }

        db.LoadExtension("extensions\\path0.dll");


        var migrations = new Migrations(db, settings);

        if (migrations.RequiresMigration(MigrationType.Pre))
        {
            var handle = notify?.Invoke();
            try
            {
                await Task.Run(() =>
                {
                    migrations.Update(MigrationType.Pre);
                });
            }
            finally
            {
                complete?.Invoke(handle);
            }
        }

        await Task.Run(() =>
        {
            object handle = null;

            void SchemaUpdated(object? sender, EventArgs args)
            {
                if (handle == null)
                {
                    handle = notify?.Invoke();
                }
            }

            try
            {
                db.SchemaUpdated += SchemaUpdated;
                db.CreateTable<Image>();
                db.CreateIndex<Image>(image => image.RootFolderId);
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
                db.CreateIndex<Image>(image => image.Unavailable);
                db.CreateIndex<Image>(image => image.CreatedDate);
                db.CreateIndex<Image>(image => image.HyperNetwork);
                db.CreateIndex<Image>(image => image.HyperNetworkStrength);
                db.CreateIndex<Image>(image => image.FileSize);
                db.CreateIndex<Image>(image => image.ModifiedDate);

                db.CreateIndex<Image>(image => image.Prompt);
                db.CreateIndex<Image>(image => image.NegativePrompt);
                db.CreateIndex<Image>(image => image.Workflow);
                db.CreateIndex<Image>(image => image.WorkflowId);
                db.CreateIndex<Image>(image => image.HasError);
                db.CreateIndex<Image>(image => image.Hash);
                db.CreateIndex<Image>(image => image.ViewedDate);
                db.CreateIndex<Image>(image => image.TouchedDate);

                db.CreateIndex<Image>(image => new { image.HasError, image.CreatedDate });
                db.CreateIndex<Image>(image => new { image.ForDeletion, image.CreatedDate });
                db.CreateIndex<Image>(image => new { image.NSFW, image.CreatedDate });
                db.CreateIndex<Image>(image => new { image.Unavailable, image.CreatedDate });
                db.CreateIndex<Image>(image => new { image.ForDeletion, image.Unavailable, image.CreatedDate });
                db.CreateIndex<Image>(image => new { image.NSFW, image.ForDeletion, image.Unavailable, image.CreatedDate });


                db.CreateTable<Album>();
                db.CreateIndex<Album>(album => album.Name, true);
                db.CreateIndex<Album>(album => album.LastUpdated);

                CreateAlbumImageTable(db);
                db.CreateTable<AlbumImage>();
                db.CreateIndex<AlbumImage>(album => album.AlbumId);
                db.CreateIndex<AlbumImage>(album => album.ImageId);

                db.CreateTable<Node>();
                db.CreateIndex<Node>(node => node.ImageId);
                db.CreateIndex<Node>(node => node.Name);
                db.CreateIndex<Node>(node => new { node.ImageId, node.NodeId }, true);

                db.CreateTable<NodeProperty>();
                db.CreateIndex<NodeProperty>(property => property.NodeId);
                db.CreateIndex<NodeProperty>(property => property.Name);
                db.CreateIndex<NodeProperty>(property => property.Value);
                db.CreateIndex<NodeProperty>(property => new { property.Name, property.Value });

                db.CreateTable<Folder>();
                db.CreateIndex<Folder>(folder => folder.ParentId);
                db.CreateIndex<Folder>(folder => folder.Path, true);
                db.CreateIndex<Folder>(folder => folder.Archived);
                db.CreateIndex<Folder>(folder => folder.Unavailable);
                db.CreateIndex<Folder>(folder => folder.Excluded);

                db.CreateTable<Query>();
                db.CreateIndex<Query>(query => query.Name, true);

            }
            finally
            {
                if (handle != null)
                {
                    complete?.Invoke(handle);
                }
            }
        });


        if (migrations.RequiresMigration(MigrationType.Post))
        {
            var handle = notify?.Invoke();
            try
            {
                await Task.Run(() =>
                {
                    migrations.Update(MigrationType.Post);
                });
            }
            finally
            {
                complete?.Invoke(handle);
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

    public void Close()
    {
        _readOnlyConnection?.Close();
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
    public bool Unavailable { get; set; }
}

public class UsedPrompt
{
    public string? Prompt { get; set; }
    public int Usage { get; set; }
}

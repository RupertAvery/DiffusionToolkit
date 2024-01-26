using System.Collections.ObjectModel;
using SQLite;
using static SQLite.SQLite3;

namespace Diffusion.Database;

public partial class DataStore
{
    public string DatabasePath { get; }
    public bool RescanRequired { get; set; }

    public SQLiteConnection OpenConnection()
    {
        var db = new SQLiteConnection(DatabasePath);

        //db.EnableLoadExtension(true);
        //db.Execute("SELECT load_extension(?)", "dlls/path0");

        return db;
    }

    public DataStore(string databasePath)
    {
        DatabasePath = databasePath;
    }

    public async Task Create(Action notify, Action complete)
    {
        var databaseDir = Path.GetDirectoryName(DatabasePath);

        if (!Directory.Exists(databaseDir))
        {
            Directory.CreateDirectory(databaseDir);
        }

        using var db = OpenConnection();

        db.EnableLoadExtension(true);
        db.LoadExtension("extensions\\path0.dll");


        var migrations = new Migrations(db);

        db.CreateTable<Image>();
        db.CreateIndex<Image>(image => image.FolderId);
        db.CreateIndex<Image>(image => image.Path);
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

        db.CreateTable<Album>();
        db.CreateIndex<Album>(album => album.Name, true);
        db.CreateIndex<Album>(album => album.LastUpdated);

        CreateAlbumImageTable(db);
        db.CreateTable<AlbumImage>();
        db.CreateIndex<AlbumImage>(album => album.AlbumId);
        db.CreateIndex<AlbumImage>(album => album.ImageId);

        db.CreateTable<Folder>();
        db.CreateIndex<Folder>(folder => folder.ParentId);

        db.CreateTable<ExcludeFolder>();

        if (migrations.RequiresMigration())
        {
            notify?.Invoke();
            try
            {
                await Task.Run(() =>
                {
                    migrations.Update();
                });
            }
            finally
            {
                complete?.Invoke();
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

    public  bool TryRestoreBackup(string path)
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

    public int AddExcludedFolders(ObservableCollection<string> excludedFolders)
    {
        using var db = OpenConnection();

        var values = string.Join(",", excludedFolders.Select(f => $"('{f.Replace("'","''")}')"));

        var result = db.Execute($"INSERT INTO ExcludeFolder (Path) VALUES {values} ON CONFLICT (Path) DO NOTHING;");

        db.Close();

        return result;
    }

    public int RemoveExcludedFolders(ObservableCollection<string> excludedFolders)
    {
        using var db = OpenConnection();

        var dbFolders = db.QueryScalars<string>("SELECT Path FROM ExcludeFolder");

        var deleted = string.Join(",", dbFolders.Except(excludedFolders).Select(f => $"'{f.Replace("'", "''")}'"));

        var result = db.Execute($"DELETE FROM ExcludeFolder WHERE Path IN ({deleted})");

        db.Close();

        return result;
    }

    public int AddFolders(ObservableCollection<string> folders)
    {
        using var db = OpenConnection();

        var values = string.Join(",", folders.Select(f => $"('{f.Replace("'", "''")}', 1)"));

        var result = db.Execute($"INSERT INTO Folder (Path, IsRoot) VALUES {values} ON CONFLICT (Path) DO NOTHING;");

        db.Close();

        return result;
    }

    public int RemoveFolders(ObservableCollection<string> folders)
    {
        using var db = OpenConnection();

        var dbFolders = db.QueryScalars<string>("SELECT Path FROM Folder");

        var deleted = string.Join(",", dbFolders.Except(folders).Select(f => $"'{f.Replace("'", "''")}'"));

        var result = db.Execute($"DELETE FROM Folder WHERE Path IN ({deleted})");

        db.Close();

        return result;
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

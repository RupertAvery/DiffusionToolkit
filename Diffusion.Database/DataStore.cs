using SQLite;

namespace Diffusion.Database;

public partial class DataStore
{
    public string DatabasePath { get; }
    public bool RescanRequired { get; set; }

    public SQLiteConnection OpenConnection()
    {
        var db= new SQLiteConnection(DatabasePath);

        //db.EnableLoadExtension(true);
        //db.Execute("SELECT load_extension(?)", "dlls/path0");

        return db;
    }

    public DataStore(string databasePath)
    {
        DatabasePath = databasePath;
        Create();
    }

    public bool CheckIsValid()
    {
        var isValid = false;
        try
        {
            var baseImageColumns = new[]
            {
                nameof(Image.Path),
                nameof(Image.Prompt),
                nameof(Image.Seed),
                nameof(Image.Sampler),
                nameof(Image.CFGScale),
            };

            using var db = OpenConnection();
            
            if (db.TableExist(nameof(Image)))
            {
                isValid = true;

                var columns = db.GetTableInfo(nameof(Image));

                var columnNames = columns.Select(cc => cc.Name).ToList();

                if (!baseImageColumns.All(c => columnNames.Any(cc => cc == c)))
                {
                    isValid = false;
                }

            }
        }
        catch (Exception e)
        {
            return false;
        }


        return isValid;
    }

    public void Create()
    {
        var databaseDir = Path.GetDirectoryName(DatabasePath);

        if (!Directory.Exists(databaseDir))
        {
            Directory.CreateDirectory(databaseDir);
        }

        using var db = OpenConnection();

        db.CreateTable<Album>();
        db.CreateIndex<Album>(album => album.Name, true);
        db.CreateIndex<Album>(album => album.LastUpdated);

        db.CreateTable<AlbumImage>();
        db.CreateIndex<AlbumImage>(album => album.AlbumId);
        db.CreateIndex<AlbumImage>(album => album.ImageId);

        db.CreateTable<Folder>();
        db.CreateIndex<Folder>(folder => folder.ParentId);

        //db.CreateTable<File>();
        db.CreateTable<Image>();

        db.CreateIndex<Image>(image => image.FolderId);
        db.CreateIndex<Image>(image => image.Path);
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

        db.Close();

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
        var temp = new DataStore(path);
        if (temp.CheckIsValid())
        {
            File.Copy(path, DatabasePath, true);
            Create();
            return true;
        }

        return false;
    }
}

public class ImagePath
{
    public int Id { get; set; }
    public string Path { get; set; }
}

public class UsedPrompt
{
    public string? Prompt { get; set; }
    public int Usage { get; set; }
}

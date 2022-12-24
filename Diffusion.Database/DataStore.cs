using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection.PortableExecutable;
using static System.Net.Mime.MediaTypeNames;

namespace Diffusion.Database;

public partial class DataStore
{
    public string DatabasePath { get; }
    public bool RescanRequired { get; set; }

    public SQLiteConnection OpenConnection()
    {
        return new SQLiteConnection(DatabasePath);
    }

    public DataStore(string databasePath)
    {
        DatabasePath = databasePath;
        Create();
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
        db.CreateTable<AlbumImage>();
        db.CreateIndex<AlbumImage>(image => image.AlbumId);

        //db.CreateTable<File>();
        db.CreateTable<Image>();

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

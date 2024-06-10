using SQLite;

namespace Diffusion.Database;

public class ThumbnailCache
{
    public string DatabasePath { get; }

    public SQLiteConnection OpenConnection()
    {
        var db = new SQLiteConnection(DatabasePath);
        return db;
    }

    public ThumbnailCache(string databasePath)
    {
        DatabasePath = databasePath;
    }


    public async Task Create()
    {
        var databaseDir = Path.GetDirectoryName(DatabasePath);

        if (!Directory.Exists(databaseDir))
        {
            Directory.CreateDirectory(databaseDir);
        }

        using var db = OpenConnection();
        db.CreateTable<ThumbnailEntry>();
        db.CreateIndex<ThumbnailEntry>(image => image.Path);
    }

    public ThumbnailEntry? GetThumbnail(int id)
    {
        using var db = OpenConnection();

        return db.FindWithQuery<ThumbnailEntry?>("SELECT Id, Size, Width, Height, Path, Data FROM ThumbnailEntry WHERE Id = ?", id);
    }

    public void AddThumbnail(ThumbnailEntry image)
    {
        using var db = OpenConnection();
        db.Insert(image);
        db.Close();
    }

    public void UpdateThumbnail(ThumbnailEntry image)
    {
        using var db = OpenConnection();
        db.Update(image);
        db.Close();
    }

    public void DeleteThumbnail(int id)
    {
        using var db = OpenConnection();

        var query = "DELETE FROM ThumbnailEntry WHERE Id = @Id";

        var command = db.CreateCommand(query);
        command.Bind("@Id", id);

        command.ExecuteNonQuery();
    }

    public void RemoveThumbnails(IEnumerable<int> ids)
    {
        using var db = OpenConnection();

        db.BeginTransaction();


        var query = "DELETE FROM ThumbnailEntry WHERE Id = @Id";
        var command = db.CreateCommand(query);

        foreach (var id in ids)
        {
            command.Bind("@Id", id);
            command.ExecuteNonQuery();
        }

        db.Commit();
    }
}
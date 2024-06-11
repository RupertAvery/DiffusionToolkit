namespace Diffusion.Database;

public class ThumbnailCache : SQLiteDB
{

    public ThumbnailCache(string databasePath)
    {
        DatabasePath = databasePath;
    }

    public override async Task Create()
    {
        var databaseDir = Path.GetDirectoryName(DatabasePath);

        if (!Directory.Exists(databaseDir))
        {
            Directory.CreateDirectory(databaseDir);
        }

        using var db = OpenConnection();
        db.CreateTable<ThumbnailEntry>();
        db.CreateIndex<ThumbnailEntry>(image => image.Path);
        db.CreateIndex<ThumbnailEntry>(image => image.CreatedDate);
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

        var sql = "DELETE FROM ThumbnailEntry WHERE Id = @Id";

        var command = db.CreateCommand(sql);
        command.Bind("@Id", id);

        command.ExecuteNonQuery();
    }

    public void RemoveThumbnails(IEnumerable<int> ids)
    {
        using var db = OpenConnection();

        db.BeginTransaction();


        var sql = "DELETE FROM ThumbnailEntry WHERE Id = @Id";
        var command = db.CreateCommand(sql);

        foreach (var id in ids)
        {
            command.Bind("@Id", id);
            command.ExecuteNonQuery();
        }

        db.Commit();
    }

    public void RemoveAllThumbnails()
    {
        using var db = OpenConnection();

        var sql = "DELETE FROM ThumbnailEntry";
        var command = db.CreateCommand(sql);

        command.ExecuteNonQuery();
    }


    public void RemoveThumbnailsByAge(int age)
    {
        using var db = OpenConnection();

        var expiryDate = DateTime.Now.Subtract(TimeSpan.FromDays(age));

        var sql = "DELETE FROM ThumbnailEntry WHERE CreatedDate < @ExpiryDate";
        var command = db.CreateCommand(sql);

        command.Bind("@ExpiryDate", expiryDate);
        command.ExecuteNonQuery();
    }
}
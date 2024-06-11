using SQLite;

namespace Diffusion.Database;

public abstract class SQLiteDB
{
    public string DatabasePath { get; protected set; }

    public SQLiteConnection OpenConnection()
    {
        var db = new SQLiteConnection(DatabasePath);
        return db;
    }

    public void CompactDatabase()
    {
        using var db = OpenConnection();

        var sql = "VACUUM";
        var command = db.CreateCommand(sql);

        command.ExecuteNonQuery();
    }

    public abstract Task Create();

}
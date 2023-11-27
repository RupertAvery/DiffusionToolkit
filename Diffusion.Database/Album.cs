using SQLite;

namespace Diffusion.Database;

public class Album
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public int Order { get; set; }
    public DateTime LastUpdated { get; set; }

}
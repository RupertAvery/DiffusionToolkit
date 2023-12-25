using SQLite;

namespace Diffusion.Database;

public class Migration
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
}
using SQLite;

namespace Diffusion.Database;

public class ExcludeFolder
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed(Name = "ExcludedPath", Unique = true)]
    public string Path { get; set; }
}
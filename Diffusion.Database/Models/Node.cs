using SQLite;

namespace Diffusion.Database.Models;

public class Node
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ImageId { get; set; }
    public string NodeId { get; set; }
    public string Name { get; set; }    
}
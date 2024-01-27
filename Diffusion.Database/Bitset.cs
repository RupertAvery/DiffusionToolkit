using SQLite;

namespace Diffusion.Database;

public class Bitset
{
    [PrimaryKey]
    public int Id { get; set; }
    public byte[] Data { get; set; }

}
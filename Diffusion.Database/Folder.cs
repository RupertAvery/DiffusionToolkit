using Dapper;
using SQLite;

namespace Diffusion.Database
{
    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Path { get; set; }
        public int ImageCount { get; set; }
        public DateTime ScannedDate { get; set; }
    }
}
using Dapper;
using SQLite;

namespace Diffusion.Database
{
    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Path { get; set; }
        public int ImageCount { get; set; }
        public DateTime ScannedDate { get; set; }
    }

    public class Album
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }

    public class AlbumImage
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public int ImageId { get; set; }
    }

}
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

    public class ImageList
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }

    public class ImageListItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ListId { get; set; }
        public int ImageId { get; set; }
    }

}
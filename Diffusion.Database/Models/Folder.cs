using Dapper;
using SQLite;

namespace Diffusion.Database.Models
{
    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Path { get; set; }
        public int ImageCount { get; set; }
        public DateTime ScannedDate { get; set; }
        public bool Unavailable { get; set; }
        public bool Archived { get; set; }
        public bool Excluded { get; set; }
        public bool IsRoot { get; set; }
        public bool Recursive { get; set; }
        public bool Watched { get; set; }
    }

    public class FolderView : Folder
    {
        public bool HasChildren { get; set; }
    }
}
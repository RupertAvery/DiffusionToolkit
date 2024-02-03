using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Diffusion.Database
{
    public partial class DataStore
    {
        public void AddImage(SQLiteConnection connection, Image image)
        {
            connection.Insert(image);
        }

        public void AddImage(Image image)
        {
            using var db = OpenConnection();
            db.Insert(image);
            db.Close();
        }

        public void DeleteImage(int id)
        {
            using var db = OpenConnection();
            
            var query = "DELETE FROM AlbumImage WHERE ImageId = @Id";

            var command = db.CreateCommand(query);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();

            query = "DELETE FROM Image WHERE Id = @Id";

            command = db.CreateCommand(query);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }

        public void RemoveImages(IEnumerable<int> ids)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var albumQuery = "DELETE FROM AlbumImage WHERE ImageId = @Id";
            var albumCommand = db.CreateCommand(albumQuery);

            var query = "DELETE FROM Image WHERE Id = @Id";
            var command = db.CreateCommand(query);

            foreach (var id in ids)
            {
                albumCommand.Bind("@Id", id);
                albumCommand.ExecuteNonQuery();

                command.Bind("@Id", id);
                command.ExecuteNonQuery();
            }

            db.Commit();
        }

        public IEnumerable<ImagePath> GetMarkedImagePaths()
        {
            //List<ImagePath> paths = new List<ImagePath>();

            using var db = OpenConnection();

            var images = db.Query<ImagePath>("SELECT Id, Path FROM Image WHERE ForDeletion = 1");

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();

            //return paths;
        }

        public IEnumerable<ImagePath> GetAllPathImages(string path)
        {
            using var db = OpenConnection();

            var images = db.Query<ImagePath>("SELECT Id, FolderId, Path FROM Image WHERE PATH LIKE ? || '%'", path);

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        public IEnumerable<ImagePath> GetFolderImages(int folderId)
        {
            using var db = OpenConnection();

            var images =  db.Query<ImagePath>("SELECT Id, FolderId, Path FROM Image WHERE FolderId = ?", folderId);

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        //public int DeleteMarkedImages()
        //{
        //    using var db = OpenConnection();

        //    var query = "DELETE FROM Image WHERE ForDeletion = 1";

        //    var command = db.CreateCommand(query);
        //    var result = command.ExecuteNonQuery();

        //    return result;
        //}

        public int UpdateImagesByPath(IEnumerable<Image> images, IEnumerable<string> includeProperties, Dictionary<string, int> folderIdCache, CancellationToken cancellationToken)
        {
            var updated = 0;

            using var db = OpenConnection();

            db.BeginTransaction();

            var exclude = new string[]
            {
                nameof(Image.Id),
                nameof(Image.CustomTags),
                nameof(Image.Rating),
                nameof(Image.Favorite),
                nameof(Image.ForDeletion),
                nameof(Image.NSFW)
            };


            exclude = exclude.Except(includeProperties).ToArray();

            var properties = typeof(Image).GetProperties().Where(p => !exclude.Contains(p.Name)).ToList();

            var query = "UPDATE Image SET ";
            var setList = new List<string>();

            foreach (var property in properties.Where(p => p.Name != nameof(Image.Path)))
            {
                if (property.Name == nameof(Image.NSFW))
                {
                    setList.Add($"{property.Name} = {property.Name} OR {property.Name}");
                }
                else
                {
                    setList.Add($"{property.Name} = @{property.Name}");
                }
            }

            query += string.Join(", ", setList);

            query += " WHERE Path = @Path";

            var command = db.CreateCommand(query);

            foreach (var image in images)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var dirName = Path.GetDirectoryName(image.Path);
                var fileName = Path.GetFileName(image.Path);

                if (!folderIdCache.TryGetValue(dirName, out var id))
                {
                    id = AddOrUpdateFolder(db, dirName);
                    folderIdCache.Add(dirName, id);
                }

                image.FolderId = id;


                foreach (var property in properties)
                {
                    command.Bind($"@{property.Name}", property.GetValue(image));
                }

                updated += command.ExecuteNonQuery();
            }

            db.Commit();

            return updated;
        }

        private int AddOrUpdateFolder(SQLiteConnection db, string dirName)
        {
            var query = "SELECT Id FROM Folder WHERE Path = @Path";

            var command = db.CreateCommand(query);

            command.Bind("@Path", dirName);

            var id = command.ExecuteScalar<int?>();

            if (id.HasValue) return id.Value;

            query = $"INSERT INTO {nameof(Folder)} (Path) VALUES (@Path)";

            command = db.CreateCommand(query);

            command.Bind("@Path", dirName);

            command.ExecuteNonQuery();

            var sql = "select last_insert_rowid();";

            command = db.CreateCommand(sql);

            id = command.ExecuteScalar<int>();

            return id.Value;
        }

        public void AddImages(IEnumerable<Image> images, IEnumerable<string> includeProperties, Dictionary<string, int> folderIdCache, CancellationToken cancellationToken)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var fieldList = new List<string>();
            var paramList = new List<string>();

            var exclude = new string[]
            {
                nameof(Image.Id),
                //nameof(Image.CustomTags),
                //nameof(Image.Rating),
                //nameof(Image.Favorite),
                //nameof(Image.ForDeletion),
                //nameof(Image.NSFW)
            };

            exclude = exclude.Except(includeProperties).ToArray();

            var properties = typeof(Image).GetProperties().Where(p => !exclude.Contains(p.Name)).ToList();

            foreach (var property in properties)
            {
                fieldList.Add($"{property.Name}");
                paramList.Add($"@{property.Name}");
            }

            var query =
                $"INSERT INTO Image ({string.Join(", ", fieldList)}) VALUES " +
                $"({string.Join(", ", paramList)}) " +
                $"ON CONFLICT (Path) DO NOTHING ";

            var command = db.CreateCommand(query);

            foreach (var image in images)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var dirName = Path.GetDirectoryName(image.Path);
                var fileName = Path.GetFileName(image.Path);

                if (!folderIdCache.TryGetValue(dirName, out var id))
                {
                    id = AddOrUpdateFolder(db, dirName);
                    folderIdCache.Add(dirName, id);
                }

                image.FolderId = id;


                foreach (var property in properties)
                {
                    command.Bind($"@{property.Name}", property.GetValue(image));
                }
                command.ExecuteNonQuery();
            }

            db.Commit();
        }

        public IEnumerable<Folder> GetFolders()
        {
            using var db = OpenConnection();

            var folders = db.Query<Folder>("SELECT Id, ParentId, Path, ImageCount, ScannedDate FROM Folder");

            foreach (var folder in folders)
            {
                yield return folder;
            }

            db.Close();
        }

        public Folder? GetFolder(string path)
        {
            using var db = OpenConnection();

            var folder = db.FindWithQuery<Folder>("SELECT Id, ParentId, Path, ImageCount, ScannedDate FROM Folder WHERE Path = ?", path);

            return folder;
        }

        public IEnumerable<ImagePath> GetImagePaths()
        {
            //List<ImagePath> paths = new List<ImagePath>();

            using var db = OpenConnection();

            var images = db.Query<ImagePath>("SELECT Id, FolderId, Path FROM Image");

            foreach (var image in images)
            {
                //paths.Add(image);
                yield return image;
            }

            db.Close();

            //return paths;
        }

        public void UpdateImageFolderId(int id, string path, Dictionary<string, int> folderIdCache)
        {
            using var db = OpenConnection();

            var dirName = Path.GetDirectoryName(path);
            
            if (!folderIdCache.TryGetValue(dirName, out var folderId))
            {
                folderId = AddOrUpdateFolder(db, dirName);
                folderIdCache.Add(dirName, folderId);
            }

            db.Execute("UPDATE Image SET FolderId = ? WHERE Id = ?", folderId, id);

            db.Close();
        }

        public void MoveImage(int id, string newPath, Dictionary<string, int> folderIdCache)
        {
            using var db = OpenConnection();

            var dirName = Path.GetDirectoryName(newPath);

            if (!folderIdCache.TryGetValue(dirName, out var folderId))
            {
                folderId = AddOrUpdateFolder(db, dirName);
                folderIdCache.Add(dirName, folderId);
            }

            db.Execute("UPDATE Image SET Path = ?, FolderId = ? WHERE Id = ?", newPath, folderId, id);

            db.Close();
        }

        public SQLiteConnection Open()
        {
            return OpenConnection();
        }

        public void MoveImage(SQLiteConnection db, int id, string newPath, Dictionary<string, int> folderIdCache)
        {
            var dirName = Path.GetDirectoryName(newPath);

            if (!folderIdCache.TryGetValue(dirName, out var folderId))
            {
                folderId = AddOrUpdateFolder(db, dirName);
                folderIdCache.Add(dirName, folderId);
            }

            db.Execute("UPDATE Image SET Path = ?, FolderId = ? WHERE Id = ?", newPath, folderId, id);
        }

        public void MoveImages(IEnumerable<ImagePath> images, string path)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            try
            {
                var query =
                    "UPDATE Image SET Path = @Path WHERE Id = @Id";

                var command = db.CreateCommand(query);

                foreach (var image in images)
                {
                    var fileName = Path.GetFileName(image.Path);
                    var newPath = Path.Join(path, fileName);
                    File.Move(image.Path, newPath);
                    command.Bind("@Path", newPath);
                    command.ExecuteNonQuery();
                }

                db.Commit();
            }
            catch (Exception e)
            {
                db.Rollback();
            }
            finally
            {
                db.Close();
            }
        }

        public int CleanRemovedFolders(IEnumerable<string> watchedFolders)
        {
            using var db = OpenConnection();

            var whereClause = string.Join(" AND ", watchedFolders.Select(f => "PATH NOT LIKE ? || '\\%'"));

            var query = $"DELETE FROM Image WHERE {whereClause}";

            var result = db.Execute(query, watchedFolders.ToArray());

            db.Close();

            return result;
        }
    }
}

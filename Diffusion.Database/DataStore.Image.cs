using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var query = "DELETE FROM Image WHERE Id = @Id";

            var command = db.CreateCommand(query);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }

        public void DeleteImages(IEnumerable<int> ids)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var query = "DELETE FROM Image WHERE Id = @Id";

            var command = db.CreateCommand(query);

            foreach (var id in ids)
            {
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
                //paths.Add(image);
                yield return image;
            }

            db.Close();

            //return paths;
        }

        public int DeleteMarkedImages()
        {
            using var db = OpenConnection();

            var query = "DELETE FROM Image WHERE ForDeletion = 1";

            var command = db.CreateCommand(query);
            var result = command.ExecuteNonQuery();

            return result;
        }

        public void UpdateImagesByPath(IEnumerable<Image> images)
        {
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

            var properties = typeof(Image).GetProperties().Where(p => !exclude.Contains(p.Name)).ToList();

            var query = "UPDATE Image SET ";
            var setList = new List<string>();

            foreach (var property in properties.Where(p => p.Name != nameof(Image.Path)))
            {
                setList.Add($"{property.Name} = @{property.Name}");
            }

            query += string.Join(", ", setList);

            query += " WHERE Path = @Path";

            var command = db.CreateCommand(query);

            foreach (var image in images)
            {
                foreach (var property in properties)
                {
                    command.Bind($"@{property.Name}", property.GetValue(image));
                }
                command.ExecuteNonQuery();
            }

            db.Commit();
        }

        public void AddImages(IEnumerable<Image> images)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var fieldList = new List<string>();
            var paramList = new List<string>();

            var nonProps = new string[] { nameof(Image.Id) }; /*            { "CustomTags", "Rating", "Favorite", "ForDeletion" } */;
            var properties = typeof(Image).GetProperties().Where(p => !nonProps.Contains(p.Name)).ToList();

            foreach (var property in properties)
            {
                fieldList.Add($"{property.Name}");
                paramList.Add($"@{property.Name}");
            }

            var query =
                $"INSERT INTO Image ({string.Join(", ", fieldList)}) VALUES " +
                $"                  ({string.Join(", ", paramList)})";

            var command = db.CreateCommand(query);

            foreach (var image in images)
            {
                foreach (var property in properties)
                {
                    command.Bind($"@{property.Name}", property.GetValue(image));
                }
                command.ExecuteNonQuery();
            }

            db.Commit();
        }

        public IEnumerable<ImagePath> GetImagePaths()
        {
            //List<ImagePath> paths = new List<ImagePath>();

            using var db = OpenConnection();

            var images = db.Query<ImagePath>("SELECT Id, Path FROM Image");

            foreach (var image in images)
            {
                //paths.Add(image);
                yield return image;
            }

            db.Close();

            //return paths;
        }

        public void MoveImage(int id, string newPath)
        {
            using var db = OpenConnection();


            db.Execute("UPDATE Image SET Path = ? WHERE Id = ?", newPath, id);

            db.Close();
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
    }
}

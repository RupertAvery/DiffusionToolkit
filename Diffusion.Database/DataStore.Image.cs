using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        private void InsertIds(SQLiteConnection db, string table, IEnumerable<int> ids)
        {
            var dropTableQuery = $"DROP TABLE IF EXISTS {table}";
            var dropCommand = db.CreateCommand(dropTableQuery);
            dropCommand.ExecuteNonQuery();

            var tempTableQuery = $"CREATE TEMP TABLE {table} (Id INT)";
            var tempCommand = db.CreateCommand(tempTableQuery);
            tempCommand.ExecuteNonQuery();

            var insertQuery = new StringBuilder();
            insertQuery.Append($"INSERT INTO {table} (Id) VALUES ");

            insertQuery.Append(string.Join(",", ids.Select(d => $"({d})")));

            var insertCommand = db.CreateCommand(insertQuery.ToString());
            insertCommand.ExecuteNonQuery();
        }

        public void RemoveImages(IEnumerable<int> ids)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            InsertIds(db, "DeletedIds", ids);

            var propsQuery = "DELETE FROM NodeProperty WHERE NodeId IN (Select Id FROM Node WHERE ImageId IN (SELECT Id FROM DeletedIds))";
            var propsCommand = db.CreateCommand(propsQuery);
            propsCommand.ExecuteNonQuery();

            var nodesQuery = "DELETE FROM Node WHERE ImageId IN (SELECT Id FROM DeletedIds)";
            var nodesCommand = db.CreateCommand(nodesQuery);
            nodesCommand.ExecuteNonQuery();

            var albumQuery = "DELETE FROM AlbumImage WHERE ImageId IN (SELECT Id FROM DeletedIds)";
            var albumCommand = db.CreateCommand(albumQuery);
            albumCommand.ExecuteNonQuery();

            var query = "DELETE FROM Image WHERE Id IN (SELECT Id FROM DeletedIds)";
            var command = db.CreateCommand(query);
            command.ExecuteNonQuery();

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

            var images = db.Query<ImagePath>("SELECT Id, FolderId, Path, Unavailable FROM Image WHERE PATH LIKE ? || '%'", path);

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        public int CountAllPathImages(string path)
        {
            using var db = OpenConnection();

            var count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Image WHERE PATH LIKE ? || '%'", path);
            
            db.Close();

            return count;
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

            //db.BeginTransaction();

            var exclude = new string[]
            {
                nameof(Image.Id),
                nameof(Image.CustomTags),
                nameof(Image.Rating),
                nameof(Image.Favorite),
                nameof(Image.ForDeletion),
                nameof(Image.NSFW),
                nameof(Image.Unavailable),
                nameof(Image.Workflow)
            };


            exclude = exclude.Except(includeProperties).ToArray();

            var properties = typeof(Image).GetProperties().Where(p => !exclude.Contains(p.Name)).ToList();

            //var querySet = new StringBuilder();
            //var command = db.CreateCommand(query);

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


                //foreach (var property in properties)
                //{
                //    command.Bind($"@{property.Name}", property.GetValue(image));
                //}

                var query = "UPDATE Image SET ";
                var setList = new List<string>();

                foreach (var property in properties.Where(p => p.Name != nameof(Image.Path)))
                {
                    var value = property.GetValue(image);

                    if (property.Name == nameof(Image.NSFW))
                    {
                        setList.Add($"{property.Name} = {SqlBoolean((bool)value)} OR {property.Name}");
                    }
                    else
                    {
                        if (value != null)
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                setList.Add($"{property.Name} = '{SqlEscape(value.ToString())}'");
                            }
                            else if (property.PropertyType == typeof(DateTime))
                            {
                                setList.Add($"{property.Name} = {SqlDateTime((DateTime)value)}");
                            }
                            else if (property.PropertyType == typeof(bool))
                            {
                                setList.Add($"{property.Name} = {SqlBoolean((bool)value)}");
                            }
                            else
                            {
                                setList.Add($"{property.Name} = {value}");
                            }
                        }
                        else
                        {
                            setList.Add($"{property.Name} = NULL");
                        }
                    }
                }

                query += string.Join(", ", setList);

                //query += $" WHERE Path = '{SqlEscape(image.Path)}'";
                query += $" WHERE Path = @Path";

                query += " RETURNING Id;";

                //Debug.WriteLine(query);

                //querySet.Append(query);

                //updated += command.ExecuteNonQuery();

                //var squery = $"SELECT * FROM Image WHERE Path = @Path";
                //var scommand = db.CreateCommand(squery);
                //scommand.Bind("@Path", image.Path);
                //var sids = scommand.ExecuteQuery<Image>();


                var command = db.CreateCommand(query);
                command.Bind("@Path", image.Path);
                var ids = command.ExecuteQuery<ReturnId>();

                image.Id = ids[0].Id;

                updated += 1;

            }



            //updated += ids.Count;

            //db.Commit();

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
                //paramList.Add($"@{property.Name}");
            }

            var query = new StringBuilder($"INSERT INTO Image ({string.Join(", ", fieldList)}) VALUES ");

            //var query =
            //    $"INSERT INTO Image ({string.Join(", ", fieldList)}) VALUES " +
            //    $"({string.Join(", ", paramList)}) " +
            //    $"ON CONFLICT (Path) DO NOTHING ";


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


                //foreach (var property in properties)
                //{
                //    command.Bind($"@{property.Name}", property.GetValue(image));
                //}
                query.Append("(");
                foreach (var property in properties)
                {
                    var value = property.GetValue(image);

                    if (value != null)
                    {
                        if (property.PropertyType == typeof(string))
                        {
                            query.Append($"'{SqlEscape(value.ToString())}',");
                        }
                        else if (property.PropertyType == typeof(DateTime))
                        {
                            query.Append($"{SqlDateTime((DateTime)value)},");
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            query.Append($"{SqlBoolean((bool)value)},");
                        }
                        else
                        {
                            query.Append($"{value},");
                        }
                    }
                    else
                    {
                        query.Append($"NULL,");
                    }
                }

                query.Remove(query.Length - 1, 1);
                query.Append("),");

                //command.ExecuteNonQuery();

                //var sql = "select last_insert_rowid();";

                //var pcommand = db.CreateCommand(sql);

                //image.Id = pcommand.ExecuteScalar<int>();
            }

            if (cancellationToken.IsCancellationRequested)
            {
                db.Rollback();
                return;
            }

            query.Remove(query.Length - 1, 1);

            query.Append(" ON CONFLICT (Path) DO NOTHING ");

            query.Append("RETURNING Id ");

            var command = db.CreateCommand(query.ToString());
            var returnIds = command.ExecuteQuery<ReturnId>();

            foreach(var item in images.Zip(returnIds))
            {
                item.First.Id = item.Second.Id;
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

            var folder = db.FindWithQuery<Folder>("SELECT Id, ParentId, Path, ImageCount, ScannedDate, Unavailable FROM Folder WHERE Path = ?", path);

            return folder;
        }

        public void SetFolderUnavailable(string path, bool unavailable)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var query = $"UPDATE Folder SET Unavailable = @Unavailable WHERE Path = @Path";
            var command = db.CreateCommand(query);

            command.Bind("@Unavailable", unavailable);
            command.Bind("@Path", path);
            command.ExecuteNonQuery();

            db.Commit();
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

            db.BeginTransaction();

            // TODO: Remove nodes

            var whereClause = string.Join(" AND ", watchedFolders.Select(f => $"PATH NOT LIKE '{f}\\%'"));

            //first remove matching entries from AlbumImage
            var albumImageQuery = $"DELETE FROM AlbumImage WHERE ImageId IN (SELECT Id FROM Image WHERE {whereClause})";
            var albumImageQueryResult = db.Execute(albumImageQuery);

            var query = $"DELETE FROM Image WHERE {whereClause}";
            var result = db.Execute(query);

            db.Commit();

            db.Close();

            return result;
        }

        public int ChangeFolderPath(string path, string newPath)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var dropTableQuery = $"DROP TABLE IF EXISTS UpdatedIds";
            var dropCommand = db.CreateCommand(dropTableQuery);
            dropCommand.ExecuteNonQuery();

            var tempTableQuery = $"CREATE TEMP TABLE UpdatedIds (Id INT)";
            var tempCommand = db.CreateCommand(tempTableQuery);
            tempCommand.ExecuteNonQuery();

            var insertQuery = "INSERT INTO UpdatedIds SELECT Id FROM Image WHERE PATH LIKE @Path || '\\%'";
            var insertCommand = db.CreateCommand(insertQuery);
            insertCommand.Bind("@Path", path);
            insertCommand.ExecuteNonQuery();

            var updateQuery = "UPDATE Image SET Path = @NewPath || SUBSTR(Path, length(@Path) + 1) WHERE Id IN (SELECT Id FROM UpdatedIds)";
            var updateCommand = db.CreateCommand(updateQuery);
            updateCommand.Bind("@Path", path);
            updateCommand.Bind("@NewPath", newPath);
            var images = updateCommand.ExecuteNonQuery();

            var updateFolderQuery = "UPDATE Folder SET Path = @NewPath WHERE PATH = @Path";
            var updateFolderCommand = db.CreateCommand(updateFolderQuery);
            updateFolderCommand.Bind("@Path", path);
            updateFolderCommand.Bind("@NewPath", newPath);
            updateFolderCommand.ExecuteNonQuery();

            db.Commit();

            db.Close();

            return images;
        }

        public int RemoveFolder(string path)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var dropTableQuery = $"DROP TABLE IF EXISTS DeletedIds";
            var dropCommand = db.CreateCommand(dropTableQuery);
            dropCommand.ExecuteNonQuery();

            var tempTableQuery = $"CREATE TEMP TABLE DeletedIds (Id INT)";
            var tempCommand = db.CreateCommand(tempTableQuery);
            tempCommand.ExecuteNonQuery();

            var insertQuery = "INSERT INTO DeletedIds SELECT Id FROM Image WHERE FolderId IN (SELECT Id FROM Folder WHERE PATH LIKE @Path || '%')";
            var insertCommand = db.CreateCommand(insertQuery);
            insertCommand.Bind("@Path", path);
            insertCommand.ExecuteNonQuery();

            var propsQuery = "DELETE FROM NodeProperty WHERE NodeId IN (Select Id FROM Node WHERE ImageId IN (SELECT Id FROM DeletedIds))";
            var propsCommand = db.CreateCommand(propsQuery);
            propsCommand.ExecuteNonQuery();

            var nodesQuery = "DELETE FROM Node WHERE ImageId IN (SELECT Id FROM DeletedIds)";
            var nodesCommand = db.CreateCommand(nodesQuery);
            nodesCommand.ExecuteNonQuery();

            var albumQuery = "DELETE FROM AlbumImage WHERE ImageId IN (SELECT Id FROM DeletedIds)";
            var albumCommand = db.CreateCommand(albumQuery);
            albumCommand.ExecuteNonQuery();

            var query = "DELETE FROM Image WHERE Id IN (SELECT Id FROM DeletedIds)";
            var command = db.CreateCommand(query);
            var images = command.ExecuteNonQuery();

            var deletFolderQuery = "DELETE FROM Folder WHERE PATH = @Path";
            var deleteFolderCommand = db.CreateCommand(deletFolderQuery);
            deleteFolderCommand.Bind("@Path", path);
            deleteFolderCommand.ExecuteNonQuery();

            db.Commit();

            db.Close();

            return images;
        }
    }
}

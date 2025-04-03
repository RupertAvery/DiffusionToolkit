using SQLite;
using System.Text;
using Diffusion.Common;

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
            RemoveImages(new[] { id });
        }

        private string InsertIds(SQLiteConnection db, string table, IEnumerable<int> ids)
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

            return $"(SELECT Id FROM {table})";
        }

        private string InsertIds(SQLiteConnection db, string table, string whereClause, Dictionary<string, object>? bindings = null)
        {
            var dropTableQuery = $"DROP TABLE IF EXISTS {table}";
            var dropCommand = db.CreateCommand(dropTableQuery);
            dropCommand.ExecuteNonQuery();

            var tempTableQuery = $"CREATE TEMP TABLE {table} (Id INT)";
            var tempCommand = db.CreateCommand(tempTableQuery);
            tempCommand.ExecuteNonQuery();

            var insertQuery = $"INSERT INTO {table} SELECT Id FROM Image WHERE {whereClause}";
            var insertCommand = db.CreateCommand(insertQuery);

            if (bindings != null)
            {
                foreach (var binding in bindings)
                {
                    insertCommand.Bind(binding.Key, binding.Value);
                }
            }

            insertCommand.ExecuteNonQuery();

            return $"(SELECT Id FROM {table})";
        }

        public void RemoveImages(IEnumerable<int> ids)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var deletedIds = InsertIds(db, "DeletedIds", ids);

            var propsQuery = $"DELETE FROM NodeProperty WHERE NodeId IN (Select Id FROM Node WHERE ImageId IN {deletedIds})";
            var propsCommand = db.CreateCommand(propsQuery);
            propsCommand.ExecuteNonQuery();

            var nodesQuery = $"DELETE FROM Node WHERE ImageId IN {deletedIds}";
            var nodesCommand = db.CreateCommand(nodesQuery);
            nodesCommand.ExecuteNonQuery();

            var albumQuery = $"DELETE FROM AlbumImage WHERE ImageId IN {deletedIds}";
            var albumCommand = db.CreateCommand(albumQuery);
            albumCommand.ExecuteNonQuery();

            var query = $"DELETE FROM Image WHERE Id IN {deletedIds}";
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

            var images = db.Query<ImagePath>("SELECT Id, FolderId, Path FROM Image WHERE FolderId = ?", folderId);

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

            var setProperties = properties.Where(p => p.Name != nameof(Image.Path)).ToList();

            var query = new StringBuilder("UPDATE Image SET ");
            var setList = new List<string>();

            foreach (var property in setProperties)
            {
                if (property.Name == nameof(Image.NSFW))
                {
                    setList.Add($"{property.Name} = ? OR {property.Name}");
                }
                else
                {
                    setList.Add($"{property.Name} = ?");
                }
            }

            query.Append(string.Join(", ", setList));
            query.Append(" WHERE Path = ? RETURNING Id;");
            var updateQuery = query.ToString();

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

                var values = new List<object>();

                foreach (var property in setProperties)
                {
                    var value = property.GetValue(image);
                    values.Add(value);
                }

                values.Add(image.Path);

                try
                {
                    var command = db.CreateCommand(updateQuery, values.ToArray());
                    var ids = command.ExecuteQuery<ReturnId>();
                    image.Id = ids[0].Id;
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                    Logger.Log(e.StackTrace);
                    Logger.Log(updateQuery);
                    Logger.Log(string.Join("\r\n", values));
                    throw;
                }

                updated += 1;
            }
            
            db.Close();

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

            var exclude = new string[]
            {
                nameof(Image.Id),
                nameof(Image.CustomTags),
                nameof(Image.Rating),
                // Make sure these values are populated with 0
                // so DO NOT Exclude them
                //nameof(Image.Favorite),
                //nameof(Image.ForDeletion),
                //nameof(Image.NSFW),
                //nameof(Image.Unavailable),
                nameof(Image.Workflow)
            };

            exclude = exclude.Except(includeProperties).ToArray();

            var properties = typeof(Image).GetProperties().Where(p => !exclude.Contains(p.Name)).ToList();

            foreach (var property in properties)
            {
                fieldList.Add($"{property.Name}");
            }

            var propertySet = $"({string.Join(",", properties.Select(p => "?"))})";

            var query = new StringBuilder($"INSERT INTO Image ({string.Join(", ", fieldList)}) VALUES ");

            var valueGroups = new List<string>();
            var values = new List<object>();

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
                    var value = property.GetValue(image);

                    values.Add(value);
                }

                valueGroups.Add(propertySet);
            }

            query.Append(string.Join(", ", valueGroups));
            query.Append(" ON CONFLICT (Path) DO NOTHING RETURNING Id;");

            if (cancellationToken.IsCancellationRequested)
            {
                db.Rollback();
                return;
            }

            try
            {
                var command = db.CreateCommand(query.ToString(), values.ToArray());

                var returnIds = command.ExecuteQuery<ReturnId>();

                foreach (var item in images.Zip(returnIds))
                {
                    item.First.Id = item.Second.Id;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
                Logger.Log(e.StackTrace);
                Logger.Log(query.ToString());
                Logger.Log(string.Join("\r\n", values));
                throw;
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

            var whereClause = string.Join(" AND ", watchedFolders.Select(f => $"PATH NOT LIKE '{f}\\%'"));

            var deletedIds = InsertIds(db, "DeletedIds", whereClause);

            var propsQuery = $"DELETE FROM NodeProperty WHERE NodeId IN (Select Id FROM Node WHERE ImageId IN {deletedIds})";
            var propsCommand = db.CreateCommand(propsQuery);
            propsCommand.ExecuteNonQuery();

            var nodesQuery = $"DELETE FROM Node WHERE ImageId IN {deletedIds}";
            var nodesCommand = db.CreateCommand(nodesQuery);
            nodesCommand.ExecuteNonQuery();

            var albumQuery = $"DELETE FROM AlbumImage WHERE ImageId IN {deletedIds}";
            var albumCommand = db.CreateCommand(albumQuery);
            albumCommand.ExecuteNonQuery();

            var query = $"DELETE FROM Image WHERE Id IN {deletedIds}";
            var command = db.CreateCommand(query);
            var images = command.ExecuteNonQuery();

            db.Commit();

            db.Close();

            return images;
        }

        public int ChangeFolderPath(string path, string newPath)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var updatedIds = InsertIds(db, "UpdatedIds", "PATH LIKE @Path || '\\%'", new Dictionary<string, object>() { { "@Path", path } });

            var updateQuery = $"UPDATE Image SET Path = @NewPath || SUBSTR(Path, length(@Path) + 1) WHERE Id IN {updatedIds}";
            var updateCommand = db.CreateCommand(updateQuery);
            updateCommand.Bind("@Path", path);
            updateCommand.Bind("@NewPath", newPath);
            var images = updateCommand.ExecuteNonQuery();

            var updateSubQuery = "UPDATE Folder SET Path = @NewPath || SUBSTR(Path, length(@Path) + 1) WHERE PATH LIKE @Path || '\\%'";
            var updateSubCommand = db.CreateCommand(updateSubQuery);
            updateSubCommand.Bind("@Path", path);
            updateSubCommand.Bind("@NewPath", newPath);
            updateSubCommand.ExecuteNonQuery();

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

            var deletedIds = InsertIds(db, "DeletedIds", "FolderId IN (SELECT Id FROM Folder WHERE PATH LIKE @Path || '%')", new Dictionary<string, object>() { { "@Path", path } });


            //var dropTableQuery = $"DROP TABLE IF EXISTS DeletedIds";
            //var dropCommand = db.CreateCommand(dropTableQuery);
            //dropCommand.ExecuteNonQuery();

            //var tempTableQuery = $"CREATE TEMP TABLE DeletedIds (Id INT)";
            //var tempCommand = db.CreateCommand(tempTableQuery);
            //tempCommand.ExecuteNonQuery();

            //var insertQuery = "INSERT INTO DeletedIds SELECT Id FROM Image WHERE FolderId IN (SELECT Id FROM Folder WHERE PATH LIKE @Path || '%')";
            //var insertCommand = db.CreateCommand(insertQuery);
            //insertCommand.Bind("@Path", path);
            //insertCommand.ExecuteNonQuery();

            var propsQuery = $"DELETE FROM NodeProperty WHERE NodeId IN (Select Id FROM Node WHERE ImageId IN {deletedIds})";
            var propsCommand = db.CreateCommand(propsQuery);
            propsCommand.ExecuteNonQuery();

            var nodesQuery = $"DELETE FROM Node WHERE ImageId IN {deletedIds}";
            var nodesCommand = db.CreateCommand(nodesQuery);
            nodesCommand.ExecuteNonQuery();

            var albumQuery = $"DELETE FROM AlbumImage WHERE ImageId IN {deletedIds}";
            var albumCommand = db.CreateCommand(albumQuery);
            albumCommand.ExecuteNonQuery();

            var query = $"DELETE FROM Image WHERE Id IN {deletedIds}";
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

using System.Reflection;
using SQLite;
using System.Text;
using Diffusion.Common;
using Diffusion.Database.Models;

namespace Diffusion.Database
{
    public class UpdateViewed
    {
        public int Id { get; set; }
        public DateTime Viewed { get; set; }
    }

    public partial class DataStore
    {
        public void RemoveImage(int id)
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

        private string CustomInsertIds(SQLiteConnection db, string table, string insertQuery, Dictionary<string, object>? bindings = null)
        {
            var dropTableQuery = $"DROP TABLE IF EXISTS {table}";
            var dropCommand = db.CreateCommand(dropTableQuery);
            dropCommand.ExecuteNonQuery();

            var tempTableQuery = $"CREATE TEMP TABLE {table} (Id INT)";
            var tempCommand = db.CreateCommand(tempTableQuery);
            tempCommand.ExecuteNonQuery();

            insertQuery = insertQuery.Replace("{table}", table);
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

        private Dictionary<string, PropertyInfo[]> _propertiesCache = new Dictionary<string, PropertyInfo[]>();
        
        private (string, string, PropertyInfo[]) InsertTable<T>(SQLiteConnection db, string table, IEnumerable<T> views)
        {
            var dropTableQuery = $"DROP TABLE IF EXISTS {table}";
            var dropCommand = db.CreateCommand(dropTableQuery);
            dropCommand.ExecuteNonQuery();

            var key = typeof(T).FullName;

            if (!_propertiesCache.TryGetValue(key, out var props))
            {
                props = typeof(T).GetProperties();
                _propertiesCache[key] = props;
            }
            
            var propNamesList = new List<string>();
            var columnsList = new List<string>();
            
            foreach (var prop in props)
            {
                var columnType = prop.PropertyType.Name switch
                {
                    "String" => "varchar",
                    "Int32" => "INT",
                    "Int64" => "INT",
                    "DateTime" => "bigint",
                    _ => ""
                };

                propNamesList.Add(prop.Name);
                columnsList.Add($"{prop.Name} {columnType}");

            }

            var tempTableQuery = $"CREATE TEMP TABLE {table} ({string.Join(",", columnsList)})";
            var tempCommand = db.CreateCommand(tempTableQuery);
            tempCommand.ExecuteNonQuery();

            var insertQuery = new StringBuilder();
            insertQuery.Append($"INSERT INTO {table} ({string.Join(",", propNamesList)}) VALUES ");

            var valuesList = new List<string>();
            var bindings = new List<object>();
            var placeHoldersList = new List<string>();

            foreach (var prop in props)
            {
                placeHoldersList.Add("?");
            }

            var valuePlaceholder = $"({string.Join(", ", placeHoldersList)})";

            foreach (var view in views)
            {
                foreach (var prop in props)
                {
                    bindings.Add(prop.GetValue(view));
                }
                valuesList.Add(valuePlaceholder);
            }

            insertQuery.Append(string.Join(", ", valuesList));

            db.Execute(insertQuery.ToString(), bindings.ToArray());

            var alias = "c";

            return ($"(SELECT {string.Join(", ", propNamesList)} FROM {table}) {alias}", alias, props);
        }

        public void RemoveImages(IEnumerable<int> ids)
        {
            using var db = OpenConnection();

            lock (_lock)
            {
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

        }

        public IEnumerable<ImagePath> GetImagesTaggedForDeletion()
        {
            var db = OpenReadonlyConnection();

            var images = db.Query<ImagePath>("SELECT Id, Path FROM Image WHERE ForDeletion = 1");

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        public IEnumerable<ImagePath> GetAllPathImages(string path)
        {
            var db = OpenReadonlyConnection();

            var images = db.Query<ImagePath>("SELECT Id, FolderId, Path, Unavailable FROM Image WHERE PATH LIKE ? || '%'", path);

            foreach (var image in images)
            {
                yield return image;
            }
        }

        public int CountAllPathImages(string path)
        {
            var db = OpenReadonlyConnection();

            var count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Image WHERE PATH LIKE ? || '%'", path);

            return count;
        }

        public IEnumerable<ImagePath> GetFolderImages(int folderId, bool recursive)
        {
            var db = OpenReadonlyConnection();

            var query = recursive
                ? $"{DirectoryTreeCTE} SELECT Id, FolderId, Path FROM Image WHERE FolderId IN (SELECT Id FROM directoryTree WHERE RootId = ?)"
                : "SELECT Id, FolderId, Path FROM Image WHERE FolderId = ?";

            var images = db.Query<ImagePath>(query, folderId);

            foreach (var image in images)
            {
                yield return image;
            }

        }

        public int UpdateImagesByPath(SQLiteConnection db, IEnumerable<Image> images, IEnumerable<string> includeProperties, Dictionary<string, Folder> folderCache, CancellationToken cancellationToken)
        {
            var updated = 0;

            // These are user-defined metadata and should NOT be overwritten when the user
            // re-scans the images
            var exclude = new string[]
            {
                nameof(Image.Id),
                nameof(Image.CustomTags),
                nameof(Image.Rating),
                nameof(Image.Favorite),
                nameof(Image.ForDeletion),
                nameof(Image.NSFW),
                nameof(Image.Unavailable),
                nameof(Image.Workflow),
                nameof(Image.ViewedDate),
                nameof(Image.TouchedDate),
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

                if (!EnsureFolderExists(db, dirName, folderCache, out var folderId))
                {
                    Logger.Log($"Root folder not found for {dirName}");
                    image.HasError = true;
                }
                else
                {
                    image.FolderId = folderId;
                }


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
                    lock (_lock)
                    {
                        var ids = command.ExecuteQuery<ReturnId>();
                        image.Id = ids[0].Id;
                    }
                    updated += 1;
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                    Logger.Log(e.StackTrace);
                    Logger.Log(updateQuery);
                    Logger.Log(string.Join("\r\n", values));
                    throw;
                }

            }

            return updated;
        }

        public int AddImages(SQLiteConnection db, IEnumerable<Image> images, IEnumerable<string> includeProperties, Dictionary<string, Folder> folderCache, CancellationToken cancellationToken)
        {
            int added = 0;

            var fieldList = new List<string>();

            var exclude = new string[]
            {
                nameof(Image.Id),
                nameof(Image.CustomTags),
                nameof(Image.Rating),
                // Make sure these values are populated with 0 (False)
                // so DO NOT Exclude them
                //nameof(Image.Favorite),
                //nameof(Image.ForDeletion),
                //nameof(Image.NSFW),
                //nameof(Image.Unavailable),
                nameof(Image.Workflow),
                nameof(Image.Rating),
                nameof(Image.ViewedDate),
                nameof(Image.TouchedDate),
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

                if (!EnsureFolderExists(db, dirName, folderCache, out var folderId))
                {
                    Logger.Log($"Root folder not found for {dirName}");
                    image.HasError = true;
                }

                image.FolderId = folderId;

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
                return 0;
            }

            try
            {
                var command = db.CreateCommand(query.ToString(), values.ToArray());

                List<ReturnId> returnIds;

                lock (_lock)
                {
                    returnIds = command.ExecuteQuery<ReturnId>();
                }

                foreach (var item in images.Zip(returnIds))
                {
                    item.First.Id = item.Second.Id;
                }

                added = returnIds.Count;

            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
                Logger.Log(e.StackTrace);
                Logger.Log(query.ToString());
                Logger.Log(string.Join("\r\n", values));
                throw;
            }

            return added;
        }

        public IEnumerable<ImagePath> GetImagePaths()
        {
            var db = OpenReadonlyConnection();

            var images = db.Query<ImagePath>("SELECT Id, FolderId, Path FROM Image");

            foreach (var image in images)
            {
                yield return image;
            }
        }

        public IEnumerable<ImagePath> GetImagePaths(IEnumerable<int> ids)
        {
            var db = OpenReadonlyConnection();

            var selectedIds = InsertIds(db, "SelectedIds", ids);

            var images = db.Query<ImagePath>($"SELECT Id, FolderId, Path FROM Image WHERE Id IN {selectedIds}");

            foreach (var image in images)
            {
                yield return image;
            }
        }

        public void UpdateImageFolderId(int id, string path, Dictionary<string, Folder> folderCache)
        {
            using var db = OpenConnection();

            var dirName = Path.GetDirectoryName(path);

            if (!EnsureFolderExists(db, dirName, folderCache, out var folderId))
            {
                Logger.Log($"Root folder not found for {dirName}");
            }

            lock (_lock)
            {
                db.Execute("UPDATE Image SET FolderId = ? WHERE Id = ?", folderId, id);
            }

            db.Close();
        }

        public void MoveImage(int id, string newPath, Dictionary<string, Folder> folderCache)
        {
            using var db = OpenConnection();

            var dirName = Path.GetDirectoryName(newPath);

            if (!EnsureFolderExists(db, dirName, folderCache, out var folderId))
            {
                Logger.Log($"Root folder not found for {dirName}");
            }

            lock (_lock)
            {
                db.Execute("UPDATE Image SET Path = ?, FolderId = ? WHERE Id = ?", newPath, folderId, id);
            }

            db.Close();
        }

        public SQLiteConnection Open()
        {
            return OpenConnection();
        }

        public void MoveImage(SQLiteConnection db, int id, string newPath, Dictionary<string, Folder> folderCache)
        {
            var dirName = Path.GetDirectoryName(newPath);

            if (!EnsureFolderExists(db, dirName, folderCache, out var folderId))
            {
                Logger.Log($"Root folder not found for {dirName}");
            }

            lock (_lock)
            {
                db.Execute("UPDATE Image SET Path = ?, FolderId = ? WHERE Id = ?", newPath, folderId, id);
            }
        }

        //public void MoveImages(IEnumerable<ImagePath> images, string path)
        //{
        //    using var db = OpenConnection();

        //    db.BeginTransaction();

        //    try
        //    {
        //        var query =
        //            "UPDATE Image SET Path = @Path WHERE Id = @Id";

        //        var command = db.CreateCommand(query);

        //        foreach (var image in images)
        //        {
        //            var fileName = Path.GetFileName(image.Path);
        //            var newPath = Path.Join(path, fileName);
        //            File.Move(image.Path, newPath);
        //            command.Bind("@Path", newPath);
        //            command.ExecuteNonQuery();
        //        }

        //        db.Commit();
        //    }
        //    catch (Exception e)
        //    {
        //        db.Rollback();
        //    }
        //    finally
        //    {
        //        db.Close();
        //    }
        //}

        public bool ImageExists(string path)
        {
            var db = OpenReadonlyConnection();

            var result = db.ExecuteScalar<int>("SELECT COUNT(1) FROM Image WHERE Path = ?", path);

            return result > 0;
        }

        public IReadOnlyCollection<HashMatch> GetImageIdByHash(string hash)
        {
            var db = OpenReadonlyConnection();

            return db.Query<HashMatch>("SELECT Id, Path FROM Image WHERE Hash = ?", hash);
        }

        public int UpdateImagePath(int id, string path)
        {
            lock (_lock)
            {
                using var db = OpenConnection();
                return db.Execute("UPDATE Image SET Path = ? WHERE Id = ?", path, id);
            }
        }

        public int UpdateViewed(int id)
        {
            lock (_lock)
            {
                using var db = OpenConnection();
                return db.Execute("UPDATE Image SET ViewedDate = ? WHERE Id = ?", DateTime.Now, id);
            }
        }

        public int UpdateViewed(IReadOnlyCollection<UpdateViewed> views)
        {
            lock (_lock)
            {
                using var db = OpenConnection();

                var (table, alias, props) = InsertTable(db, "UpdateViews", views);

                return db.Execute($"UPDATE Image SET ViewedDate = {alias}.Viewed FROM {table} WHERE {alias}.Id = Image.Id");
            }
        }

        public int UpdateImageFilename(int id, string path, string filename)
        {

            lock (_lock)
            {
                using var db = OpenConnection();
                return db.Execute("UPDATE Image SET Path = ?, FileName = ? WHERE Id = ?", path, filename, id);
            }
        }
    }

    public class HashMatch
    {
        public int Id { get; set; }
        public string Path { get; set; }
    }
}

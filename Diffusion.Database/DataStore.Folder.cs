using System.Net.Sockets;
using SQLite;
using System.Text;
using Diffusion.Common;
using Diffusion.Database.Models;

namespace Diffusion.Database
{
    public partial class DataStore
    {
        private string DirectoryTreeWithPathCTE => @"WITH RECURSIVE
	directoryTree(Id, ParentId, Path, RootId, Depth) AS
	(
		SELECT Id, ParentId, Path, Id AS RootId, 0 AS Depth FROM Folder 
		UNION ALL
		SELECT f.Id, f.ParentId, f.Path, t.RootId, t.Depth + 1 FROM Folder f JOIN directoryTree t ON f.ParentId = t.Id
	)";

        private string DirectoryTreeCTE => @"WITH RECURSIVE
	directoryTree(Id, ParentId, RootId, Depth) AS
	(
		SELECT Id, ParentId, Id AS RootId, 0 AS Depth FROM Folder 
		UNION ALL
		SELECT f.Id, f.ParentId, t.RootId, t.Depth + 1 FROM Folder f JOIN directoryTree t ON f.ParentId = t.Id
	)";


        public void SetFolderExcluded(string path, bool excluded, bool recursive)
        {
            using var db = OpenConnection();

            if (EnsureFolderExists(db, path, null, out var folderId))
            {
                var query = recursive
                    ? $"{DirectoryTreeCTE} UPDATE Folder SET Excluded = ? FROM (SELECT Id FROM directoryTree WHERE RootId = ?) AS Subfolder WHERE Folder.Id = Subfolder.Id"
                    : "UPDATE Folder SET Excluded = ? WHERE Id = ?";

                db.Execute(query, excluded, folderId);
            }

            db.Close();

        }

        public void SetFolderExcluded(int id, bool excluded, bool recursive)
        {
            using var db = OpenConnection();

            var query = recursive
                ? $"{DirectoryTreeCTE} UPDATE Folder SET Excluded = ? FROM (SELECT Id FROM directoryTree WHERE RootId = ?) AS Subfolder WHERE Folder.Id = Subfolder.Id"
                : "UPDATE Folder SET Excluded = ? WHERE Id = ?";

            lock (_lock)
            {
                db.Execute(query, excluded, id);
            }

            db.Close();
        }

        public void SetFolderUnavailable(int id, bool unavailable, bool recursive)
        {
            using (var db = OpenConnection())
            {
                var query = recursive
                    ? $"{DirectoryTreeCTE} UPDATE Folder SET Unavailable = ? FROM (SELECT Id FROM directoryTree WHERE RootId = ?) AS Subfolder WHERE Folder.Id = Subfolder.Id"
                    : "UPDATE Folder SET Unavailable = ? WHERE Id = ?";

                lock (_lock)
                {
                    db.Execute(query, unavailable, id);
                }

                // Set files unavailable
                //query = recursive
                //    ? $"{DirectoryTreeCTE} UPDATE Image SET Unavailable = ? FROM (SELECT i.Id FROM Image i JOIN directoryTree d on i.FolderId = d.Id WHERE d.RootId = ?) AS RefImage WHERE Image.Id = RefImage.Id"
                //    : "";

                //db.Execute(query, unavailable, id);
            }
        }

        public void SetFolderArchived(int id, bool archived, bool recursive)
        {
            using var db = OpenConnection();

            var query = recursive
                ? $"{DirectoryTreeCTE} UPDATE Folder SET Archived = ? FROM (SELECT Id FROM directoryTree WHERE RootId = ?) AS Subfolder WHERE Folder.Id = Subfolder.Id"
                : "UPDATE Folder SET Archived = ? WHERE Id = ?";

            lock (_lock)
            {
                db.Execute(query, archived, id);
            }

            db.Close();
        }

        // TODO: Per-folder Recursive?
        public int AddRootFolder(string path, bool recursive)
        {
            using var db = OpenConnection();

            // ParentId, Path, ImageCount, ScannedDate, Unavailable, Archived, Excluded, IsRoot
            int id;

            lock (_lock)
            {
                id = db.ExecuteScalar<int>($"INSERT INTO Folder ({FolderColumnsSansId}) VALUES (0, ?, 0, NULL, 0, 0, 0, 1)", path);
            }

            db.Close();

            return id;
        }

        public void RemoveRootFolder(int id, bool removeImages)
        {
            using var db = OpenConnection();

            db.Execute("DELETE FROM Folder WHERE Id = ?", id);

            db.Close();
        }

        public static bool EnsureFolderExistsExt(SQLiteConnection db, string path, Dictionary<string, Folder>? folderCache,
            out int folderId)
        {
            if (folderCache?.TryGetValue(path, out var folder) ?? false)
            {
                folderId = folder.Id;
                return true;
            }

            var rootFolders = db.Query<Folder>($"SELECT {FolderColumns} FROM Folder WHERE IsRoot = 1");

            var root = rootFolders.FirstOrDefault(d => path.StartsWith(d.Path));
            if (root == null)
            {
                folderId = -1;
                return false;
            }

            var current = root.Path;
            var currentParentId = root.Id;
            // skip root
            int nextSeparator;
            do
            {

                nextSeparator = path.IndexOf("\\", current.Length + 1, StringComparison.Ordinal);
                current = nextSeparator > 0 ? path[..nextSeparator] : path;

                var existingId = db.ExecuteScalar<int?>("SELECT Id FROM Folder WHERE Path = ?", current);

                if (existingId.HasValue)
                {
                    currentParentId = existingId.Value;
                }
                else
                {
                    var id = db.ExecuteScalar<int>("INSERT INTO Folder (ParentId, Path, Unavailable, Archived, Excluded, IsRoot) VALUES (?, ?, 0, 0, 0, 0) RETURNING Id", currentParentId, current);
                    folderCache?.Add(current, new Folder() { Id = id, ParentId = currentParentId, Path = current });
                    currentParentId = id;
                }


            } while (nextSeparator > 0);

            folderId = currentParentId;

            return true;
        }

        public bool EnsureFolderExists(SQLiteConnection db, string path, Dictionary<string, Folder>? folderCache, out int folderId)
        {
            if (folderCache?.TryGetValue(path, out var folder) ?? false)
            {
                folderId = folder.Id;
                return true;
            }

            var rootFolders = db.Query<Folder>($"SELECT {FolderColumns} FROM Folder WHERE IsRoot = 1");

            var root = rootFolders.FirstOrDefault(d => path.StartsWith(d.Path));
            if (root == null)
            {
                folderId = -1;
                return false;
            }

            var current = root.Path;
            var currentParentId = root.Id;
            // skip root
            int nextSeparator;
            do
            {

                nextSeparator = path.IndexOf("\\", current.Length + 1, StringComparison.Ordinal);
                current = nextSeparator > 0 ? path[..nextSeparator] : path;

                var existingId = db.ExecuteScalar<int?>("SELECT Id FROM Folder WHERE Path = ?", current);

                if (existingId.HasValue)
                {
                    currentParentId = existingId.Value;
                }
                else
                {
                    int id;
                    lock (_lock)
                    {
                        id = db.ExecuteScalar<int>("INSERT INTO Folder (ParentId, Path, Unavailable, Archived, Excluded, IsRoot) VALUES (?, ?, 0, 0, 0, 0) RETURNING Id", currentParentId, current);
                    }
                    folderCache?.Add(current, new Folder() { Id = id, ParentId = currentParentId, Path = current });
                    currentParentId = id;
                }


            } while (nextSeparator > 0);

            folderId = currentParentId;

            return true;
        }


        public IEnumerable<FolderView> GetSubFoldersView(int id)
        {
            var db = OpenReadonlyConnection();

            var query = $@"{DirectoryTreeCTE} SELECT f.Id, f.ParentId, f.Path, ImageCount, ScannedDate, Unavailable, Archived, Excluded, IsRoot, min(coalesce(P.Children, 0),1) AS HasChildren
FROM Folder f JOIN 
directoryTree t ON f.Id = t.Id 
LEFT JOIN (SELECT RootId, COUNT(*) AS Children FROM directoryTree 
WHERE Depth = 1 
GROUP BY RootId) P ON F.Id = P.RootId
WHERE t.RootId = ? AND t.Depth = 1
";
            var folders = db.Query<FolderView>(query, id);

            foreach (var folder in folders)
            {
                yield return folder;
            }

        }

        public IEnumerable<Folder> GetSubfolders(int id)
        {
            var db = OpenReadonlyConnection();

            var folders = db.Query<Folder>($"{DirectoryTreeCTE} SELECT f.Id, f.ParentId, f.Path, ImageCount, ScannedDate, Unavailable, Archived, Excluded, IsRoot FROM Folder f JOIN directoryTree t ON f.Id = t.Id WHERE t.RootId = ? AND t.Depth = 1", id);

            foreach (var folder in folders)
            {
                yield return folder;
            }
        }

        public IEnumerable<Folder> GetDescendants(int id)
        {
            using var db = OpenConnection();

            var folders = db.Query<Folder>($"{DirectoryTreeCTE} SELECT f.Id, f.ParentId, f.Path, ImageCount, ScannedDate, Unavailable, Archived, Excluded, IsRoot FROM Folder f JOIN directoryTree t ON f.Id = t.Id WHERE t.RootId = ?", id);

            foreach (var folder in folders)
            {
                yield return folder;
            }

            db.Close();
        }

        public IEnumerable<Folder> GetExcludedFolders()
        {
            using var db = OpenConnection();

            var folders = db.Query<Folder>($"SELECT {FolderColumns} FROM Folder WHERE Excluded = 1");

            foreach (var folder in folders)
            {
                yield return folder;
            }

            db.Close();
        }

        public IEnumerable<Folder> GetArchivedFolders(bool archived = true)
        {
            using var db = OpenConnection();

            var folders = db.Query<Folder>($"SELECT {FolderColumns} FROM Folder WHERE Archived = ?", archived);

            foreach (var folder in folders)
            {
                yield return folder;
            }

            db.Close();
        }

        public IEnumerable<Folder> GetRootFolders()
        {
            using var db = OpenConnection();

            var folders = db.Query<Folder>($"SELECT {FolderColumns} FROM Folder WHERE IsRoot = 1");

            foreach (var folder in folders)
            {
                yield return folder;
            }

            db.Close();
        }

        private static string FolderColumns = "Id, ParentId, Path, ImageCount, ScannedDate, Unavailable, Archived, Excluded, IsRoot";
        private static string FolderColumnsSansId = "ParentId, Path, ImageCount, ScannedDate, Unavailable, Archived, Excluded, IsRoot";

        public IEnumerable<FolderView> GetFoldersView()
        {
            using var db = OpenConnection();

            var query = $@"WITH RECURSIVE
	directoryTree(Id, ParentId, RootId, Depth) AS
	(
		SELECT Id, ParentId, Id AS RootId, 0 AS Depth FROM Folder 
		UNION ALL
		SELECT f.Id, f.ParentId, t.RootId, t.Depth + 1 FROM Folder f JOIN directoryTree t ON f.ParentId = t.Id
	)
SELECT {FolderColumns}, min(coalesce(P.Children, 0),1) AS HasChildren  FROM Folder F 
LEFT JOIN (SELECT RootId, COUNT(*) AS Children FROM directoryTree WHERE Depth = 1 GROUP BY RootId) P ON F.Id = P.RootId
";

            //$"SELECT {FolderColumns} FROM Folder"

            var folders = db.Query<FolderView>(query);

            foreach (var folder in folders)
            {
                yield return folder;
            }

            db.Close();
        }

        public IEnumerable<Folder> GetFolders()
        {
            using var db = OpenConnection();

            var query = $"SELECT {FolderColumns} FROM Folder";

            var folders = db.Query<FolderView>(query);

            foreach (var folder in folders)
            {
                yield return folder;
            }

            db.Close();
        }

        public Folder? GetFolder(string path)
        {
            using var db = OpenConnection();

            var folder = db.FindWithQuery<Folder>($"SELECT {FolderColumns} FROM Folder WHERE Path = ?", path);

            return folder;
        }

        //public void SetFolderUnavailable(string path, bool unavailable, bool recursive)
        //{
        //    using var db = OpenConnection();

        //    db.BeginTransaction();

        //    var query = recursive 
        //        ? $"{DirectoryTreeCTE} UPDATE Folder SET Unavailable = ? FROM (SELECT Id FROM directoryTree WHERE RootId = ?) AS Subfolder WHERE Folder.Id = Subfolder.Id"
        //        : "UPDATE Folder SET Unavailable = @Unavailable WHERE Path = @Path";
        //    var command = db.CreateCommand(query);

        //    command.Bind("@Unavailable", unavailable);
        //    command.Bind("@Path", path);
        //    command.ExecuteNonQuery();

        //    db.Commit();

        //    db.Close();

        //    DataChanged?.Invoke(this, new DataChangedEventArgs()
        //    {
        //        EntityType = EntityType.Folder,
        //        SourceType = SourceType.Item,
        //        Property = "Unavailable"
        //    });

        //}


        public int CleanRemovedFolders()
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            // TODO: implement properly
            //var whereClause = string.Join(" AND ", watchedFolders.Select(f => $"PATH NOT LIKE '{f}\\%'"));

            //var deletedIds = InsertIds(db, "DeletedIds", whereClause);

            //var propsQuery = $"DELETE FROM NodeProperty WHERE NodeId IN (Select Id FROM Node WHERE ImageId IN {deletedIds})";
            //var propsCommand = db.CreateCommand(propsQuery);
            //propsCommand.ExecuteNonQuery();

            //var nodesQuery = $"DELETE FROM Node WHERE ImageId IN {deletedIds}";
            //var nodesCommand = db.CreateCommand(nodesQuery);
            //nodesCommand.ExecuteNonQuery();

            //var albumQuery = $"DELETE FROM AlbumImage WHERE ImageId IN {deletedIds}";
            //var albumCommand = db.CreateCommand(albumQuery);
            //albumCommand.ExecuteNonQuery();

            //var query = $"DELETE FROM Image WHERE Id IN {deletedIds}";
            //var command = db.CreateCommand(query);
            //var images = command.ExecuteNonQuery();

            //db.Commit();

            //db.Close();

            return 0;
        }

        public int ChangeFolderPath(string path, string newPath)
        {
            using var db = OpenConnection();

            int imagesUpdated = 0;

            lock (_lock)
            {
                db.BeginTransaction();

                var updatedIds = InsertIds(db, "UpdatedIds", "PATH LIKE @Path || '\\%'", new Dictionary<string, object>() { { "@Path", path } });

                // TODO: Change to use folderIDs?
                var updateQuery = $"UPDATE Image SET Path = @NewPath || SUBSTR(Path, length(@Path) + 1) WHERE Id IN {updatedIds}";
                var updateCommand = db.CreateCommand(updateQuery);
                updateCommand.Bind("@Path", path);
                updateCommand.Bind("@NewPath", newPath);
                imagesUpdated = updateCommand.ExecuteNonQuery();

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
            }

            return imagesUpdated;
        }

        public int RemoveFolder(string path)
        {
            using var db = OpenConnection();

            int images = 0;

            lock (_lock)
            {
                db.BeginTransaction();

                var deletedIds = InsertIds(db, "DeletedIds", "FolderId IN (SELECT Id FROM Folder WHERE PATH LIKE @Path || '%')", new Dictionary<string, object>() { { "@Path", path } });

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
                images = command.ExecuteNonQuery();

                var deleteFolderQuery = "DELETE FROM Folder WHERE PATH = @Path";
                var deleteFolderCommand = db.CreateCommand(deleteFolderQuery);
                deleteFolderCommand.Bind("@Path", path);
                deleteFolderCommand.ExecuteNonQuery();

                db.Commit();


            }

            db.Close();

            return images;
        }

        public int RemoveFolder(SQLiteConnection db, string path)
        {
            var deletedIds = InsertIds(db, "DeletedIds", "FolderId IN (SELECT Id FROM Folder WHERE PATH LIKE @Path || '%')", new Dictionary<string, object>() { { "@Path", path } });

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

            var deleteFolderQuery = "DELETE FROM Folder WHERE PATH = @Path";
            var deleteFolderCommand = db.CreateCommand(deleteFolderQuery);
            deleteFolderCommand.Bind("@Path", path);
            deleteFolderCommand.ExecuteNonQuery();

            return images;
        }

        public IEnumerable<FolderArchived> GetArchivedStatus()
        {
            var db = OpenReadonlyConnection();

//            var query = @"WITH RECURSIVE
//	directoryTree(Id, ParentId, Archived, RootId, Depth) AS
//	(
//		SELECT Id, ParentId, Archived, Id AS RootId, 0 AS Depth FROM Folder 
//		UNION ALL
//		SELECT f.Id, f.ParentId, f.Archived, t.RootId, t.Depth + 1 FROM Folder f JOIN directoryTree t ON f.ParentId = t.Id
//	)
//SELECT 
//	F.RootId FolderId, 
//	--F.Path,
//	F.Archived FolderState,
//	CASE 
//		WHEN Q.ArchivedTotal IS NULL AND P.Total IS NULL THEN 1 
//		WHEN Q.ArchivedTotal = P.Total THEN 1 
//		ELSE 0 END ChildState,
//	F.Archived * 2 + CASE 
//		WHEN Q.ArchivedTotal IS NULL AND P.Total IS NULL THEN 1 
//		WHEN Q.ArchivedTotal = P.Total THEN 1 
//		ELSE 0 END State 
//FROM 
//(SELECT Id AS RootId, Path, Archived FROM Folder) F
//LEFT JOIN 
//(SELECT RootId, Count(*) AS ArchivedTotal FROM directoryTree WHERE Archived = 1 AND Depth > 0  GROUP BY RootId) Q  ON Q.RootId = F.RootId
//LEFT JOIN 
//(SELECT RootId, Count(*) AS Total FROM directoryTree WHERE Depth > 0  GROUP BY RootId) P ON P.RootId = F.RootId
//ORDER BY FolderId
//";
            return db.Query<FolderArchived>("SELECT Id, Archived FROM Folder ORDER BY Id");
        }
        
        public bool FolderHasImages(string path)
        {
            var db = OpenReadonlyConnection();

            var count = db.ExecuteScalar<int>("SELECT Count(1) FROM Image i INNER JOIN Folder f ON i.FolderId = f.Id WHERE f.Path = ?", path);

            return count > 0;
        }
    }

    public class FolderArchived
    {
        public int Id { get; set; }
        public bool Archived { get; set; }
    }
}

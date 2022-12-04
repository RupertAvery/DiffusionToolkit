using SQLite;
using System.Data;
using System.Reflection.PortableExecutable;
using static System.Net.Mime.MediaTypeNames;

namespace Diffusion.Database;

public class DataStore
{
    public string DatabasePath { get; }
    public bool RescanRequired { get; set; }

    public SQLiteConnection OpenConnection()
    {
        return new SQLiteConnection(DatabasePath);
    }

    public DataStore(string databasePath)
    {
        DatabasePath = databasePath;
        Create();
    }

    public void Create()
    {
        var databaseDir = Path.GetDirectoryName(DatabasePath);

        if (!Directory.Exists(databaseDir))
        {
            Directory.CreateDirectory(databaseDir);
        }

        using var db = OpenConnection();

        db.CreateTable<Folder>();
        //db.CreateTable<File>();
        db.CreateTable<Image>();

        db.CreateIndex<Image>(image => image.Path);
        db.CreateIndex<Image>(image => image.ModelHash);
        db.CreateIndex<Image>(image => image.Seed);
        db.CreateIndex<Image>(image => image.Sampler);
        db.CreateIndex<Image>(image => image.Height);
        db.CreateIndex<Image>(image => image.Width);
        db.CreateIndex<Image>(image => image.CFGScale);
        db.CreateIndex<Image>(image => image.Steps);
        db.CreateIndex<Image>(image => image.AestheticScore);
        db.CreateIndex<Image>(image => image.Favorite);
        db.CreateIndex<Image>(image => image.CreatedDate);
        db.CreateIndex<Image>(image => image.ForDeletion);
        db.CreateIndex<Image>(image => image.HyperNetwork);
        db.CreateIndex<Image>(image => image.HyperNetworkStrength);

        db.Close();

    }


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

    public void Close(SQLiteConnection connection)
    {
        connection.Close();
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

        var nonProps = new string[] { nameof(Image.Id) }; /*            { "CustomTags", "Rating", "Favorite", "ForDeletion" } */;
        var properties = typeof(Image).GetProperties().Where(p => !nonProps.Contains(p.Name)).ToList();

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


    public void RebuildIndexes()
    {
        using var db = OpenConnection();

        db.DropIndex<Image>(image => image.Path);
        db.DropIndex<Image>(image => image.ModelHash);
        db.DropIndex<Image>(image => image.Seed);
        db.DropIndex<Image>(image => image.Sampler);
        db.DropIndex<Image>(image => image.Height);
        db.DropIndex<Image>(image => image.Width);
        db.DropIndex<Image>(image => image.CFGScale);
        db.DropIndex<Image>(image => image.Steps);

        db.CreateIndex<Image>(image => image.Path);
        db.CreateIndex<Image>(image => image.ModelHash);
        db.CreateIndex<Image>(image => image.Seed);
        db.CreateIndex<Image>(image => image.Sampler);
        db.CreateIndex<Image>(image => image.Height);
        db.CreateIndex<Image>(image => image.Width);
        db.CreateIndex<Image>(image => image.CFGScale);
        db.CreateIndex<Image>(image => image.Steps);
        db.Close();

    }

    public int GetTotal()
    {
        using var db = OpenConnection();

        var count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Image");

        db.Close();

        return count;
    }


    public int Count(string prompt)
    {
        using var db = OpenConnection();

        if (string.IsNullOrEmpty(prompt))
        {
            var allcount = db.ExecuteScalar<int>($"SELECT COUNT(*) FROM Image");
            return allcount;
        }

        var q = QueryBuilder.Parse(prompt);

        var count = db.ExecuteScalar<int>($"SELECT COUNT(*) FROM Image WHERE {q.Item1}", q.Item2.ToArray());

        db.Close();

        return count;
    }

    public IEnumerable<Image> Search(string prompt, int pageSize, int offset)
    {
        using var db = OpenConnection();

        if (string.IsNullOrEmpty(prompt))
        {
            var allimages = db.Query<Image>($"SELECT * FROM Image ORDER BY CreatedDate DESC LIMIT ? OFFSET ?", pageSize, offset);

            foreach (var image in allimages)
            {
                yield return image;
            }

            db.Close();

            yield break;
        }

        //SELECT foo, bar, baz, quux FROM table
        //WHERE oid NOT IN(SELECT oid FROM table
        //ORDER BY title ASC LIMIT 50 )
        //ORDER BY title ASC LIMIT 10

        var q = QueryBuilder.Parse(prompt);

        var images = db.Query<Image>($"SELECT * FROM Image WHERE {q.Item1} ORDER BY CreatedDate DESC LIMIT ? OFFSET ?", q.Item2.Concat(new object[] { pageSize, offset }).ToArray());

        foreach (var image in images)
        {
            yield return image;
        }

        db.Close();
    }

    public bool FolderExists(string path)
    {
        string sql = @"select count(*) from Folder where Path = @path";

        using var db = OpenConnection();
        var cmd = db.CreateCommand("");
        cmd.CommandText = sql;
        cmd.Bind("@path", path);
        var count = cmd.ExecuteScalar<int>();
        db.Close();
        return count > 0;
    }

    public Folder AddFolder(Folder folder)
    {
        string sql = @"insert into Folder (Path) values (@path); select last_insert_rowid();";

        using var db = OpenConnection();
        var cmd = db.CreateCommand("");
        cmd.CommandText = sql;
        cmd.Bind("@path", folder.Path);
        cmd.ExecuteNonQuery();

        sql = "select last_insert_rowid();";

        cmd = db.CreateCommand(sql);
        folder.Id = cmd.ExecuteScalar<int>();

        db.Close();

        return folder;
    }

    //        public void RemoveFolder(int folderId)
    //        {
    //            string sql = @"delete from Folder where Id = @id;";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }

    //        public void RemoveImages(int folderId)
    //        {
    //            string sql = @"delete from Image where rowid in (select Image.rowid from Image inner join File on Image.FileId = File.Id where FolderId = @id)";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }

    //        public void RemoveFiles(int folderId)
    //        {
    //            string sql = @"delete from File where FolderId = @id";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }

    //        public void RemoveInvalidImages(int folderId)
    //        {
    //            string sql = @"delete from Image where rowid in (select Image.rowid from Image inner join File on Image.FileId = File.Id where FolderId = @id) and CheckedState = -1";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }


    //        public void RemoveUncheckedImages(int folderId)
    //        {
    //            string sql = @"delete from Image (select Image.rowid from Image inner join File on Image.FileId = File.Id where FolderId = @id) and CheckedState in (-1, 0)";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }

    //        public File AddFile(File file)
    //        {
    //            string sql = @"insert into 
    //File (FolderId, Path, ImageCount) 
    //values (@folderId, @path, @imageCount)";

    //            using var db = OpenConnection();

    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@folderId", file.FolderId);
    //            cmd.Bind("@path", file.Path);
    //            cmd.Bind("@imageCount", file.ImageCount);
    //            cmd.ExecuteNonQuery();

    //            sql = "select last_insert_rowid();";

    //            cmd = db.CreateCommand(sql);
    //            file.Id = cmd.ExecuteScalar<int>();

    //            db.Close();

    //            return file;
    //        }

    //        public void AddImage(Image image)
    //        {
    //            using var db = OpenConnection();
    //            db.Insert(image);
    //            db.Close();
    //        }

    //        public void RebuildIndexes()
    //        {
    //            using var db = OpenConnection();
    //            db.DropIndex<File>(file => file.FolderId);

    //            db.DropIndex<Image>(image => image.FileId);
    //            db.DropIndex<Image>(image => image.RecognizedDigits);
    //            db.DropIndex<Image>(image => image.CreatedDate);

    //            db.CreateIndex<File>(file => file.FolderId);
    //            db.CreateIndex<Image>(image => image.FileId);
    //            db.CreateIndex<Image>(image => image.RecognizedDigits);
    //            db.CreateIndex<Image>(image => image.CreatedDate);
    //            db.Close();

    //        }


    //        public IEnumerable<File> GetFiles(int folderId)
    //        {
    //            using var db = OpenConnection();

    //            var files = db.Query<File>("select * from File where FolderId = ?", folderId);

    //            foreach (var file in files)
    //            {
    //                yield return file;
    //            }

    //            db.Close();
    //        }

    //        public IEnumerable<Image> GetFolderImages(int folderId)
    //        {
    //            using var db = OpenConnection();

    //            var images = db.Query<Image>("select Image.* from Image inner join File on Image.FileId = File.Id where File.FolderId = ?", folderId);

    //            foreach (var image in images)
    //            {
    //                yield return image;
    //            }

    //            db.Close();
    //        }

    //        public IEnumerable<Image> GetImages(int fileId)
    //        {
    //            using var db = OpenConnection();

    //            var images = db.Query<Image>("select * from Image where FileId = ?", fileId);

    //            foreach (var image in images)
    //            {
    //                yield return image;
    //            }

    //            db.Close();
    //        }

    //        /// <summary>
    //        /// Sets the checked state of the image (-1=invalid, 0=unchecked, 1=valid)
    //        /// </summary>
    //        /// <param name="imageId"></param>
    //        /// <param name="state"></param>
    //        public void SetCheckedState(int imageId, int state)
    //        {
    //            using var db = OpenConnection();

    //            db.Execute("update Image set CheckedState = ? WHERE Id = ?", state, imageId);

    //            db.Close();
    //        }


    //        /// <summary>
    //        /// Updates the RecognizedDigits of a File and sets IsCorrected to 1
    //        /// </summary>
    //        /// <param name="fileId"></param>
    //        /// <param name="digits"></param>
    //        public void UpdateImage(int fileId, string digits)
    //        {
    //            using var db = OpenConnection();

    //            db.Execute("update Image set RecognizedDigits = ?, IsCorrected = 1 WHERE Id = ?", digits, fileId);

    //            db.Close();
    //        }

    //        public void GetFile(int fileId)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public void UpdateImage(Image image)
    //        {
    //            using var db = OpenConnection();

    //            db.Update(image);

    //            db.Close();
    //        }


    //        public IEnumerable<PhysicalFile> GetFilePaths(int folderId)
    //        {
    //            using var db = OpenConnection();

    //            var files = db.Query<File>("select FolderId, Path from File where FolderId = ?", folderId);

    //            foreach (var file in files)
    //            {
    //                yield return new PhysicalFile()
    //                {
    //                    FolderId = folderId,
    //                    Path = file.Path
    //                };
    //            }


    //            db.Close();
    //        }

    //        public IEnumerable<FolderView> GetFoldersForView()
    //        {
    //            using var db = OpenConnection();

    //            var folders = db.Query<FolderView>("select Id, Path, (select count(*) from Image inner join File on Image.FileId = File.Id where File.FolderId = Folder.Id) as FileCount from Folder");

    //            foreach (var folder in folders)
    //            {
    //                yield return folder;
    //            }

    //            db.Close();
    //        }

    //        public IEnumerable<Folder> GetFolders()
    //        {
    //            using var db = OpenConnection();

    //            var folders = db.Query<Folder>("select Id, Path from Folder");

    //            foreach (var folder in folders)
    //            {
    //                yield return folder;
    //            }

    //            db.Close();
    //        }


    //        public FolderView GetFolder(int folderId)
    //        {
    //            using var db = OpenConnection();
    //            var result = db.FindWithQuery<FolderView>("select Id, Path from Folder where Id = ?", folderId);
    //            db.Close();
    //            return result;
    //        }

    //        public bool PathExists(string path)
    //        {
    //            using var db = OpenConnection();

    //            var count = db.ExecuteScalar<int>("select Count(Path) from Folder where Path = substr(?, 0, length(Path) + 1)", path);

    //            db.Close();

    //            return count > 0;
    //        }

    //        public File? GetFile(int folderId, string relativePath)
    //        {
    //            using var db = OpenConnection();

    //            var file = db.FindWithQuery<File>("select * from File where FolderId = ? and Path = ?", folderId, relativePath);

    //            db.Close();

    //            return file;
    //        }

    //        private class FileFolderPath
    //        {
    //            public string Folder { get; set; }
    //            public string File { get; set; }
    //        }

    //        public string GetFilePath(int fileId)
    //        {
    //            using var db = OpenConnection();

    //            var ff = db.FindWithQuery<FileFolderPath>("select Folder.Path as Folder, File.Path as File from File inner join Folder on File.FolderId = Folder.Id where File.Id = ?", fileId);

    //            db.Close();

    //            // TODO: find out why this causes an Object reference not set

    //            return Path.Join(ff.Folder, ff.File);
    //        }

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

    public void SetDeleted(int id, bool forDeletion)
    {
        var db = OpenConnection();

        var query = "UPDATE Image SET ForDeletion = @ForDeletion WHERE Id = @Id";

        var command = db.CreateCommand(query);

        command.Bind("@ForDeletion", forDeletion);
        command.Bind("@Id", id);

        command.ExecuteNonQuery();
    }

    public void SetFavorite(int id, bool favorite)
    {
        var db = OpenConnection();

        var query = "UPDATE Image SET Favorite = @Favorite WHERE Id = @Id";

        var command = db.CreateCommand(query);

        command.Bind("@Favorite", favorite);
        command.Bind("@Id", id);

        command.ExecuteNonQuery();
    }

    public void SetRating(int id, int? rating)
    {
        var db = OpenConnection();

        var query = "UPDATE Image SET Rating = @Rating WHERE Id = @Id";

        var command = db.CreateCommand(query);

        command.Bind("@Rating", rating);
        command.Bind("@Id", id);

        command.ExecuteNonQuery();
    }

    public void SetCustomTags(int id, string tags)
    {
        var db = OpenConnection();

        var query = "UPDATE Image SET CustomTags = @CustomTags WHERE Id = @Id";

        var command = db.CreateCommand(query);

        command.Bind("@CustomTags", tags);
        command.Bind("@Id", id);

        command.ExecuteNonQuery();
    }
}

public class ImagePath
{
    public int Id { get; set; }
    public string Path { get; set; }
}
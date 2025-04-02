using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Diffusion.Database
{
    public partial class DataStore
    {

        public IEnumerable<AlbumListItem> GetAlbumsView()
        {
            using var db = OpenConnection();

            var lists = db.Query<AlbumListItem>($"SELECT A.Id, A.Name, A.[Order], A.LastUpdated, (SELECT COUNT(1) FROM {nameof(AlbumImage)} AI WHERE A.Id = AI.AlbumId) AS ImageCount FROM {nameof(Album)} A");

            db.Close();

            return lists;
        }

        public IEnumerable<Album> GetAlbums()
        {
            using var db = OpenConnection();

            var lists = db.Query<Album>($"SELECT Id, Name, [Order], LastUpdated FROM {nameof(Album)}");

            db.Close();

            return lists;
        }

        public IEnumerable<Album> GetAlbumsByLastUpdated(int limit)
        {
            using var db = OpenConnection();

            var lists = db.Query<Album>($"SELECT Id, Name, [Order], LastUpdated FROM {nameof(Album)} ORDER BY LastUpdated DESC LIMIT ?", limit);

            db.Close();

            return lists;
        }

        public IEnumerable<Album> GetAlbumsByName()
        {
            using var db = OpenConnection();

            var lists = db.Query<Album>($"SELECT Id, Name, [Order], LastUpdated FROM {nameof(Album)} ORDER BY Name");

            db.Close();

            return lists;
        }

        public void RenameAlbum(int id, string name)
        {
            using var db = OpenConnection();

            var query = $"UPDATE {nameof(Album)} SET Name = @Name WHERE Id = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@Name", name);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();

            db.Close();
        }

        public Album? GetAlbum(int id)
        {
            using var db = OpenConnection();

            var query = $"SELECT * FROM {nameof(Album)} WHERE Id = @Id LIMIT 1";

            var command = db.CreateCommand(query);

            command.Bind("@Id", id);

            var album = command.ExecuteQuery<Album>();

            db.Close();

            if (album.Count < 1)
                return null;
            return album[0];
        }


        //public Album GetAlbumByName(string name)
        //{
        //    using var db = OpenConnection();

        //    var query = $"SELECT * FROM {nameof(Album)} WHERE Name = @Name LIMIT 1";

        //    var command = db.CreateCommand(query);

        //    command.Bind("@Name", name);

        //    var album = command.ExecuteQuery<Album>();

        //    db.Close();

        //    return album[0];
        //}

        public Album CreateAlbum(Album album)
        {
            using var db = OpenConnection();

            var query = $"INSERT INTO {nameof(Album)} (Name, LastUpdated) VALUES (@Name, @LastUpdated)";

            var command = db.CreateCommand(query);

            command.Bind("@Name", album.Name);
            command.Bind("@LastUpdated", DateTime.Now);

            command.ExecuteNonQuery();

            var sql = "select last_insert_rowid();";

            command = db.CreateCommand(sql);

            album.Id = command.ExecuteScalar<int>();

            return album;
        }

        //public void CleanupOrphanedAlbumImages()
        //{
        //    using var db = OpenConnection();

        //    var query = "DELETE FROM AlbumImage WHERE AlbumId IN (SELECT AlbumId FROM AlbumImage WHERE AlbumId NOT IN (SELECT Id FROM Album))";

        //    var command = db.CreateCommand(query);

        //    command.ExecuteNonQuery();


        //    query = "DELETE FROM AlbumImage WHERE ImageId NOT IN(SELECT Id FROM Image)";

        //    command = db.CreateCommand(query);

        //    command.ExecuteNonQuery();

        //}

        public void RemoveAlbum(int id)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var query = $"DELETE FROM {nameof(AlbumImage)} WHERE AlbumId = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@Id", id);

            command.ExecuteNonQuery();

            query = $"DELETE FROM {nameof(Album)} WHERE Id = @Id";

            command = db.CreateCommand(query);

            command.Bind("@Id", id);

            command.ExecuteNonQuery();

            db.Commit();
        }

        public bool AddImagesToAlbum(int albumId, IEnumerable<int> imageId)
        {
            //add a check to make sure that album exists
            if (GetAlbum(albumId) == null)
                return false;

            using var db = OpenConnection();

            db.BeginTransaction();

            var query = $"INSERT OR IGNORE INTO {nameof(AlbumImage)} (AlbumId, ImageId) VALUES (@AlbumId, @ImageId)";

            var command = db.CreateCommand(query);

            foreach (var id in imageId)
            {
                command.Bind("@AlbumId", albumId);
                command.Bind("@ImageId", id);
                command.ExecuteNonQuery();
            }

            query = $"UPDATE {nameof(Album)} SET LastUpdated = @LastUpdated WHERE Id = @Id";

            command = db.CreateCommand(query);

            command.Bind("@LastUpdated", DateTime.Now);
            command.Bind("@Id", albumId);

            command.ExecuteNonQuery();

            db.Commit();

            return true;
        }

        public int RemoveImagesFromAlbum(int albumId, IEnumerable<int> imageIds)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var selectedIds = InsertIds(db, "SelectedIds", imageIds);

            var query = $"DELETE FROM {nameof(AlbumImage)}  WHERE AlbumId = @AlbumId AND ImageId IN {selectedIds}";

            var command = db.CreateCommand(query);
            command.Bind("@AlbumId", albumId);
            var affected = command.ExecuteNonQuery();

            db.Commit();

            return affected;
        }

        public IEnumerable<Image> GetAlbumImages(int albumId, int pageSize, int offset)
        {
            using var db = OpenConnection();

            var images = db.Query<Image>($"SELECT * FROM {nameof(Image)} i INNER JOIN {nameof(AlbumImage)} ai ON i.Id = ai.ImageId WHERE ai.AlbumId = ? ORDER BY CreatedDate DESC LIMIT ? OFFSET ?", albumId, pageSize, offset);

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }

        public void UpdateAlbumsOrder(IEnumerable<Album> albums)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var query = $"UPDATE {nameof(Album)} SET [Order] = @Order WHERE Id = @Id";

            var command = db.CreateCommand(query);

            foreach (var album in albums)
            {
                command.Bind("@Order", album.Order);
                command.Bind("@Id", album.Id);
                command.ExecuteNonQuery();
            }

            db.Commit();
        }
    }
}

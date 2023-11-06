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

        public IEnumerable<Album> GetAlbums()
        {
            using var db = OpenConnection();

            var lists = db.Query<Album>($"SELECT * FROM {nameof(Album)}");

            db.Close();

            return lists;
        }

        public IEnumerable<Album> GetAlbumsByLastUpdated(int limit)
        {
            using var db = OpenConnection();

            var lists = db.Query<Album>($"SELECT * FROM {nameof(Album)} ORDER BY LastUpdated DESC LIMIT ?", limit);

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

        public Album GetAlbum(int id)
        {
            using var db = OpenConnection();

            var query = $"SELECT * FROM {nameof(Album)} WHERE Id = @Id LIMIT 1";

            var command = db.CreateCommand(query);

            command.Bind("@Id", id);

            var album = command.ExecuteQuery<Album>();

            db.Close();

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

        public void RemoveAlbum(int id)
        {
            using var db = OpenConnection();

            var query = $"DELETE FROM {nameof(AlbumImage)} WHERE AlbumId = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@Id", id);

            query = $"DELETE FROM {nameof(Album)} WHERE Id = @Id";

            command = db.CreateCommand(query);

            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }

        public void AddImagesToAlbum(int albumId, IEnumerable<int> imageId)
        {
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
        }

        public void RemoveImagesFromAlbum(int albumId, IEnumerable<int> imageId)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var query = $"DELETE FROM {nameof(AlbumImage)}  WHERE AlbumId = @AlbumId AND ImageId = @ImageId";

            var command = db.CreateCommand(query);

            foreach (var id in imageId)
            {
                command.Bind("@AlbumId", albumId);
                command.Bind("@ImageId", id);
                command.ExecuteNonQuery();
            }

            db.Commit();
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

    }
}

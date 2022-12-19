using System;
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
            var db = OpenConnection();

            var lists = db.Query<Album>($"SELECT * FROM {nameof(Album)}");

            db.Close();

            return lists;
        }

        public Album CreateAlbum(Album imageList)
        {
            var db = OpenConnection();

            var query = $"INSERT INTO {nameof(Album)} (Name) VALUES (@Name)";

            var command = db.CreateCommand(query);

            command.Bind("@Name", imageList.Name);

            command.ExecuteNonQuery();

            var sql = "select last_insert_rowid();";

            command = db.CreateCommand(sql);

            imageList.Id = command.ExecuteScalar<int>();

            return imageList;
        }

        public void RemoveAlbum(int id)
        {
            var db = OpenConnection();

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
            var db = OpenConnection();

            db.BeginTransaction();

            var query = $"INSERT INTO {nameof(AlbumImage)} (AlbumId, ImageId) VALUES (@AlbumId, @ImageId)";

            var command = db.CreateCommand(query);

            foreach (var id in imageId)
            {
                command.Bind("@AlbumId", albumId);
                command.Bind("@ImageId", id);
                command.ExecuteNonQuery();
            }

            db.Commit();
        }

        public void RemoveImagesFromAlbum(int albumId, IEnumerable<int> imageId)
        {
            var db = OpenConnection();

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

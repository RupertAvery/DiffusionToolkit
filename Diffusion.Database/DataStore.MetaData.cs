using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diffusion.Database
{
    public partial class DataStore
    {

        public void SetDeleted(int id, bool forDeletion)
        {
            var db = OpenConnection();

            var query = "UPDATE Image SET ForDeletion = @ForDeletion WHERE Id = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@ForDeletion", forDeletion);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }

        public void SetDeleted(IEnumerable<int> ids, bool forDeletion)
        {
            var db = OpenConnection();

            db.BeginTransaction();

            var query = "UPDATE Image SET ForDeletion = @ForDeletion WHERE Id = @Id";

            var command = db.CreateCommand(query);

            foreach (var id in ids)
            {
                command.Bind("@ForDeletion", forDeletion);
                command.Bind("@Id", id);
                command.ExecuteNonQuery();
            }

            db.Commit();
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


        public void SetFavorite(IEnumerable<int> ids, bool favorite)
        {
            var db = OpenConnection();

            db.BeginTransaction();

            var query = "UPDATE Image SET Favorite = @Favorite WHERE Id = @Id";
            var command = db.CreateCommand(query);

            foreach (var id in ids)
            {
                command.Bind("@Favorite", favorite);
                command.Bind("@Id", id);
                command.ExecuteNonQuery();
            }

            db.Commit();
        }

        public void SetNSFW(int id, bool nsfw)
        {
            var db = OpenConnection();

            var query = "UPDATE Image SET NSFW = @NSFW WHERE Id = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@NSFW", nsfw);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }


        public void SetNSFW(IEnumerable<int> ids, bool nsfw)
        {
            var db = OpenConnection();

            db.BeginTransaction();

            var query = "UPDATE Image SET NSFW = @NSFW WHERE Id = @Id";
            var command = db.CreateCommand(query);

            foreach (var id in ids)
            {
                command.Bind("@NSFW", nsfw);
                command.Bind("@Id", id);
                command.ExecuteNonQuery();
            }

            db.Commit();
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

        public void SetRating(IEnumerable<int> ids, int? rating)
        {
            var db = OpenConnection();

            db.BeginTransaction();

            var query = "UPDATE Image SET Rating = @Rating WHERE Id = @Id";

            var command = db.CreateCommand(query);

            foreach (var id in ids)
            {
                command.Bind("@Rating", rating);
                command.Bind("@Id", id);
                command.ExecuteNonQuery();
            }

            db.Commit();
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
}

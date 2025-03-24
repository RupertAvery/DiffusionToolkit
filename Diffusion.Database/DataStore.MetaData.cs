using System.Text;
using Diffusion.IO;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using ComfyUINode = Diffusion.IO.Node;

namespace Diffusion.Database
{
    public partial class DataStore
    {

        public void SetDeleted(int id, bool forDeletion)
        {
            using var db = OpenConnection();

            var query = "UPDATE Image SET ForDeletion = @ForDeletion WHERE Id = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@ForDeletion", forDeletion);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }

        public void SetDeleted(IEnumerable<int> ids, bool forDeletion)
        {
            using var db = OpenConnection();

            InsertIds(db, "MarkedIds", ids);

            db.BeginTransaction();

            var query = "UPDATE Image SET ForDeletion = @ForDeletion WHERE Id IN (SELECT Id FROM MarkedIds)";
            var command = db.CreateCommand(query);
            command.Bind("@ForDeletion", forDeletion);
            command.ExecuteNonQuery();

            db.Commit();
        }

        public void SetFavorite(int id, bool favorite)
        {
            using var db = OpenConnection();

            var query = "UPDATE Image SET Favorite = @Favorite WHERE Id = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@Favorite", favorite);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }


        public void SetFavorite(IEnumerable<int> ids, bool favorite)
        {
            using var db = OpenConnection();

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
            using var db = OpenConnection();

            var query = "UPDATE Image SET NSFW = @NSFW WHERE Id = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@NSFW", nsfw);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }


        public void SetNSFW(IEnumerable<int> ids, bool nsfw, bool preserve = false)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var update = preserve ? "NSFW = NSFW OR @NSFW" : "NSFW = @NSFW";

            InsertIds(db, "MarkedIds", ids);

            var query = $"UPDATE Image SET {update} WHERE Id IN (SELECT Id FROM MarkedIds)";
            var command = db.CreateCommand(query);
            command.Bind("@NSFW", nsfw);
            command.ExecuteNonQuery();

            db.Commit();
        }


        public void SetRating(int id, int? rating)
        {
            using var db = OpenConnection();

            var query = "UPDATE Image SET Rating = @Rating WHERE Id = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@Rating", rating);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }

        public void SetRating(IEnumerable<int> ids, int? rating)
        {
            using var db = OpenConnection();

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

        public IEnumerable<ImagePath> GetUnavailable(bool unavailable)
        {
            using var db = OpenConnection();

            var query = "SELECT Id, Path FROM Image WHERE Unavailable = @Unavailable";

            var images = db.Query<ImagePath>(query, unavailable);

            foreach (var image in images)
            {
                yield return image;
            }

            db.Close();
        }


        public void SetUnavailable(IEnumerable<int> ids, bool unavailable)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var query = $"UPDATE Image SET Unavailable = @Unavailable WHERE Id = @Id";
            var command = db.CreateCommand(query);

            foreach (var id in ids)
            {
                command.Bind("@Unavailable", unavailable);
                command.Bind("@Id", id);
                command.ExecuteNonQuery();
            }

            db.Commit();
        }

        public void SetCustomTags(int id, string tags)
        {
            using var db = OpenConnection();

            var query = "UPDATE Image SET CustomTags = @CustomTags WHERE Id = @Id";

            var command = db.CreateCommand(query);

            command.Bind("@CustomTags", tags);
            command.Bind("@Id", id);

            command.ExecuteNonQuery();
        }

        public void AddNodes(IEnumerable<ComfyUINode> nodes, CancellationToken cancellationToken)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var nodeQuery = new StringBuilder($"INSERT INTO {nameof(Node)} (ImageId, NodeId, Name) VALUES ");

            foreach (var node in nodes)
            {
                var image = (Image)node.ImageRef;
                nodeQuery.Append($"({image.Id}, '{node.Id}', '{node.Name}'),");
            }

            nodeQuery.Remove(nodeQuery.Length - 1, 1);

            nodeQuery.Append(" RETURNING Id;");

            var nodeCommand = db.CreateCommand(nodeQuery.ToString());

            var nodeIds = nodeCommand.ExecuteQuery<ReturnId>();

            foreach (var node in nodes.Zip(nodeIds))
            {
                node.First.RefId = node.Second.Id;
            }

            var propertyQuery = new StringBuilder($"INSERT INTO {nameof(NodeProperty)} (NodeId, Name, Value) VALUES ");


            foreach (var property in nodes.SelectMany(n => n.Inputs.Select(p => new NodeProperty()
            {
                NodeId = n.RefId,
                Name = p.Name,
                Value = p.Value.ToString()
            })))
            {
                propertyQuery.Append($"({property.NodeId}, '{property.Name}', '{SqlEscape(property.Value)}'),");
            }

            propertyQuery.Remove(propertyQuery.Length - 1, 1);

            var propertyCommand = db.CreateCommand(propertyQuery.ToString());
            propertyCommand.ExecuteNonQuery();

            db.Commit();
        }


        string SqlEscape(string value)
        {
            return value.Replace("'", "''");
        }

        long SqlDateTime(DateTime value)
        {
            return value.Ticks;
        }

        int SqlBoolean(bool value)
        {
            return value ? 1 : 0;
        }

        public class ReturnId
        {
            public int Id { get; set; }
        }

        public void UpdateNodes(IEnumerable<ComfyUINode> nodes, CancellationToken cancellationToken)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            var imageIds = nodes.Select(d => (Image)d.ImageRef).Select(d => d.Id).Distinct();

            InsertIds(db, "DeletedIds", imageIds);

            var delNodePropQuery = $"DELETE FROM {nameof(NodeProperty)} WHERE NodeId IN (SELECT Id FROM {nameof(Node)} WHERE ImageId IN (SELECT Id FROM DeletedIds))";
            var delNodePropCommand = db.CreateCommand(delNodePropQuery);
            delNodePropCommand.ExecuteNonQuery();

            var delNodeQuery = $"DELETE FROM {nameof(Node)} WHERE ImageId IN (SELECT Id FROM DeletedIds)";
            var delNodeCommand = db.CreateCommand(delNodeQuery);
            delNodeCommand.ExecuteNonQuery();

            var nodeQuery = new StringBuilder($"INSERT INTO {nameof(Node)} (ImageId, NodeId, Name) VALUES ");

            var values = new List<string>();

            foreach (var node in nodes)
            {
                var image = (Image)node.ImageRef;
                values.Add($"({image.Id}, '{node.Id}', '{node.Name}')");
            }

            nodeQuery.Append(string.Join(",", values));

            nodeQuery.Append(" RETURNING Id;");

            var nodeCommand = db.CreateCommand(nodeQuery.ToString());

            var nodeIds = nodeCommand.ExecuteQuery<ReturnId>();

            foreach (var node in nodes.Zip(nodeIds))
            {
                node.First.RefId = node.Second.Id;
            }

            var propertyQuery = new StringBuilder($"INSERT INTO {nameof(NodeProperty)} (NodeId, Name, Value) VALUES ");

            values.Clear();

            foreach (var property in nodes.SelectMany(n => n.Inputs.Select(p => new NodeProperty()
            {
                NodeId = n.RefId,
                Name = p.Name,
                Value = p.Value.ToString()
            })))
            {
                values.Add($"({property.NodeId}, '{property.Name}', '{SqlEscape(property.Value)}')");
            }

            propertyQuery.Append(string.Join(",", values));

            var propertyCommand = db.CreateCommand(propertyQuery.ToString());
            propertyCommand.ExecuteNonQuery();

            db.Commit();
        }
    }
}

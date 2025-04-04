using System.Text;
using SQLite;
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

            InsertIds(db, "UnavailableIds", ids);

            var query = "UPDATE Image SET Unavailable = @Unavailable WHERE Id IN (SELECT Id FROM UnavailableIds)";
            var command = db.CreateCommand(query);
            command.Bind("@Unavailable", unavailable);

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

            AddNodesInternal(db, nodes);

            db.Commit();
        }

        private void AddNodesInternal(SQLiteConnection db, IEnumerable<ComfyUINode> nodes)
        {
            var nodeQuery = new StringBuilder($"INSERT INTO {nameof(Node)} (ImageId, NodeId, Name) VALUES ");

            var nodeValues = new List<object>();
            var nodeHolders = new List<string>();

            foreach (var node in nodes)
            {
                var image = (Image)node.ImageRef;
                nodeValues.Add(image.Id);
                nodeValues.Add(node.Id);
                nodeValues.Add(node.Name);
                nodeHolders.Add("(?,?,?)");
            }

            nodeQuery.Append(string.Join(",", nodeHolders));

            nodeQuery.Append(" RETURNING Id;");

            var nodeCommand = db.CreateCommand(nodeQuery.ToString(), nodeValues.ToArray());

            var nodeIds = nodeCommand.ExecuteQuery<ReturnId>();

            foreach (var node in nodes.Zip(nodeIds))
            {
                node.First.RefId = node.Second.Id;
            }

            var nodeProperties = nodes.SelectMany(n => n.Inputs.Select(p => new NodeProperty()
            {
                NodeId = n.RefId,
                Name = p.Name,
                Value = p.Value.ToString()
            }));

            // Break up large workflows into smaller chunks
            foreach (var chunk in nodeProperties.Chunk(100))
            {
                var chunkSet = chunk.ToList();

                var propertyQuery = new StringBuilder($"INSERT INTO {nameof(NodeProperty)} (NodeId, Name, Value) VALUES ");

                var propertyValues = new List<object>();
                var propertyHolders = new List<string>();

                foreach (var property in chunkSet)
                {
                    propertyValues.Add(property.NodeId);
                    propertyValues.Add(property.Name);
                    propertyValues.Add(property.Value);
                    propertyHolders.Add("(?, ?, ?)");
                }

                propertyQuery.Append(string.Join(",", propertyHolders));

                var propertyCommand = db.CreateCommand(propertyQuery.ToString(), propertyValues.ToArray());
                propertyCommand.ExecuteNonQuery();
            }
        }

        public class ReturnId
        {
            public int Id { get; set; }
        }

        public void UpdateNodes(IReadOnlyCollection<ComfyUINode> nodes, CancellationToken cancellationToken)
        {
            using var db = OpenConnection();

            db.BeginTransaction();

            DeleteNodesInternal(db, nodes);

            AddNodesInternal(db, nodes);

            db.Commit();
        }

        private void DeleteNodesInternal(SQLiteConnection db, IReadOnlyCollection<ComfyUINode> nodes)
        {
            var imageIds = nodes.Select(d => (Image)d.ImageRef).Select(d => d.Id).Distinct();

            InsertIds(db, "DeletedIds", imageIds);

            var delNodePropQuery = $"DELETE FROM {nameof(NodeProperty)} WHERE NodeId IN (SELECT Id FROM {nameof(Node)} WHERE ImageId IN (SELECT Id FROM DeletedIds))";
            var delNodePropCommand = db.CreateCommand(delNodePropQuery);
            delNodePropCommand.ExecuteNonQuery();

            var delNodeQuery = $"DELETE FROM {nameof(Node)} WHERE ImageId IN (SELECT Id FROM DeletedIds)";
            var delNodeCommand = db.CreateCommand(delNodeQuery);
            delNodeCommand.ExecuteNonQuery();
        }
    }
}

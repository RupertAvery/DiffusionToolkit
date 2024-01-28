using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Diffusion.Database
{
    public class Session
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DataStore DataStore { get; set; }
    }

    public class SessionManager
    {
        private Dictionary<string, Session> _sessions = new Dictionary<string, Session>();

        public SessionManager()
        {

        }

        public void LoadSessions()
        {

        }

        public void SelectSession(string name)
        {
        }

        public Session CurrentSession { get; private set; }
    }

    public partial class DataStore
    {
        public void SetBitsets(IEnumerable<Bitset> bitsets, Action<int>? progress = null)
        {
            var db = OpenConnection();

            var query = "INSERT INTO Bitset (Id, Data) VALUES (@id, @data) ON CONFLICT (Id) DO UPDATE SET Data = @data";

            int i = 0;

            foreach (var chunk in bitsets.Chunk(500))
            {
                db.BeginTransaction();

                var command = db.CreateCommand(query);

                foreach (var bitset in chunk)
                {
                    command.Bind("@id", bitset.Id);
                    command.Bind("@data", bitset.Data);
                    command.ExecuteNonQuery();
                }

                i += chunk.Length;

                progress?.Invoke(i);

                db.Commit();
            }


            db.Close();
        }

        public List<Bitset> GetBitsets()
        {
            var db = OpenConnection();

            var query = "SELECT Id, Data FROM Bitset";

            var command = db.CreateCommand(query);

            var value = command.ExecuteQuery<Bitset>();

            db.Close();

            return value;
        }

        public void SetBitset(int id, byte[] data)
        {
            var db = OpenConnection();

            var query = "INSERT INTO Bitset (Id, Data) VALUES (@id, @data) ON CONFLICT (Id) DO UPDATE SET Data = @data";

            var command = db.CreateCommand(query);
            command.Bind("@id", id);
            command.Bind("@data", data);
            command.ExecuteNonQuery();

            db.Close();
        }

        public byte[] GetBitset(int id)
        {
            var db = OpenConnection();

            var query = "SELECT Data FROM Bitset WHERE ID = @id";

            var command = db.CreateCommand(query);
            command.Bind("@id", id);

            var value = command.ExecuteScalar<byte[]>();

            db.Close();

            return value;
        }
    }
}
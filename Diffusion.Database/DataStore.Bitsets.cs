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
    public partial class DataStore
    {
        public void SetBitsetBatched(IEnumerable<Bitset> bitsets)
        {
            var db = OpenConnection();

            var query = "INSERT INTO Bitset (Id, Data) VALUES (@id, @data) ON CONFLICT (Id) DO UPDATE SET Data = @data";

            int  i= 0;

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

                Console.WriteLine($"{i}");

                db.Commit();
            }


            db.Close();
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
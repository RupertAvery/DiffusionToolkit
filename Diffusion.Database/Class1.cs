using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Diffusion.Database
{
    public partial class DataStore
    {
        public void SetBitset(int id, byte[] data)
        {
            var db = OpenConnection();

            var query = "INSERT INTO Bitset (Id, Data) VALUES (@id, @data)";
            
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

        //static byte[] GetBytes(SQLiteDataReader reader)
        //{
        //    const int CHUNK_SIZE = 2 * 1024;
        //    byte[] buffer = new byte[CHUNK_SIZE];
        //    long bytesRead;
        //    long fieldOffset = 0;
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        while ((bytesRead = reader.GetBytes(0, fieldOffset, buffer, 0, buffer.Length)) > 0)
        //        {
        //            stream.Write(buffer, 0, (int)bytesRead);
        //            fieldOffset += bytesRead;
        //        }

        //        return stream.ToArray();
        //    }
        //}

    }
}
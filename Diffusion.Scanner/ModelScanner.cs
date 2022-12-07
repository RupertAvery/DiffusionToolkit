using System.IO.Compression;
using System.Security.Cryptography;

namespace Diffusion.IO;

public class ModelScanner
{
    public static IEnumerable<Model> Scan(string path)
    {
        var files = Directory.EnumerateFiles(path, "*.ckpt", SearchOption.AllDirectories);

        foreach (var file in files)
        {

            var buffer = new byte[0x10000];
            using (var f = File.Open(file, FileMode.Open, FileAccess.Read))
            {
                f.Seek(0x100000, SeekOrigin.Begin);
                f.Read(buffer, 0, 0x10000);
            }

            uint crc32Sum = 0;

            using (var zip = ZipFile.Open(file, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    if (entry.FullName != "archive/diffusion.json")
                    {
                        crc32Sum = crc32Sum + entry.Crc32 & 0xFFFFFFFF;
                    }
                }
            }

            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(buffer);

            yield return new Model() { 
                Path = file, 
                Filename = Path.GetFileNameWithoutExtension(file),
                Hash = Convert.ToHexString(hash).ToLower().Substring(0, 8),
                Hashv2 = $"{crc32Sum:x8}"
            };
        }
    }
}
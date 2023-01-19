using System.Security.Cryptography;

namespace Diffusion.IO;

public static class HashFunctions
{
    public static string CalculateHash(string file)
    {
        var buffer = new byte[0x10000];
        using (var f = File.Open(file, FileMode.Open, FileAccess.Read))
        {
            f.Seek(0x100000, SeekOrigin.Begin);
            f.Read(buffer, 0, 0x10000);
        }

        using var hashstring = HashAlgorithm.Create("SHA256");
        byte[] hash = hashstring.ComputeHash(buffer);

        return Convert.ToHexString(hash).ToLower().Substring(0, 8);
    }

    public static string CalculateSHA256(string file)
    {
        byte[] sha256;

        using var sha = HashAlgorithm.Create("SHA256");

        using (var f = File.Open(file, FileMode.Open, FileAccess.Read))
        {
            var buffer = new byte[1024 * 1024];
            sha.Initialize();

            int br = f.Read(buffer, 0, buffer.Length);
            while (br > 0)
            {
                sha.TransformBlock(buffer, 0, br, null, 0);
                br = f.Read(buffer, 0, buffer.Length);
            }
            sha.TransformFinalBlock(buffer, 0, 0);
            sha256 = sha.Hash;
        }

        return Convert.ToHexString(sha256).ToLower();
    }
}
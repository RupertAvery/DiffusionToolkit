using System;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace Diffusion.Common;

public static class ByteExtensions
{
    public static string ToHexString(this byte[] input)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in input)
        {
            sb.AppendFormat("{0:x2}", b);
        }
        return sb.ToString();
    }
}


public static class StreamExtensions
{
    public static byte[] CopyAndHash(this Stream inputStream, Stream outputStream)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Feed the buffer to SHA256
                sha256.TransformBlock(buffer, 0, bytesRead, null, 0);

                // Write to the output stream
                outputStream.Write(buffer, 0, bytesRead);
            }

            // Finalize the hash
            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

            return sha256.Hash;
        }
    }
}

public class Hashing
{
    public static string CalculateHash(string file)
    {
        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return CalculateHash(stream);
        }
    }

    public static string CalculateHash(Stream stream)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }

  
}
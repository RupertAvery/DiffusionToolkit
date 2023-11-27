using System;
using System.IO.Compression;
using System.Security.Cryptography;
using Diffusion.Common;

namespace Diffusion.IO;

public class ModelScanner
{
    public static IEnumerable<Model> Scan(string path)
    {

        var files = Directory.EnumerateFiles(path, "*.ckpt", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(path, "*.safetensors", SearchOption.AllDirectories)); ;

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(path, file);

            if (File.Exists(file))
            {
                string hash = null;
                try
                {
                    hash = HashFunctions.CalculateHash(file);
                }
                catch (Exception e)
                {
                }

                yield return new Model()
                {
                    Path = relativePath,
                    Filename = Path.GetFileNameWithoutExtension(file),
                    Hash = hash,
                    IsLocal = true
                };
            }
        }
    }
}
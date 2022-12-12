using Dir = System.IO.Directory;
using System.IO;
using static System.Net.WebRequestMethods;

namespace Diffusion.IO
{
    public class Scanner
    {
        public static IEnumerable<string> GetFiles(string path, string extensions, HashSet<string>? ignoreFiles)
        {
            var files = Enumerable.Empty<string>();

            foreach (var extension in extensions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                files = files.Concat(Dir.EnumerateFiles(path, $"*{extension}",
                    SearchOption.AllDirectories));
            }

            if (ignoreFiles != null)
            {
                files = files.Where(f => !ignoreFiles.Contains(f));
            }

            return files;
        }

        public static IEnumerable<FileParameters> Scan(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                yield return Metadata.ReadFromFile(file);
            }
        }


    }
}

using Dir = System.IO.Directory;
using System.IO;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace Diffusion.IO
{
    public class Scanner
    {
        public static IEnumerable<string> GetFiles(string path, string extensions, HashSet<string>? ignoreFiles)
        {
            var files = Enumerable.Empty<string>();

            foreach (var extension in extensions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Directory.Exists(path))
                {
                    files = files.Concat(Dir.EnumerateFiles(path, $"*{extension}",
                        SearchOption.AllDirectories));
                }
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
                FileParameters? fp = null;

                try
                {
                    fp = Metadata.ReadFromFile(file);
                }
                catch (Exception e)
                {
                    Logger.Log($"An error occurred while reading {file}: {e.Message}\r\n\r\n{e.StackTrace}");
                }

                if (fp != null)
                {
                    yield return fp;
                }
            }
        }


    }
}

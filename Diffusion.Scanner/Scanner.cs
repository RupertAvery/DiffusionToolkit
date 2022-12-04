using Dir = System.IO.Directory;
using System.IO;
using static System.Net.WebRequestMethods;

namespace Diffusion.IO
{
    public class Scanner
    {
        private readonly string _extensions;


        public Scanner(string extensions)
        {
            _extensions = extensions;
        }

        private IEnumerable<string> GetFiles(string path)
        {
            var files = Enumerable.Empty<string>();
            foreach (var extension in _extensions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                files = files.Concat(Dir.EnumerateFiles(path, $"*{extension}",
                    SearchOption.AllDirectories));
            }

            return files;
        }

        public int Count(string path)
        {
            return GetFiles(path).Count();
        }

        public IEnumerable<FileParameters> Scan(string path, HashSet<string> ignoreFiles)
        {
            var files = GetFiles(path);

            if (ignoreFiles != null)
            {
                files = files.Where(f => !ignoreFiles.Contains(f));
            }

            foreach (var file in files)
            {
                yield return Metadata.ReadFromFile(file);
            }
        }


    }
}

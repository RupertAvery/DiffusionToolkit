using Diffusion.Common;

namespace Diffusion.IO
{
    public class MetadataScanner
    {
        public static IEnumerable<string> GetFiles(string path, string extensions, HashSet<string>? ignoreFiles, bool recursive, IEnumerable<string>? excludePaths)
        {
            var files = Enumerable.Empty<string>();


            if (Directory.Exists(path))
            {

                foreach (var extension in extensions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    try
                    {
                        var dirFiles = Directory.EnumerateFiles(path, $"*{extension}", new EnumerationOptions()
                        {
                            RecurseSubdirectories = recursive,
                            IgnoreInaccessible = true
                        });

                        files = files.Concat(dirFiles);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"MetadataScanner.GetFiles: {ex}");
                    }

                }

                if (ignoreFiles != null)
                {
                    files = files.Where(f => !ignoreFiles.Contains(f));
                }

                if (recursive && excludePaths != null)
                {
                    files = files.Where(f => !excludePaths.Any(p => f.StartsWith(p)));
                }

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

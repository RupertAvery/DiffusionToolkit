using Diffusion.Common;
using System.Threading;

namespace Diffusion.IO
{
    public class MetadataScanner
    {
        public static IEnumerable<string> GetFiles(string path, string extensions, bool recursive, HashSet<string> excludePaths, CancellationToken cancellationToken)
        {
            var files = Enumerable.Empty<string>();

            if (Directory.Exists(path))
            {

                foreach (var extension in extensions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        var dirFiles = EnumerateFilesRecursively(path, $"*{extension}", excludePaths, cancellationToken);

                        //var dirFiles = Directory.EnumerateFiles(path, $"*{extension}", new EnumerationOptions()
                        //{
                        //    RecurseSubdirectories = recursive,
                        //    IgnoreInaccessible = true
                        //});

                        files = files.Concat(dirFiles);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"MetadataScanner.GetFiles: {ex}");
                    }

                }
            }

            return files;
        }


        public static IEnumerable<string> EnumerateFilesRecursively(string path, string extension, HashSet<string> excludePaths, CancellationToken cancellationToken)
        {
            if (!excludePaths.Contains(path))
            {
                var dirFiles = Directory.EnumerateFiles(path, $"*{extension}", new EnumerationOptions()
                {
                    IgnoreInaccessible = true
                });

                foreach (var file in dirFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    yield return file;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }
            }

            IEnumerable<string> dirs = Enumerable.Empty<string>();

            try
            {
                dirs = Directory.GetDirectories(path);
            }
            catch (System.UnauthorizedAccessException e)
            {
                yield break;
            }
            catch (System.Security.SecurityException e)
            {
                yield break;
            }

            foreach (var dir in dirs)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var childDirFiles = EnumerateFilesRecursively(dir, extension, excludePaths, cancellationToken);

                foreach (var file in childDirFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    yield return file;
                }
            }
        }

        public static IEnumerable<string> GetFiles(string path, string extensions, HashSet<string>? ignoreFiles, bool recursive, HashSet<string> excludePaths, CancellationToken cancellationToken)
        {
            var files = Enumerable.Empty<string>();

            if (Directory.Exists(path))
            {

                foreach (var extension in extensions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        var dirFiles = EnumerateFilesRecursively(path, $"*{extension}", excludePaths, cancellationToken);

                        //var dirFiles = Directory.EnumerateFiles(path, $"*{extension}", new EnumerationOptions()
                        //{
                        //    RecurseSubdirectories = recursive,
                        //    IgnoreInaccessible = true
                        //});

                        files = files.Concat(dirFiles);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"MetadataScanner.GetFiles: {ex}");
                    }

                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return files;
                }

                if (ignoreFiles != null)
                {
                    files = files.Where(f => !ignoreFiles.Contains(f));
                }

                //if (recursive && excludePaths != null)
                //{
                //    files = files.Where(f => !excludePaths.Any(p => f.StartsWith(p)));
                //}

            }


            return files;
        }



        public static IEnumerable<FileParameters> Scan(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                FileParameters? fp = null;

                fp = Metadata.ReadFromFile(file);

                if (fp != null)
                {
                    yield return fp;
                }
            }
        }


    }
}

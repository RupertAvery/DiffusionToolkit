using MetadataExtractor;
using MetadataExtractor.Formats.Png;
using Directory = MetadataExtractor.Directory;
using Dir = System.IO.Directory;
using System.IO;

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
                var fileParameters = new FileParameters();

                fileParameters.Path = file;

                try
                {
                    string? parameters = null;

                    var ext = Path.GetExtension(file).ToLower();
                    bool isNotPng = true;

                    if (ext == ".png")
                    {
                        IEnumerable<Directory> directories = PngMetadataReader.ReadMetadata(file);

                        parameters = directories.FirstOrDefault(d => d.Name == "PNG-tEXt")?.Tags
                            .FirstOrDefault(t => t.Name == "Textual Data")?.Description;

                        isNotPng = false;
                    }
                    else if (ext != ".png")
                    {
                        var parameterFile = file.Replace(ext, ".txt", StringComparison.InvariantCultureIgnoreCase);

                        if (File.Exists(parameterFile))
                        {
                            parameters = File.ReadAllText(parameterFile);
                        }
                    }

                    if (parameters != null)
                    {
                        var parts = parameters.Split(new[] { '\n' });

                        const string parametersKey = "parameters:";
                        const string negativePromptKey = "Negative prompt:";
                        const string stepsKey = "Steps:";


                        fileParameters.Parameters = parameters;

                        var index = 0;

                        foreach (var part in parts)
                        {
                            if (isNotPng && index == 0)
                            {
                                fileParameters.Prompt = part;
                            }
                            else if (part.StartsWith(parametersKey, StringComparison.InvariantCultureIgnoreCase))
                            {
                                fileParameters.Prompt = part.Substring(parametersKey.Length + 1);
                            }
                            else if (part.StartsWith(negativePromptKey, StringComparison.InvariantCultureIgnoreCase))
                            {
                                fileParameters.NegativePrompt = part.Substring(negativePromptKey.Length);
                            }
                            else if (part.StartsWith(stepsKey, StringComparison.InvariantCultureIgnoreCase))
                            {
                                fileParameters.OtherParameters = part;

                                var subParts = part.Split(new[] { ',' });
                                foreach (var keyValue in subParts)
                                {
                                    var kvp = keyValue.Split(new[] { ':' });
                                    switch (kvp[0].Trim())
                                    {
                                        case "Steps":
                                            fileParameters.Steps = int.Parse(kvp[1].Trim());
                                            break;
                                        case "Sampler":
                                            fileParameters.Sampler = kvp[1].Trim();
                                            break;
                                        case "CFG scale":
                                            fileParameters.CFGScale = decimal.Parse(kvp[1].Trim());
                                            break;
                                        case "Seed":
                                            fileParameters.Seed = long.Parse(kvp[1].Trim());
                                            break;
                                        case "Size":
                                            var size = kvp[1].Split(new[] { 'x' });
                                            fileParameters.Width = int.Parse(size[0].Trim());
                                            fileParameters.Height = int.Parse(size[1].Trim());
                                            break;
                                        case "Model hash":
                                            fileParameters.ModelHash = kvp[1].Trim();
                                            break;
                                        case "Batch size":
                                            fileParameters.BatchSize = int.Parse(kvp[1].Trim());
                                            break;
                                        case "Batch pos":
                                            fileParameters.BatchPos = int.Parse(kvp[1].Trim());
                                            break;
                                    }
                                }
                            }

                            index++;
                        }
                    }
                }
                catch (Exception e)
                {

                }


                yield return fileParameters;
            }
        }
    }
}
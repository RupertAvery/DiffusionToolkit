using MetadataExtractor;
using MetadataExtractor.Formats.Png;
using Directory = MetadataExtractor.Directory;
using Dir = System.IO.Directory;
using System.IO;

namespace Diffusion.IO
{
    public class Scanner
    {
        public int Count(string path)
        {
            return Dir.GetFiles(path, "*.png", SearchOption.AllDirectories).Length;
        }

        public IEnumerable<FileParameters> Scan(string path)
        {
            var files = Dir.EnumerateFiles(path, "*.png", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileParameters = new FileParameters();

                fileParameters.Path = file;

                try
                {
                    IEnumerable<Directory> directories = PngMetadataReader.ReadMetadata(file);

                    var parameters = directories.FirstOrDefault(d => d.Name == "PNG-tEXt")?.Tags
                        .FirstOrDefault(t => t.Name == "Textual Data")?.Description;

                    if (parameters != null)
                    {
                        var parts = parameters.Split(new[] { '\n' });

                        const string parametersKey = "parameters:";
                        const string negativePromptKey = "Negative prompt:";
                        const string stepsKey = "Steps:";


                        fileParameters.Parameters = parameters;

                        foreach (var part in parts)
                        {
                            if (part.StartsWith(parametersKey, StringComparison.InvariantCultureIgnoreCase))
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
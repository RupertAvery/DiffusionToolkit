using System.Text.Json;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Png;
using Directory = MetadataExtractor.Directory;

namespace Diffusion.IO;

public class Metadata
{
    public static FileParameters ReadFromFile(string file)
    {
        FileParameters fileParameters = null;

        try
        {
            var ext = Path.GetExtension(file).ToLower();

            if (ext == ".png")
            {
                IEnumerable<Directory> directories = PngMetadataReader.ReadMetadata(file);

                var isNovelAI = directories.Any(d => d.Name == "PNG-tEXt" && d.Tags.Any(t => t.Name == "Textual Data" && t.Description == "Software: NovelAI"));

                if (isNovelAI)
                {
                    fileParameters = ReadNovelAIParameters(file, directories);
                }
                else
                {
                    fileParameters = ReadAutomatic1111Parameters(file, directories);
                }
            }
            else
            {
                var parameterFile = file.Replace(ext, ".txt", StringComparison.InvariantCultureIgnoreCase);

                if (File.Exists(parameterFile))
                {
                    var parameters = File.ReadAllText(parameterFile);
                    fileParameters = ReadA111Parameters(parameters);
                }
            }


        }
        catch (Exception e)
        {

        }


        if (fileParameters != null)
        {
            fileParameters.Path = file;
        }

        return fileParameters;
    }


    private static FileParameters ReadNovelAIParameters(string file, IEnumerable<Directory> directories)
    {
        var fileParameters = new FileParameters();

        fileParameters.Path = file;

        Tag tag;

        if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith("Description:"), out tag))
        {
            fileParameters.Prompt = tag.Description.Substring("Description: ".Length);
        }

        if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith("Source:"), out tag))
        {
            var hashRegex = new Regex("[0-9A-F]{8}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = hashRegex.Match(tag.Description.Substring("Source: ".Length));
            if (match.Success)
            {
                fileParameters.ModelHash = match.Groups[0].Value;
            }
        }

        var propList = new List<string>();

        if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith("Comment:"), out tag))
        {
            var json = JsonDocument.Parse(tag.Description.Substring("Comment: ".Length));

            fileParameters.Steps = json.RootElement.GetProperty("steps").GetInt32();
            fileParameters.Sampler = json.RootElement.GetProperty("sampler").GetString();
            fileParameters.Seed = json.RootElement.GetProperty("seed").GetInt64();
            fileParameters.CFGScale = json.RootElement.GetProperty("scale").GetDecimal();

            var properties = json.RootElement.EnumerateObject();

            foreach (var property in properties)
            {
                if (property.Name != "uc")
                {
                    propList.Add($"{property.Name}: {property.Value.ToString()}");
                }
            }

 
            fileParameters.NegativePrompt = json.RootElement.GetProperty("uc").GetString();
        }

        propList.Add($"model hash: {fileParameters.ModelHash}");

        fileParameters.OtherParameters = string.Join(", ", propList);

        var pngIHDR = directories.FirstOrDefault(d => d.Name == "PNG-IHDR");

        if (pngIHDR != null)
        {
            foreach (var ptag in pngIHDR.Tags)
            {
                switch (ptag.Name)
                {
                    case "Image Width":
                        fileParameters.Width = int.Parse(ptag.Description);
                        break;
                    case "Image Height":
                        fileParameters.Height = int.Parse(ptag.Description);
                        break;
                }
            }
        }

        return fileParameters;
    }

    private static FileParameters ReadA111Parameters(string data)
    {
        var fileParameters = new FileParameters();

        var parts = data.Split(new[] { '\n' });

        const string parametersKey = "parameters:";
        const string negativePromptKey = "Negative prompt:";
        const string stepsKey = "Steps:";

        fileParameters.Parameters = data;

        var state = 0;

        fileParameters.Prompt = "";
        fileParameters.NegativePrompt = "";

        foreach (var part in parts)
        {
            var isNegativePrompt = part.StartsWith(negativePromptKey, StringComparison.InvariantCultureIgnoreCase);
            var isPromptStart = part.StartsWith(parametersKey, StringComparison.InvariantCultureIgnoreCase);
            var isOther = part.StartsWith(stepsKey, StringComparison.InvariantCultureIgnoreCase);

            if (isPromptStart)
            {
                state = 0;
            }
            else if (isNegativePrompt)
            {
                state = 1;
            }
            else if (isOther)
            {
                state = 2;
            }

            switch (state)
            {
                case 0:
                    if (isPromptStart)
                    {
                        fileParameters.Prompt += part.Substring(parametersKey.Length + 1) + "\n";
                    }
                    else
                    {
                        fileParameters.Prompt += part + "\n";
                    }
                    break;
                case 1:
                    if (isNegativePrompt)
                    {
                        fileParameters.NegativePrompt += part.Substring(negativePromptKey.Length);
                    }
                    else
                    {
                        fileParameters.NegativePrompt += part + "\n";
                    }
                    break;
                case 2:

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
                            case "Hypernet":
                                fileParameters.HyperNetwork = kvp[1].Trim();
                                break;
                            case "Hypernet strength":
                                fileParameters.HyperNetworkStrength = decimal.Parse(kvp[1].Trim());
                                break;
                            case "aesthetic_score":
                                fileParameters.AestheticScore = decimal.Parse(kvp[1].Trim());
                                break;
                        }
                    }

                    state = 3;

                    break;
            }

        }

        return fileParameters;
    }

    private static FileParameters ReadAutomatic1111Parameters(string file, IEnumerable<Directory> directories)
    {
        FileParameters fileParameters = null;

        var ext = Path.GetExtension(file).ToLower();

        //var parameters = directories.FirstOrDefault(d => d.Name == "PNG-tEXt")?.Tags
        //    .FirstOrDefault(t => t.Name == "Textual Data")?.Description;

        Tag tag;

        if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith("parameters:"), out tag))
        {
            fileParameters = ReadA111Parameters(tag.Description);
        }
        else
        {
            var parameterFile = file.Replace(ext, ".txt", StringComparison.InvariantCultureIgnoreCase);

            if (File.Exists(parameterFile))
            {
                var parameters = File.ReadAllText(parameterFile);
                fileParameters = ReadA111Parameters(parameters);
            }

        }

        if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith("aesthetic_score:"), out tag))
        {
            fileParameters.AestheticScore = decimal.Parse(tag.Description.Substring("aesthetic_score:".Length));
        }


        return fileParameters;
    }


    private static bool TryFindTag(IEnumerable<Directory> directories, string directoryName, string tagName, Func<Tag, bool> matchTag, out Tag foundTag)
    {
        foreach (var directory in directories)
        {
            if (directory.Name == directoryName)
            {
                foreach (var tag in directory.Tags)
                {
                    if (tag.Name == tagName)
                    {
                        if (matchTag(tag))
                        {
                            foundTag = tag;
                            return true;
                        }
                    }
                }
            }
        }
        foundTag = null;
        return false;
    }
}
using System.Text.Json;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using Directory = MetadataExtractor.Directory;
using Dir = System.IO.Directory;
using System.Globalization;
using MetadataExtractor.Formats.WebP;

namespace Diffusion.IO;

public class Metadata
{
    private static Dictionary<string, List<string>> DirectoryTextFileCache = new Dictionary<string, List<string>>();

    public static List<string> GetDirectoryTextFileCache(string path)
    {
        if (!DirectoryTextFileCache.TryGetValue(path, out var files))
        {
            files = Dir.GetFiles(path, "*.txt").ToList();
            DirectoryTextFileCache.Add(path, files);
        }

        return files;
    }

    public static FileParameters? ReadFromFile(string file)
    {
        FileParameters? fileParameters = null;


        var ext = Path.GetExtension(file).ToLower();

        switch (ext)
        {
            case ".png":
                {
                    IEnumerable<Directory> directories = PngMetadataReader.ReadMetadata(file);

                    var isNovelAI = directories.Any(d => d.Name == "PNG-tEXt" && d.Tags.Any(t => t.Name == "Textual Data" && t.Description == "Software: NovelAI"));

                    var isInvokeAI = directories.Any(d => d.Name == "PNG-tEXt" && d.Tags.Any(t => t.Name == "Textual Data" && t.Description.StartsWith("Dream: ")));

                    var isInvokeAINew = directories.Any(d => d.Name == "PNG-tEXt" && d.Tags.Any(t => t.Name == "Textual Data" && t.Description.StartsWith("sd-metadata: ")));

                    var isComfyUI = directories.Any(d => d.Name == "PNG-tEXt" && d.Tags.Any(t => t.Name == "Textual Data" && t.Description.StartsWith("prompt: ")));

                    if (isInvokeAINew)
                    {
                        fileParameters = ReadInvokeAIParametersNew(file, directories);
                    }
                    else if (isInvokeAI)
                    {
                        fileParameters = ReadInvokeAIParameters(file, directories);
                    }
                    else if (isNovelAI)
                    {
                        fileParameters = ReadNovelAIParameters(file, directories);
                    }
                    else if (isComfyUI)
                    {
                        fileParameters = ReadComfyUIParameters(file, directories);
                    }
                    else
                    {
                        fileParameters = ReadAutomatic1111Parameters(file, directories);
                    }

                    if (fileParameters != null && (fileParameters.Width == 0 || fileParameters.Height == 0))
                    {
                        var imageMetaData = directories.FirstOrDefault(d => d.Name == "PNG-IHDR");

                        foreach (var tag in imageMetaData.Tags)
                        {
                            if (tag.Name == "Image Width")
                            {
                                fileParameters.Width = int.Parse(tag.Description);
                            }
                            else if (tag.Name == "Image Height")
                            {
                                fileParameters.Height = int.Parse(tag.Description);
                            }
                        }
                    }


                    break;
                }
            case ".jpg" or ".jpeg":
                {
                    IEnumerable<Directory> directories = JpegMetadataReader.ReadMetadata(file);

                    fileParameters = ReadAutomatic1111Parameters(file, directories);

                    break;
                }
            case ".webp":
                {
                    IEnumerable<Directory> directories = WebPMetadataReader.ReadMetadata(file);

                    fileParameters = ReadAutomatic1111Parameters(file, directories);

                    break;
                }
        }

        if (fileParameters == null)
        {
            var parameterFile = file.Replace(ext, ".txt", StringComparison.InvariantCultureIgnoreCase);

            if (File.Exists(parameterFile))
            {
                var parameters = File.ReadAllText(parameterFile);
                fileParameters = DetectAndReadMetaType(parameters);
            }
            else
            {
                var currPath = Path.GetDirectoryName(parameterFile);
                var textFiles = GetDirectoryTextFileCache(currPath);

                var matchingFile = textFiles.FirstOrDefault(t => Path.GetFileNameWithoutExtension(parameterFile).StartsWith(Path.GetFileNameWithoutExtension(t)));

                if (matchingFile != null)
                {
                    var parameters = File.ReadAllText(matchingFile);
                    fileParameters = DetectAndReadMetaType(parameters);
                }
            }
        }

        fileParameters ??= new FileParameters()
        {
            NoMetadata = true
        };

        FileInfo fileInfo = new FileInfo(file);
        fileParameters.Path = file;
        fileParameters.FileSize = fileInfo.Length;

        return fileParameters;
    }

    private static FileParameters DetectAndReadMetaType(string parameters)
    {
        if (IsStableDiffusion(parameters))
        {
            return ReadStableDiffusionParameters(parameters);
        }
        else
        {
            return ReadA111Parameters(parameters);
        }
    }

    private static bool IsStableDiffusion(string metadata)
    {
        return metadata.Contains("\nWidth:") && metadata.Contains("\nHeight:") && metadata.Contains("\nSeed:");
    }

    private static FileParameters ReadInvokeAIParameters(string file, IEnumerable<Directory> directories)
    {
        if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith("Dream: "), out var tag))
        {
            var fp = new FileParameters();
            var command = tag.Description.Substring("Dream: ".Length);
            var start = command.IndexOf("\"");
            var end = command.IndexOf("\"", start + 1);
            fp.Prompt = command.Substring(start + 1, end - start - 1);
            var others = command.Substring(end + 1);
            var args = others.Split(new char[] { ' ' });
            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                switch (arg)
                {
                    case "-s":
                        fp.Steps = int.Parse(args[index + 1], CultureInfo.InvariantCulture);
                        index++;
                        break;
                    case "-S":
                        fp.Seed = long.Parse(args[index + 1], CultureInfo.InvariantCulture);
                        index++;
                        break;
                    case "-W":
                        fp.Width = int.Parse(args[index + 1], CultureInfo.InvariantCulture);
                        index++;
                        break;
                    case "-H":
                        fp.Height = int.Parse(args[index + 1], CultureInfo.InvariantCulture);
                        index++;
                        break;
                    case "-C":
                        fp.CFGScale = decimal.Parse(args[index + 1], CultureInfo.InvariantCulture);
                        index++;
                        break;
                    case "-A":
                        fp.Sampler = args[index + 1];
                        index++;
                        break;
                }
            }

            fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Size: {fp.Width}x{fp.Height}";

            return fp;
        }

        return null;
    }

    private static FileParameters ReadComfyUIParameters(string file, IEnumerable<Directory> directories)
    {
        if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith("prompt: "), out var tag))
        {
            var fp = new FileParameters();
            var json = tag.Description.Substring("prompt: ".Length);

            var root = JsonDocument.Parse(json);
            var nodes = root.RootElement.EnumerateObject().ToDictionary(o => o.Name, o => o.Value);

            var ksampler = nodes.Values.SingleOrDefault(o =>
            {
                if (o.TryGetProperty("class_type", out var element))
                {
                    return element.GetString() == "KSampler";
                }

                return false;
            });

            //"seed": 1102676403634909,
            //"steps": 20,
            //"cfg": 8.0,
            //"sampler_name": "dpmpp_2m",
            //"scheduler": "normal",
            //"denoise": 1.0,
            //"model": [
            //"4",
            //0
            //    ],
            //"positive": [
            //"6",
            //0
            //    ],
            //"negative": [
            //"7",
            //0
            //    ],
            //"latent_image": [
            //"14",
            //0
            //    ]


            var image = ksampler.GetProperty("inputs");

            if (image.TryGetProperty("positive", out var positive))
            {
                var promptIndex = positive.EnumerateArray().First().GetString();
                var promptObject = nodes[promptIndex].GetProperty("inputs");
                fp.Prompt = promptObject.GetProperty("text").GetString();
            }

            if (image.TryGetProperty("negative", out var negative))
            {
                var promptIndex = negative.EnumerateArray().First().GetString();
                var promptObject = nodes[promptIndex].GetProperty("inputs");
                fp.NegativePrompt = promptObject.GetProperty("text").GetString();
            }

            if (image.TryGetProperty("latent_image", out var latent_image))
            {
                var index = latent_image.EnumerateArray().First().GetString();
                var promptObject = nodes[index].GetProperty("inputs");
                var hasWidth = promptObject.TryGetProperty("width", out var widthObject);
                var hasHeight = promptObject.TryGetProperty("height", out var heightObject);

                if (hasWidth && hasHeight)
                {
                    fp.Width = widthObject.GetInt32();
                    fp.Height = heightObject.GetInt32();
                }
            }

            fp.Steps = image.GetProperty("steps").GetInt32();
            fp.CFGScale = image.GetProperty("cfg").GetDecimal();
            fp.Seed = image.GetProperty("seed").GetInt64();
            fp.Sampler = image.GetProperty("sampler_name").GetString();

            fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Size: {fp.Width}x{fp.Height}";

            return fp;
        }

        return null;
    }

    private static FileParameters ReadInvokeAIParametersNew(string file, IEnumerable<Directory> directories)
    {
        if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith("sd-metadata: "), out var tag))
        {
            var fp = new FileParameters();
            var json = tag.Description.Substring("sd-metadata: ".Length);
            var root = JsonDocument.Parse(json);
            var image = root.RootElement.GetProperty("image");
            var promptArray = image.GetProperty("prompt");
            var promptArrayEnumerator = promptArray.EnumerateArray();
            promptArrayEnumerator.MoveNext();
            var promptObject = promptArrayEnumerator.Current;

            fp.Prompt = promptObject.GetProperty("prompt").GetString();
            fp.PromptStrength = promptObject.GetProperty("weight").GetDecimal();

            fp.Steps = image.GetProperty("steps").GetInt32();
            fp.CFGScale = image.GetProperty("cfg_scale").GetDecimal();
            fp.Height = image.GetProperty("height").GetInt32();
            fp.Width = image.GetProperty("width").GetInt32();
            fp.Seed = image.GetProperty("seed").GetInt64();
            fp.Sampler = image.GetProperty("sampler").GetString();

            fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Size: {fp.Width}x{fp.Height}";

            return fp;
        }

        return null;
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
                        fileParameters.Width = int.Parse(ptag.Description, CultureInfo.InvariantCulture);
                        break;
                    case "Image Height":
                        fileParameters.Height = int.Parse(ptag.Description, CultureInfo.InvariantCulture);
                        break;
                }
            }
        }

        return fileParameters;
    }

    private static FileParameters ReadStableDiffusionParameters(string data)
    {
        var fileParameters = new FileParameters();

        var parts = data.Split(new[] { '\n' });

        const string negativePromptKey = "Negative prompt: ";
        const string modelKey = "Stable Diffusion model: ";
        const string widthKey = "Width: ";

        fileParameters.Parameters = data;

        var state = 0;

        fileParameters.Prompt = "";
        fileParameters.NegativePrompt = "";

        string otherParam = "";

        foreach (var part in parts)
        {
            var isNegativePrompt = part.StartsWith(negativePromptKey, StringComparison.InvariantCultureIgnoreCase);
            var isModel = part.StartsWith(modelKey, StringComparison.InvariantCultureIgnoreCase);
            var isWidth = part.StartsWith(widthKey, StringComparison.InvariantCultureIgnoreCase);

            if (isWidth)
            {
                state = 1;
            }
            else if (isNegativePrompt)
            {
                state = 2;
            }
            else if (isModel)
            {
                state = 3;
            }

            switch (state)
            {
                case 0:
                    fileParameters.Prompt += part + "\n";
                    break;
                case 2:
                    if (isNegativePrompt)
                    {
                        fileParameters.NegativePrompt += part.Substring(negativePromptKey.Length);
                    }
                    else
                    {
                        fileParameters.NegativePrompt += part + "\n";
                    }
                    break;
                case 1:


                    var subParts = part.Split(new[] { ',' });
                    foreach (var keyValue in subParts)
                    {
                        var kvp = keyValue.Split(new[] { ':' });
                        switch (kvp[0].Trim())
                        {
                            case "Steps":
                                fileParameters.Steps = int.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Sampler":
                                fileParameters.Sampler = kvp[1].Trim();
                                break;
                            case "Guidance Scale":
                                fileParameters.CFGScale = decimal.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Seed":
                                fileParameters.Seed = long.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Width":
                                fileParameters.Width = int.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Height":
                                fileParameters.Height = int.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Prompt Strength":
                                fileParameters.PromptStrength = decimal.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                                //case "Model hash":
                                //    fileParameters.ModelHash = kvp[1].Trim();
                                //    break;
                                //case "Batch size":
                                //    fileParameters.BatchSize = int.Parse(kvp[1].Trim());
                                //    break;
                                //case "Hypernet":
                                //    fileParameters.HyperNetwork = kvp[1].Trim();
                                //    break;
                                //case "Hypernet strength":
                                //    fileParameters.HyperNetworkStrength = decimal.Parse(kvp[1].Trim());
                                //    break;
                                //case "aesthetic_score":
                                //    fileParameters.AestheticScore = decimal.Parse(kvp[1].Trim());
                                //    break;
                        }
                    }

                    otherParam += part + "\n";

                    break;
                case 3:
                    otherParam += part + "\n";
                    break;
            }

        }

        fileParameters.OtherParameters = otherParam;

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

                    //var decimalFormatter = new DecimalFormatter();

                    var subParts = part.Split(new[] { ',' });
                    foreach (var keyValue in subParts)
                    {
                        var kvp = keyValue.Split(new[] { ':' });
                        switch (kvp[0].Trim())
                        {
                            case "Steps":
                                fileParameters.Steps = int.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Sampler":
                                fileParameters.Sampler = kvp[1].Trim();
                                break;
                            case "CFG scale":
                                fileParameters.CFGScale = decimal.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Seed":
                                fileParameters.Seed = long.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Size":
                                var size = kvp[1].Split(new[] { 'x' });
                                fileParameters.Width = int.Parse(size[0].Trim(), CultureInfo.InvariantCulture);
                                fileParameters.Height = int.Parse(size[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Model hash":
                                fileParameters.ModelHash = kvp[1].Trim();
                                break;
                            case "Model":
                                fileParameters.Model = kvp[1].Trim();
                                break;
                            case "Batch size":
                                fileParameters.BatchSize = int.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "Hypernet":
                                fileParameters.HyperNetwork = kvp[1].Trim();
                                break;
                            case "Hypernet strength":
                                fileParameters.HyperNetworkStrength = decimal.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
                                break;
                            case "aesthetic_score":
                                fileParameters.AestheticScore = decimal.Parse(kvp[1].Trim(), CultureInfo.InvariantCulture);
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


        if (TryFindTag(directories, "Exif SubIFD", "User Comment", tag => true, out tag))
        {
            fileParameters = ReadA111Parameters(tag.Description);
        }
        else if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith("parameters:"), out tag))
        {
            fileParameters = ReadA111Parameters(tag.Description);
        }
        else if (TryFindTag(directories, "PNG-iTXt", "Textual Data", tag => tag.Description.StartsWith("parameters:"), out tag))
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
            if (fileParameters == null)
            {
                fileParameters = new FileParameters();
            }
            fileParameters.AestheticScore = decimal.Parse(tag.Description.Substring("aesthetic_score:".Length), CultureInfo.InvariantCulture);
            fileParameters.OtherParameters ??= $"aesthetic_score: {fileParameters.AestheticScore}";
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

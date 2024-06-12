using System.Text.Json;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using Directory = MetadataExtractor.Directory;
using Dir = System.IO.Directory;
using System.Globalization;
using MetadataExtractor.Formats.WebP;
using Diffusion.Common;
using System.Linq;

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

    public enum MetaFormat
    {
        A1111,
        NovelAI,
        InvokeAI,
        InvokeAINew,
        InvokeAI2,
        EasyDiffusion,
        ComfyUI,
        RuinedFooocus,
        FooocusMRE,
        Fooocus,
        Unknown,
    }

    public enum FileType
    {
        PNG,
        JPEG,
        WebP,
        Other,
    }

    private static byte[] PNGMagic = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
    private static byte[] JPEGMagic = new byte[] { 0xFF, 0xD8, 0xFF };
    private static byte[] RIFFMagic = new byte[] { 0x52, 0x49, 0x46, 0x46 };
    private static byte[] WebPMagic = new byte[] { 0x57, 0x45, 0x42, 0x50 };

    private static FileType GetFileType(Stream stream)
    {
        var buffer = new byte[12];

        stream.Read(buffer, 0, 12);

        var span = buffer.AsSpan();

        if (span.Slice(0, 4).SequenceEqual(PNGMagic))
        {
            return FileType.PNG;
        }
        if (span.Slice(0, 3).SequenceEqual(JPEGMagic) && ((span[3] & 0xE0) == 0xE0))
        {
            return FileType.JPEG;
        }
        if (span.Slice(0, 4).SequenceEqual(RIFFMagic) && span.Slice(8, 4).SequenceEqual(WebPMagic))
        {
            return FileType.WebP;
        }

        return FileType.Other;
    }

    public static FileParameters? ReadFromFile(string file)
    {
        FileParameters? fileParameters = null;

        var ext = Path.GetExtension(file).ToLowerInvariant();

        using var stream = File.OpenRead(file);

        var fileType = GetFileType(stream);

        stream.Seek(0, SeekOrigin.Begin);

        switch (fileType)
        {
            case FileType.PNG:
                {
                    IEnumerable<Directory> directories = PngMetadataReader.ReadMetadata(stream);

                    var format = MetaFormat.Unknown;

                    decimal aestheticScore = 0;

                    //string tagData = null;

                    foreach (var directory in directories)
                    {
                        if (directory.Name == "PNG-tEXt")
                        {
                            foreach (var tag in directory.Tags)
                            {
                                if (tag.Name == "Textual Data")
                                {
                                    if (tag.Description.StartsWith("parameters:"))
                                    {
                                        if (fileParameters == null)
                                        {
                                            var isJson = tag.Description.Substring("parameters: ".Length).Trim().StartsWith("{");
                                            format = isJson ? MetaFormat.RuinedFooocus : MetaFormat.A1111;
                                            fileParameters = isJson ? ReadRuinedFooocusParameters(tag.Description) : ReadA111Parameters(tag.Description);
                                        }
                                    }
                                    else if (tag.Description.StartsWith("Comment:"))
                                    {
                                        if (fileParameters == null)
                                        {
                                            if (directory.Tags.Any(t => t.Description == "Software: NovelAI"))
                                            {
                                                format = MetaFormat.NovelAI;
                                                fileParameters = ReadNovelAIParameters(file, directories);
                                            }
                                            else
                                            {
                                                format = MetaFormat.FooocusMRE;
                                                fileParameters = ReadFooocusMREParameters(tag.Description);
                                            }
                                        }
                                    }
                                    else if (tag.Description == "Software: NovelAI")
                                    {
                                        if (fileParameters == null)
                                        {
                                            format = MetaFormat.NovelAI;
                                            fileParameters = ReadNovelAIParameters(file, directories);
                                        }
                                    }
                                    else if (tag.Description.StartsWith("Dream: "))
                                    {
                                        format = MetaFormat.InvokeAI;
                                        fileParameters = ReadInvokeAIParameters(file, tag.Description);
                                    }
                                    else if (tag.Description.StartsWith("sd-metadata: "))
                                    {
                                        format = MetaFormat.InvokeAINew;
                                        fileParameters = ReadInvokeAIParametersNew(file, tag.Description);
                                    }
                                    else if (tag.Description.StartsWith("invokeai_metadata: "))
                                    {
                                        format = MetaFormat.InvokeAI2;
                                        fileParameters = ReadInvokeAIParameters2(file, tag.Description);
                                    }
                                    else if (tag.Description.StartsWith("prompt: "))
                                    {
                                        if (fileParameters == null)
                                        {
                                            var isJson = tag.Description.Substring("prompt: ".Length).Trim().StartsWith("{");
                                            format = isJson ? MetaFormat.ComfyUI : MetaFormat.EasyDiffusion;
                                            fileParameters = isJson ? ReadComfyUIParameters(file, tag.Description) : ReadEasyDiffusionParameters(file, directories);
                                        }
                                    }
                                    else if (tag.Description.StartsWith("Score:"))
                                    {
                                        decimal.TryParse(tag.Description[6..], NumberStyles.Any, CultureInfo.InvariantCulture, out aestheticScore);
                                    }
                                    else if (tag.Description.StartsWith("aesthetic_score:"))
                                    {
                                        decimal.TryParse(tag.Description[16..], NumberStyles.Any, CultureInfo.InvariantCulture, out aestheticScore);
                                    }

                                }
                            }
                        }
                        else if (directory.Name == "PNG-iTXt")
                        {
                            foreach (var tag in directory.Tags)
                            {
                                if (tag.Name == "Textual Data" && tag.Description.StartsWith("parameters:"))
                                {
                                    format = MetaFormat.A1111;
                                    fileParameters = ReadA111Parameters(tag.Description);
                                }
                            }

                        }
                        else if (directory.Name == "Exif SubIFD")
                        {
                            foreach (var tag in directory.Tags)
                            {
                                if (tag.Name == "User Comment")
                                {
                                    format = MetaFormat.A1111;
                                    fileParameters = ReadA111Parameters(tag.Description);
                                }
                            }
                        }
                    }

                    try
                    {

                    }
                    catch (Exception e)
                    {
                        Logger.Log($"An error occurred while reading {file}: {e.Message}\r\n\r\n{e.StackTrace}");
                    }


                    if (aestheticScore > 0)
                    {
                        fileParameters ??= new FileParameters();
                        fileParameters.AestheticScore = aestheticScore;
                        if (fileParameters.OtherParameters == null)
                        {
                            fileParameters.OtherParameters = $"aesthetic_score: {fileParameters.AestheticScore}";
                        }
                        else
                        {
                            fileParameters.OtherParameters += $"\naesthetic_score: {fileParameters.AestheticScore}";
                        }
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
            case FileType.JPEG:
                {
                    var format = MetaFormat.Unknown;

                    IEnumerable<Directory> directories = JpegMetadataReader.ReadMetadata(stream);

                    try
                    {
                        var isFoocus = false;
                        foreach (var directory in directories)
                        {
                            if (directory.Name == "Exif SubIFD")
                            {
                                foreach (var tag in directory.Tags)
                                {
                                    switch (tag.Name)
                                    {
                                        case "User Comment":
                                            var description = tag.Description;
                                            if (description.Contains("sui_image_params"))
                                            {
                                                fileParameters = ReadStableSwarmParameters(description);
                                            }
                                            break;
                                    }
                                }
                            }
                            else if (directory.Name == "Exif IFD0")
                            {
                                foreach (var tag in directory.Tags)
                                {
                                    switch (tag.Name)
                                    {
                                        case "Software" when tag.Description.StartsWith("Fooocus"):
                                            isFoocus = true;
                                            break;

                                        case "Makernote":
                                            if (isFoocus)
                                            {
                                                format = tag.Description switch
                                                {
                                                    "fooocus" => MetaFormat.Fooocus,
                                                    "a1111" => MetaFormat.A1111,
                                                    _ => format
                                                };
                                            }
                                            break;

                                        case "User Comment":
                                            if (isFoocus)
                                            {
                                                fileParameters = format switch
                                                {
                                                    MetaFormat.Fooocus => ReadFooocusParameters(tag.Description),
                                                    MetaFormat.A1111 => ReadA111Parameters(tag.Description),
                                                    _ => fileParameters
                                                };
                                            }
                                            break;
                                    }
                                }


                            }
                        }

                        if (fileParameters == null)
                        {
                            format = MetaFormat.A1111;

                            fileParameters = ReadAutomatic1111Parameters(file, directories);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"An error occurred while reading {file}: {e.Message}\r\n\r\n{e.StackTrace}");
                    }

                    break;
                }
            case FileType.WebP:
                {
                    IEnumerable<Directory> directories = WebPMetadataReader.ReadMetadata(stream);

                    try
                    {
                        fileParameters = ReadAutomatic1111Parameters(file, directories);
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"An error occurred while reading {file}: {e.Message}\r\n\r\n{e.StackTrace}");
                    }

                    break;
                }
        }

        if (fileParameters == null)
        {
            try
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
            catch (Exception e)
            {
                Logger.Log($"An error occurred while reading {file}: {e.Message}\r\n\r\n{e.StackTrace}");
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

    private static FileParameters ReadInvokeAIParameters(string file, string description)
    {
        var fp = new FileParameters();
        var command = description.Substring("Dream: ".Length);
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



    private static FileParameters ReadEasyDiffusionParameters(string file, IEnumerable<Directory> directories)
    {
        var fp = new FileParameters();

        string? GetTag(string key)
        {
            if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith($"{key}: "), out var tag))
            {
                return tag.Description.Substring($"{key}: ".Length);
            }

            return null;
        }

        float GetFloatTag(string key)
        {
            if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith($"{key}: "), out var tag))
            {
                var value = tag.Description.Substring($"{key}: ".Length);

                return float.Parse(value);
            }

            return 0f;
        }

        decimal GetDecimalTag(string key)
        {
            if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith($"{key}: "), out var tag))
            {
                var value = tag.Description.Substring($"{key}: ".Length);

                return decimal.Parse(value);
            }

            return 0m;
        }


        int GetIntTag(string key)
        {
            if (TryFindTag(directories, "PNG-tEXt", "Textual Data", tag => tag.Description.StartsWith($"{key}: "), out var tag))
            {
                var value = tag.Description.Substring($"{key}: ".Length);

                return int.Parse(value);
            }

            return 0;
        }

        fp.Prompt = GetTag("prompt");
        fp.NegativePrompt = GetTag("negative_prompt");
        fp.Width = GetIntTag("width");
        fp.Height = GetIntTag("height");
        fp.Steps = GetIntTag("num_inference_steps");
        fp.CFGScale = GetDecimalTag("guidance_scale");
        fp.Seed = GetIntTag("seed");
        fp.Sampler = GetTag("sampler_name");
        fp.Model = GetTag("use_stable_diffusion_model");


        fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Seed: {fp.Seed} Size: {fp.Width}x{fp.Height}";

        //    return fp;

        return fp;
    }

    private static FileParameters ReadComfyUIParameters(string file, string description)
    {
        var fp = new FileParameters();

        try
        {
            var json = description.Substring("prompt: ".Length);

            // fix for errant nodes
            json = json.Replace("NaN", "null");

            var root = JsonDocument.Parse(json);
            var nodes = root.RootElement.EnumerateObject().ToDictionary(o => o.Name, o => o.Value);

            var isSDXL = false;
            var isEfficient = false;
            var isEfficientSDXL = false;

            var ksampler = nodes.Values.SingleOrDefault(o =>
            {
                if (o.TryGetProperty("class_type", out var element))
                {
                    return element.GetString() == "KSampler";
                }

                return false;
            });

            if (ksampler.ValueKind == JsonValueKind.Undefined)
            {
                ksampler = nodes.Values.FirstOrDefault(o =>
                {
                    if (o.TryGetProperty("class_type", out var element))
                    {
                        return element.GetString() == "KSampler (Efficient)";
                    }

                    return false;
                });

                if (ksampler.ValueKind != JsonValueKind.Undefined)
                {
                    isEfficient = true;
                }
            }

            if (ksampler.ValueKind == JsonValueKind.Undefined)
            {
                ksampler = nodes.Values.FirstOrDefault(o =>
                {
                    if (o.TryGetProperty("class_type", out var element))
                    {
                        return element.GetString() == "KSamplerAdvanced";
                    }

                    return false;
                });

                if (ksampler.ValueKind != JsonValueKind.Undefined)
                {
                    isSDXL = true;
                }
            }

            if (ksampler.ValueKind == JsonValueKind.Undefined)
            {
                ksampler = nodes.Values.FirstOrDefault(o =>
                {
                    if (o.TryGetProperty("class_type", out var element))
                    {
                        return element.GetString() == "Eff. Loader SDXL";
                    }

                    return false;
                });

                if (ksampler.ValueKind != JsonValueKind.Undefined)
                {
                    isEfficientSDXL = true;
                }
            }


            if (ksampler.ValueKind != JsonValueKind.Undefined)
            {

                var image = ksampler.GetProperty("inputs");

                if (image.TryGetProperty("positive", out var positive))
                {
                    var promptIndex = positive.EnumerateArray().First().GetString();
                    var promptObject = nodes[promptIndex].GetProperty("inputs");
                    if (isSDXL)
                    {
                        fp.Prompt = promptObject.GetProperty("text_g").GetString();
                    }
                    else if (isEfficient)
                    {
                        fp.Prompt = promptObject.GetProperty("positive").GetString();
                    }
                    else if (isEfficientSDXL)
                    {
                        fp.Prompt = promptObject.GetProperty("text").GetString();
                    }
                    else
                    {
                        fp.Prompt = promptObject.GetProperty("text").GetString();
                    }
                }

                if (image.TryGetProperty("negative", out var negative))
                {
                    if (negative.ValueKind == JsonValueKind.Array)
                    {
                        var promptIndex = negative.EnumerateArray().First().GetString();
                        var promptObject = nodes[promptIndex].GetProperty("inputs");
                        if (isSDXL)
                        {
                            fp.NegativePrompt = promptObject.GetProperty("text_g").GetString();
                        }
                        else if (isEfficient)
                        {
                            fp.NegativePrompt = promptObject.GetProperty("negative").GetString();
                        }
                        else
                        {
                            fp.NegativePrompt = promptObject.GetProperty("text").GetString();
                        }
                    }
                    else if (negative.ValueKind == JsonValueKind.String)
                    {
                        fp.NegativePrompt = negative.GetString();
                    }

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

                if (isSDXL)
                {
                    fp.Seed = image.GetProperty("noise_seed").GetInt64();
                }
                else
                {
                    var seed = image.GetProperty("seed");

                    if (seed.ValueKind == JsonValueKind.Number)
                    {
                        fp.Seed = seed.GetInt64();
                    }
                }

                fp.Sampler = image.GetProperty("sampler_name").GetString();

                fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Seed: {fp.Seed} Size: {fp.Width}x{fp.Height}";
            }

            return fp;
        }
        catch
        {
            return fp;
        }
    }

    private static FileParameters ReadInvokeAIParametersNew(string file, string description)
    {
        var fp = new FileParameters();
        var json = description.Substring("sd-metadata: ".Length);
        var root = JsonDocument.Parse(json);
        var image = root.RootElement.GetProperty("image");
        var prompt = image.GetProperty("prompt");
        if (prompt.ValueKind == JsonValueKind.Array)
        {
            var promptArrayEnumerator = prompt.EnumerateArray();
            promptArrayEnumerator.MoveNext();
            var promptObject = promptArrayEnumerator.Current;
            fp.Prompt = promptObject.GetProperty("prompt").GetString();
            fp.PromptStrength = promptObject.GetProperty("weight").GetDecimal();
        }
        else if (prompt.ValueKind == JsonValueKind.String)
        {
            fp.Prompt = prompt.GetString();
        }

        fp.ModelHash = root.RootElement.GetProperty("model_hash").GetString();
        fp.Steps = image.GetProperty("steps").GetInt32();
        fp.CFGScale = image.GetProperty("cfg_scale").GetDecimal();
        fp.Height = image.GetProperty("height").GetInt32();
        fp.Width = image.GetProperty("width").GetInt32();
        fp.Seed = image.GetProperty("seed").GetInt64();
        fp.Sampler = image.GetProperty("sampler").GetString();

        fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Seed: {fp.Seed} Size: {fp.Width}x{fp.Height}";

        return fp;
    }

    private static FileParameters ReadInvokeAIParameters2(string file, string description)
    {
        var fp = new FileParameters();
        var json = description.Substring("invokeai_metadata: ".Length);
        var root = JsonDocument.Parse(json);
        var image = root.RootElement;

        fp.Prompt = image.GetProperty("positive_prompt").GetString();
        fp.NegativePrompt = image.GetProperty("negative_prompt").GetString();
        fp.Steps = image.GetProperty("steps").GetInt32();
        fp.CFGScale = image.GetProperty("cfg_scale").GetDecimal();
        fp.Height = image.GetProperty("height").GetInt32();
        fp.Width = image.GetProperty("width").GetInt32();
        fp.Seed = image.GetProperty("seed").GetInt64();
        fp.Sampler = image.GetProperty("scheduler").GetString();

        fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Seed: {fp.Seed} Size: {fp.Width}x{fp.Height}";

        return fp;
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
                if (property.Name != "uc" && property.Name != "prompt")
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

    private static FileParameters ReadFooocusMREParameters(string data)
    {
        var fp = new FileParameters();
        var json = data.Substring("Comment: ".Length);
        var root = JsonDocument.Parse(json);

        fp.Prompt = root.RootElement.GetProperty("prompt").GetString();

        string real_prompt;

        try
        {
            var real_prompt_array = root.RootElement.GetProperty("real_prompt").EnumerateArray();
            real_prompt = string.Join(", ", real_prompt_array);

            if (!string.IsNullOrEmpty(real_prompt))
            {
                if (!string.IsNullOrEmpty(fp.Prompt))
                    fp.Prompt += ", ";

                fp.Prompt += real_prompt;
            }
        }
        catch
        {
            // Ignore real_prompt if it fails
        }

        fp.NegativePrompt = root.RootElement.GetProperty("negative_prompt").GetString();

        string real_negative_prompt;

        try
        {
            var real_negative_prompt_array = root.RootElement.GetProperty("real_negative_prompt").EnumerateArray();
            real_negative_prompt = string.Join(", ", real_negative_prompt_array);

            if (!string.IsNullOrEmpty(real_negative_prompt))
            {
                if (!string.IsNullOrEmpty(fp.NegativePrompt))
                    fp.NegativePrompt += ", ";

                fp.NegativePrompt += real_negative_prompt;
            }
        }
        catch
        {
            // Ignore real_negative_prompt if it fails
        }

        fp.PromptStrength = root.RootElement.GetProperty("positive_prompt_strength").GetDecimal();
        fp.Steps = root.RootElement.GetProperty("steps").GetInt32();
        fp.CFGScale = root.RootElement.GetProperty("cfg").GetDecimal();
        fp.Height = root.RootElement.GetProperty("height").GetInt32();
        fp.Width = root.RootElement.GetProperty("width").GetInt32();
        fp.Seed = root.RootElement.GetProperty("seed").GetInt64();
        fp.Sampler = root.RootElement.GetProperty("sampler").GetString();
        fp.Model = root.RootElement.GetProperty("base_model").GetString();

        fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Size: {fp.Width}x{fp.Height}";

        return fp;
    }


    private static FileParameters ReadStableSwarmParameters(string data)
    {
        var json = JsonDocument.Parse(data);

        var root = json.RootElement;

        var fp = new FileParameters();

        var suiRoot = root.GetProperty("sui_image_params");

        fp.Prompt = suiRoot.GetProperty("prompt").GetString();
        fp.NegativePrompt = suiRoot.GetProperty("negativeprompt").GetString();
        fp.Steps = suiRoot.GetProperty("steps").GetInt32();
        fp.CFGScale = suiRoot.GetProperty("cfgscale").GetDecimal();

        //var resolution = root.GetProperty("resolution").GetString();
        //resolution = resolution.Substring(1, resolution.Length - 2);
        //var parts = resolution.Split(new[] { ',' }, StringSplitOptions.TrimEntries);

        fp.Width = suiRoot.GetProperty("width").GetInt32();
        fp.Height = suiRoot.GetProperty("height").GetInt32();
        fp.Seed = suiRoot.GetProperty("seed").GetInt64();

        if (suiRoot.TryGetProperty("sampler", out var sampler))
        {
            fp.Sampler = sampler.GetString();
        }

        if (suiRoot.TryGetProperty("model", out var model))
        {
            fp.Model = model.GetString();
        }

        //fp.ModelHash = root.GetProperty("base_model_hash").GetString();
        fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Seed: {fp.Seed} Size: {fp.Width}x{fp.Height}";

        return fp;
    }

    private static FileParameters ReadFooocusParameters(string data)
    {
        var json = JsonDocument.Parse(data);

        var root = json.RootElement;

        var fp = new FileParameters();

        var fullPrompt = root.GetProperty("full_prompt").EnumerateArray().Select(x => x.GetString());
        var fullNegativePrompt = root.GetProperty("full_negative_prompt").EnumerateArray().Select(x => x.GetString());

        fp.Prompt = string.Join("\r\n", fullPrompt);
        fp.NegativePrompt = string.Join("\r\n", fullNegativePrompt);
        fp.Steps = root.GetProperty("steps").GetInt32();
        fp.CFGScale = root.GetProperty("guidance_scale").GetDecimal();

        var resolution = root.GetProperty("resolution").GetString();
        resolution = resolution.Substring(1, resolution.Length - 2);
        var parts = resolution.Split(new[] { ',' }, StringSplitOptions.TrimEntries);

        fp.Width = int.Parse(parts[0]);
        fp.Height = int.Parse(parts[1]);
        fp.Seed = root.GetProperty("seed").GetInt64();
        fp.Sampler = root.GetProperty("sampler").GetString();
        fp.Model = root.GetProperty("base_model").GetString();
        fp.ModelHash = root.GetProperty("base_model_hash").GetString();
        fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Seed: {fp.Seed} Size: {fp.Width}x{fp.Height}";

        return fp;
    }

    private static FileParameters ReadRuinedFooocusParameters(string data)
    {
        var json = JsonDocument.Parse(data.Substring("parameters: ".Length));

        var root = json.RootElement;

        var fp = new FileParameters();

        if (root.TryGetProperty("software", out var softwareProperty))
        {
            var software = softwareProperty.GetString();

            if (software == "RuinedFooocus")
            {
                fp.Prompt = root.GetProperty("Prompt").GetString();
                fp.NegativePrompt = root.GetProperty("Negative").GetString();
                fp.Steps = root.GetProperty("steps").GetInt32();
                fp.CFGScale = root.GetProperty("cfg").GetDecimal();
                fp.Width = root.GetProperty("width").GetInt32();
                fp.Height = root.GetProperty("height").GetInt32();
                fp.Seed = root.GetProperty("seed").GetInt64();
                fp.Sampler = root.GetProperty("sampler_name").GetString();
                fp.Model = root.GetProperty("base_model_name").GetString();
                fp.ModelHash = root.GetProperty("base_model_hash").GetString();
                fp.OtherParameters = $"Steps: {fp.Steps} Sampler: {fp.Sampler} CFG Scale: {fp.CFGScale} Seed: {fp.Seed} Size: {fp.Width}x{fp.Height}";
            }
        }
        else
        {
            fp = ReadFooocusParameters(data.Substring("parameters: ".Length));
        }



        return fp;
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
                            case "Score":
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

        decimal aestheticScore = 0m;


        foreach (var directory in directories)
        {
            if (directory.Name == "PNG-tEXt")
            {
                foreach (var tag in directory.Tags)
                {
                    if (tag.Name == "Textual Data")
                    {
                        if (tag.Description.StartsWith("parameters:"))
                        {
                            fileParameters = ReadA111Parameters(tag.Description);
                        }
                        else if (tag.Description.StartsWith("Score:"))
                        {
                            decimal.TryParse(tag.Description[6..], NumberStyles.Any, CultureInfo.InvariantCulture, out aestheticScore);
                        }
                        else if (tag.Description.StartsWith("aesthetic_score:"))
                        {
                            decimal.TryParse(tag.Description[16..], NumberStyles.Any, CultureInfo.InvariantCulture, out aestheticScore);
                        }
                    }
                }
            }
            else if (directory.Name == "PNG-iTXt")
            {
                foreach (var tag in directory.Tags)
                {
                    if (tag.Name == "Textual Data" && tag.Description.StartsWith("parameters:"))
                    {
                        fileParameters = ReadA111Parameters(tag.Description);
                    }
                }

            }
            else if (directory.Name == "Exif SubIFD")
            {
                foreach (var tag in directory.Tags)
                {
                    if (tag.Name == "User Comment")
                    {
                        fileParameters = ReadA111Parameters(tag.Description);
                    }
                }
            }

        }


        if (fileParameters == null)
        {
            var parameterFile = file.Replace(ext, ".txt", StringComparison.InvariantCultureIgnoreCase);

            if (File.Exists(parameterFile))
            {
                var parameters = File.ReadAllText(parameterFile);
                fileParameters = ReadA111Parameters(parameters);
            }
        }



        if (aestheticScore > 0)
        {
            fileParameters ??= new FileParameters();
            fileParameters.AestheticScore = aestheticScore;
            if (fileParameters.OtherParameters == null)
            {
                fileParameters.OtherParameters = $"aesthetic_score: {fileParameters.AestheticScore}";
            }
            else
            {
                fileParameters.OtherParameters += $"\naesthetic_score: {fileParameters.AestheticScore}";
            }
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

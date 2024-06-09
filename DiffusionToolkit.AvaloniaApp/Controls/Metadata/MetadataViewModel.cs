using System;
using Avalonia;
using Diffusion.IO;

namespace DiffusionToolkit.AvaloniaApp.Controls.Metadata;

public class MetadataViewModel
{
    public string Path { get; set; }
    public string? Prompt { get; set; }
    public string? NegativePrompt { get; set; }
    public int Steps { get; set; }
    public string? Sampler { get; set; }
    public decimal CFGScale { get; set; }
    public long Seed { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? ModelHash { get; set; }
    public string? Model { get; set; }
    public int BatchSize { get; set; }
    public int BatchPos { get; set; }
    public string? OtherParameters { get; set; }
    public string Parameters { get; set; }
    public decimal? AestheticScore { get; set; }
    public string? HyperNetwork { get; set; }
    public decimal? HyperNetworkStrength { get; set; }
    public int? ClipSkip { get; set; }
    public int? ENSD { get; set; }
    public decimal? PromptStrength { get; set; }
    public long FileSize { get; set; }
    public bool NoMetadata { get; set; }
    public DateTime CreatedDate { get; set; }
    public string RawData { get; set; }
    public string Size => $"{Width}x{Height}";

    public static MetadataViewModel FromFileParameters(FileParameters fp)
    {
        return new MetadataViewModel()
        {
            Prompt = fp.Prompt,
            NegativePrompt = fp.NegativePrompt,
            Height = fp.Height,
            Width = fp.Width,
            Sampler = fp.Sampler,
            Seed = fp.Seed,
            Steps = fp.Steps,
            CFGScale = fp.CFGScale,
            Model = fp.Model,
            ModelHash = fp.ModelHash,
            FileSize = fp.FileSize,
            ClipSkip = fp.ClipSkip,
            BatchPos = fp.BatchPos,
            BatchSize = fp.BatchSize,
            OtherParameters = fp.OtherParameters,
            AestheticScore = fp.AestheticScore,
            Path = fp.Path,
            CreatedDate = fp.CreatedDate,
            RawData = fp.RawData,
        };
    }
}
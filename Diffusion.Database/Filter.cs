﻿namespace Diffusion.Database;

public class Filter
{

    public bool UsePrompt { get; set; }
    public string Prompt { get; set; }
    public bool UseNegativePrompt { get; set; }
    public string NegativePrompt { get; set; }
    public bool UseSteps { get; set; }
    public string Steps { get; set; }
    public bool UseSampler { get; set; }
    public string Sampler { get; set; }
    public bool UseSeed { get; set; }
    public string? SeedStart { get; set; }
    public string? SeedEnd { get; set; }
    public bool UseCFGScale { get; set; }
    public string CFGScale { get; set; }
    public bool UseSize { get; set; }
    public string Width { get; set; }
    public string Height { get; set; }
    public bool UseModelHash { get; set; }
    public string ModelHash { get; set; }
    public bool UseModelName { get; set; }
    public string ModelName { get; set; }

    public bool UseFavorite { get; set; }
    public bool Favorite { get; set; }
    public bool UseRating { get; set; }
    public string RatingOp { get; set; }
    public int? Rating { get; set; }
    public bool Unrated { get; set; }
    public bool UseNSFW { get; set; }
    public bool NSFW { get; set; }
    public bool UseForDeletion { get; set; }
    public bool ForDeletion { get; set; }

    public bool UseBatchSize { get; set; }
    public int BatchSize { get; set; }

    public bool UseBatchPos { get; set; }
    public int BatchPos { get; set; }
        
    public bool UseAestheticScore { get; set; }
    public bool NoAestheticScore { get; set; }
    public string AestheticScoreOp { get; set; }
    public double? AestheticScore { get; set; }
        
    public bool UsePath { get; set; }
    public string Path { get; set; }
        
    public bool UseCreationDate { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }

    public bool UseHyperNet { get; set; }
    public string HyperNet { get; set; }
        
    public bool UseHyperNetStr { get; set; }
    public string HyperNetStrOp { get; set; }
    public double HyperNetStr { get; set; }
        
    public bool UseNoMetadata { get; set; }
    public bool NoMetadata { get; set; }

    public bool IsEmpty => !(UsePrompt ||
                            UseNegativePrompt ||
                            UseSteps ||
                            UseSampler ||
                            UseSeed ||
                            UseCFGScale ||
                            UseSize ||
                            UseModelHash ||
                            UseModelName ||
                            UseFavorite ||
                            UseRating ||
                            Unrated ||
                            UseNSFW ||
                            UseForDeletion ||
                            UseBatchSize ||
                            UseBatchPos ||
                            NoAestheticScore ||
                            UseAestheticScore ||
                            UsePath ||
                            UseCreationDate ||
                            UseHyperNet ||
                            UseHyperNetStr ||
                            UseNoMetadata ||
                            UseAlbum || 
                            UseFolder);

    public bool UseFolder => !string.IsNullOrEmpty(Folder);
    public string? Folder { get; set; }
    public bool UseAlbum => !string.IsNullOrEmpty(Album);
    public string? Album { get; set; }
}
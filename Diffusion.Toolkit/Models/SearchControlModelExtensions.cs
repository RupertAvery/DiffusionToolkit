using System.Collections.ObjectModel;
using System.Linq;
using Diffusion.Common;
using Diffusion.Toolkit.Controls;
using NodeFilter = Diffusion.Common.NodeFilter;

namespace Diffusion.Toolkit.Models;

public static class SearchControlModelExtensions
{
    public static FilterControlModel AsModel(this Filter filter)
    {
        var model = new FilterControlModel();

        model.UsePrompt = filter.UsePrompt;
        model.Prompt = filter.Prompt;
        model.UsePromptEx = filter.UsePromptEx;
        model.PromptEx = filter.PromptEx;
        model.UseNegativePrompt = filter.UseNegativePrompt;
        model.NegativePrompt = filter.NegativePrompt;
        model.UseNegativePromptEx = filter.UseNegativePromptEx;
        model.NegativePromptEx = filter.NegativePromptEx;

        model.UseSteps = filter.UseSteps;
        model.Steps = filter.Steps;
        model.UseSampler = filter.UseSampler;
        model.Sampler = filter.Sampler;
        model.UseSeed = filter.UseSeed;
        model.SeedStart = filter.SeedStart;
        model.SeedEnd = filter.SeedEnd;
        model.UseCFGScale = filter.UseCFGScale;
        model.CFGScale = filter.CFGScale;
        model.UseSize = filter.UseSize;
        model.SizeOp = filter.SizeOp;
        model.UseSize = filter.UseSize;
        model.Width = filter.Width;
        model.Height = filter.Height;
        model.UseModelHash = filter.UseModelHash;
        model.ModelHash = filter.ModelHash;
        model.UseModelName = filter.UseModelName;
        model.ModelName = filter.ModelName;

        model.UseFavorite = filter.UseFavorite;
        model.Favorite = filter.Favorite;
        model.UseRating = filter.UseRating;
        model.RatingOp = filter.RatingOp;
        model.Rating = filter.Rating;
        model.Unrated = filter.Unrated;
        model.UseNSFW = filter.UseNSFW;
        model.NSFW = filter.NSFW;

        model.UseForDeletion = filter.UseForDeletion;
        model.ForDeletion = filter.ForDeletion;

        model.UseBatchSize = filter.UseBatchSize;
        model.BatchSize = filter.BatchSize;

        model.UseBatchPos = filter.UseBatchPos;
        model.BatchPos = filter.BatchPos;

        model.NoAestheticScore = filter.NoAestheticScore;
        model.UseAestheticScore = filter.UseAestheticScore;
        model.AestheticScoreOp = filter.AestheticScoreOp;
        model.AestheticScore = filter.AestheticScore;

        model.UsePath = filter.UsePath;
        model.Path = filter.Path;

        model.UseCreationDate = filter.UseCreationDate;
        model.Start = filter.Start;
        model.End = filter.End;

        model.UseHyperNet = filter.UseHyperNet;
        model.HyperNet = filter.HyperNet;

        model.UseHyperNetStr = filter.UseHyperNetStr;
        model.HyperNetStrOp = filter.HyperNetStrOp;
        model.HyperNetStr = filter.HyperNetStr;

        model.UseNoMetadata = filter.UseNoMetadata;
        model.NoMetadata = filter.NoMetadata;

        model.UseInAlbum = filter.UseInAlbum;
        model.InAlbum = filter.InAlbum;

        model.UseUnavailable = filter.UseUnavailable;
        model.Unavailable = filter.Unavailable;

        model.NodeFilters = new ObservableCollection<Controls.NodeFilter>(filter.NodeFilters.Select(d => new Controls.NodeFilter()
        {
            IsActive = d.IsActive,
            Operation = d.Operation,
            Node = d.Node,
            Property = d.Property,
            Comparison = d.Comparison,
            Value = d.Value,
        }));

        return model;
    }

    public static Filter AsFilter(this FilterControlModel model)
    {
        var filter = new Filter();

        filter.UsePrompt = model.UsePrompt;
        filter.Prompt = model.Prompt;
        filter.UsePromptEx = model.UsePromptEx;
        filter.PromptEx = model.PromptEx;
        filter.UseNegativePrompt = model.UseNegativePrompt;
        filter.NegativePrompt = model.NegativePrompt;
        filter.UseNegativePromptEx = model.UseNegativePromptEx;
        filter.NegativePromptEx = model.NegativePromptEx;

        filter.UseSteps = model.UseSteps;
        filter.Steps = model.Steps;
        filter.UseSampler = model.UseSampler;
        filter.Sampler = model.Sampler;
        filter.UseSeed = model.UseSeed;
        filter.SeedStart = model.SeedStart;
        filter.SeedEnd = model.SeedEnd;
        filter.UseCFGScale = model.UseCFGScale;
        filter.CFGScale = model.CFGScale;
        filter.UseSize = model.UseSize;
        filter.SizeOp = model.SizeOp;
        filter.Width = model.Width;
        filter.Height = model.Height;
        filter.UseModelHash = model.UseModelHash;
        filter.ModelHash = model.ModelHash;
        filter.UseModelName = model.UseModelName;
        filter.ModelName = model.ModelName;

        filter.UseFavorite = model.UseFavorite;
        filter.Favorite = model.Favorite;
        filter.UseRating = model.UseRating;
        filter.RatingOp = model.RatingOp;
        filter.Rating = model.Rating;
        filter.Unrated = model.Unrated;
        filter.UseNSFW = model.UseNSFW;
        filter.NSFW = model.NSFW;
        
        filter.UseForDeletion = model.UseForDeletion;
        filter.ForDeletion = model.ForDeletion;

        filter.UseBatchSize = model.UseBatchSize;
        filter.BatchSize = model.BatchSize;

        filter.UseBatchPos = model.UseBatchPos;
        filter.BatchPos = model.BatchPos;

        filter.NoAestheticScore = model.NoAestheticScore;
        filter.UseAestheticScore = model.UseAestheticScore;
        filter.AestheticScoreOp = model.AestheticScoreOp;
        filter.AestheticScore = model.AestheticScore;

        filter.UsePath = model.UsePath;
        filter.Path = model.Path;

        filter.UseCreationDate = model.UseCreationDate;
        filter.Start = model.Start;
        filter.End = model.End;

        filter.UseHyperNet = model.UseHyperNet;
        filter.HyperNet = model.HyperNet;

        filter.UseHyperNetStr = model.UseHyperNetStr;
        filter.HyperNetStrOp = model.HyperNetStrOp;
        filter.HyperNetStr = model.HyperNetStr;

        filter.UseNoMetadata = model.UseNoMetadata;
        filter.NoMetadata = model.NoMetadata;

        filter.UseInAlbum = model.UseInAlbum;
        filter.InAlbum = model.InAlbum;

        filter.UseUnavailable = model.UseUnavailable;
        filter.Unavailable = model.Unavailable;

        filter.NodeFilters = model.NodeFilters.Select(d => new NodeFilter()
        {
            IsActive = d.IsActive,
            Operation = d.Operation,
            Node = d.Node,
            Property = d.Property,
            Comparison = d.Comparison,
            Value = d.Value,
        }).ToList();

        return filter;
    }


}
using Diffusion.Database;
using System;

namespace Diffusion.Toolkit.Controls;

public class SearchControlModel : BaseNotify
{
    private bool _usePrompt;
    private string _prompt;
    private bool _useNegativePrompt;
    private string _negativePrompt;
    private bool _useSteps;
    private string _steps;
    private bool _useSampler;
    private string _sampler;
    private bool _useSeed;
    private long? _seedStart;
    private long? _seedEnd;
    private bool _useCfgScale;
    private string _cfgScale;
    private bool _useSize;
    private string _width;
    private string _height;
    private bool _useModelHash;
    private string _modelHash;
    private bool _useModelName;
    private string _modelName;
    private bool _useFavorite;
    private bool _favorite;
    private bool _useRating;
    private string _ratingOp;
    private int? _rating;
    private bool _unrated;
    private bool _useNsfw;
    private bool _nsfw;
    private bool _useForDeletion;
    private bool _forDeletion;
    private bool _useBatchSize;
    private int _batchSize;
    private bool _useBatchPos;
    private int _batchPos;
    private bool _useAestheticScore;
    private string _aestheticScoreOp;
    private double? _aestheticScore;
    private bool _usePath;
    private string _path;
    private bool _useCreationDate;
    private DateTime? _start;
    private DateTime? _end;
    private bool _useHyperNet;
    private string _hyperNet;
    private bool _useHyperNetStr;
    private string _hyperNetStrOp;
    private double _hyperNetStr;
    private bool _useNoMetadata;
    private bool _noMetadata;

    public bool UsePrompt
    {
        get => _usePrompt;
        set => SetField(ref _usePrompt, value);
    }

    public string Prompt
    {
        get => _prompt;
        set => SetField(ref _prompt, value);
    }

    public bool UseNegativePrompt
    {
        get => _useNegativePrompt;
        set => SetField(ref _useNegativePrompt, value);
    }

    public string NegativePrompt
    {
        get => _negativePrompt;
        set => SetField(ref _negativePrompt, value);
    }

    public bool UseSteps
    {
        get => _useSteps;
        set => SetField(ref _useSteps, value);
    }

    public string Steps
    {
        get => _steps;
        set => SetField(ref _steps, value);
    }

    public bool UseSampler
    {
        get => _useSampler;
        set => SetField(ref _useSampler, value);
    }

    public string Sampler
    {
        get => _sampler;
        set => SetField(ref _sampler, value);
    }

    public bool UseSeed
    {
        get => _useSeed;
        set => SetField(ref _useSeed, value);
    }

    public long? SeedStart
    {
        get => _seedStart;
        set => SetField(ref _seedStart, value);
    }

    public long? SeedEnd
    {
        get => _seedEnd;
        set => SetField(ref _seedEnd, value);
    }

    public bool UseCFGScale
    {
        get => _useCfgScale;
        set => SetField(ref _useCfgScale, value);
    }

    public string CFGScale
    {
        get => _cfgScale;
        set => SetField(ref _cfgScale, value);
    }

    public bool UseSize
    {
        get => _useSize;
        set => SetField(ref _useSize, value);
    }

    public string Width
    {
        get => _width;
        set => SetField(ref _width, value);
    }

    public string Height
    {
        get => _height;
        set => SetField(ref _height, value);
    }

    public bool UseModelHash
    {
        get => _useModelHash;
        set => SetField(ref _useModelHash, value);
    }

    public string ModelHash
    {
        get => _modelHash;
        set => SetField(ref _modelHash, value);
    }

    public bool UseModelName
    {
        get => _useModelName;
        set => SetField(ref _useModelName, value);
    }

    public string ModelName
    {
        get => _modelName;
        set => SetField(ref _modelName, value);
    }

    public bool UseFavorite
    {
        get => _useFavorite;
        set => SetField(ref _useFavorite, value);
    }

    public bool Favorite
    {
        get => _favorite;
        set => SetField(ref _favorite, value);
    }

    public bool UseRating
    {
        get => _useRating;
        set => SetField(ref _useRating, value);
    }

    public string RatingOp
    {
        get => _ratingOp;
        set => SetField(ref _ratingOp, value);
    }

    public int? Rating
    {
        get => _rating;
        set => SetField(ref _rating, value);
    }

    public bool Unrated
    {
        get => _unrated;
        set => SetField(ref _unrated, value);
    }

    public bool UseNSFW
    {
        get => _useNsfw;
        set => SetField(ref _useNsfw, value);
    }

    public bool NSFW
    {
        get => _nsfw;
        set => SetField(ref _nsfw, value);
    }

    public bool UseForDeletion
    {
        get => _useForDeletion;
        set => SetField(ref _useForDeletion, value);
    }

    public bool ForDeletion
    {
        get => _forDeletion;
        set => SetField(ref _forDeletion, value);
    }

    public bool UseBatchSize
    {
        get => _useBatchSize;
        set => SetField(ref _useBatchSize, value);
    }

    public int BatchSize
    {
        get => _batchSize;
        set => SetField(ref _batchSize, value);
    }

    public bool UseBatchPos
    {
        get => _useBatchPos;
        set => SetField(ref _useBatchPos, value);
    }

    public int BatchPos
    {
        get => _batchPos;
        set => SetField(ref _batchPos, value);
    }

    public bool UseAestheticScore
    {
        get => _useAestheticScore;
        set => SetField(ref _useAestheticScore, value);
    }

    public string AestheticScoreOp
    {
        get => _aestheticScoreOp;
        set => SetField(ref _aestheticScoreOp, value);
    }

    public double? AestheticScore
    {
        get => _aestheticScore;
        set => SetField(ref _aestheticScore, value);
    }

    public bool UsePath
    {
        get => _usePath;
        set => SetField(ref _usePath, value);
    }

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    public bool UseCreationDate
    {
        get => _useCreationDate;
        set => SetField(ref _useCreationDate, value);
    }

    public DateTime? Start
    {
        get => _start;
        set => SetField(ref _start, value);
    }

    public DateTime? End
    {
        get => _end;
        set => SetField(ref _end, value);
    }

    public bool UseHyperNet
    {
        get => _useHyperNet;
        set => SetField(ref _useHyperNet, value);
    }

    public string HyperNet
    {
        get => _hyperNet;
        set => SetField(ref _hyperNet, value);
    }

    public bool UseHyperNetStr
    {
        get => _useHyperNetStr;
        set => SetField(ref _useHyperNetStr, value);
    }

    public string HyperNetStrOp
    {
        get => _hyperNetStrOp;
        set => SetField(ref _hyperNetStrOp, value);
    }

    public double HyperNetStr
    {
        get => _hyperNetStr;
        set => SetField(ref _hyperNetStr, value);
    }

    public bool UseNoMetadata
    {
        get => _useNoMetadata;
        set => SetField(ref _useNoMetadata, value);
    }

    public bool NoMetadata
    {
        get => _noMetadata;
        set => SetField(ref _noMetadata, value);
    }

    public void Clear()
    {
        Prompt = String.Empty;
        NegativePrompt = String.Empty;
        Steps = String.Empty;
        Sampler = String.Empty;
        SeedStart = null;
        SeedEnd = null;
        CFGScale = String.Empty;
        Width = String.Empty;
        Height = String.Empty;
        ModelHash = String.Empty;
        ModelName = String.Empty;
        Favorite = false;
        Rating = null;
        RatingOp = String.Empty;
        Unrated = false;
        NSFW = false;
        ForDeletion = false;
        BatchSize = 0;
        BatchPos = 0;
        AestheticScoreOp = String.Empty;
        AestheticScore = null;
        Path = String.Empty;
        Start = null;
        End = null;
        HyperNet = String.Empty;
        HyperNetStr = 0;
        NoMetadata = false;


        UsePrompt = false;
        UseNegativePrompt = false;
        UseSteps = false;
        UseSampler = false;
        UseSeed = false;
        UseCFGScale = false;
        UseSize = false;
        UseModelHash = false;
        UseModelName = false;
        UseFavorite = false;
        UseRating = false;
        UseNSFW = false;
        UseForDeletion = false;
        UseBatchSize = false;
        UseBatchPos = false;
        UseAestheticScore = false;
        UsePath = false;
        UseCreationDate = false;
        UseHyperNet = false;
        UseHyperNetStr = false;
        UseNoMetadata = false;

    }
}
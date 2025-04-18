using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls;

public class FilterControlModel : BaseNotify
{
    private bool _usePrompt;
    private string _prompt;
    private bool _usePromptEx;
    private string _promptEx;
    private bool _useNegativePrompt;
    private string _negativePrompt;
    private bool _useNegativePromptEx;
    private string _negativePromptEx;
    private bool _useSteps;
    private string _steps;
    private bool _useSampler;
    private string _sampler;
    private bool _useSeed;
    private string? _seedStart;
    private string? _seedEnd;
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
    private bool _noAestheticScore;
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
    private bool _useInAlbum;
    private bool _inAlbum;
    private bool _useUnavailable;
    private bool _unavailable;
    private ObservableCollection<NodeFilter> _nodeFilters;
    private string _sizeOp;

    public FilterControlModel()
    {
        PropertyChanged += FilterControlModel_PropertyChanged;
        NodeFilters = new ObservableCollection<NodeFilter>();

        var nops = new List<NameValue<NodeOperation>>()
        {
            new() { Name = "or", Value = NodeOperation.UNION },
            new() { Name = "and", Value = NodeOperation.INTERSECT },
            new() { Name = "not", Value = NodeOperation.EXCEPT },
        };

        var comps = new List<NameValue<NodeComparison>>()
        {
            new() { Name = "contains", Value = NodeComparison.Contains },
            new() { Name = "equals", Value = NodeComparison.Equals },
            new() { Name = "starts with", Value = NodeComparison.StartsWith },
            new() { Name = "ends with", Value = NodeComparison.EndsWith },
        };

        NodeOperations = nops;
        NodePropertyComparisons = comps;
        SizeOp = "pixels";

        AddNodeFilter();
    }

    private void NodeFiltersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            RegisterNodeFilters(e.NewItems.Cast<NodeFilter>());
        }
    }

    private void RegisterNodeFilters(IEnumerable<NodeFilter> filters)
    {
        foreach (var nodeFilter in filters)
        {
            nodeFilter.IsFirst = NodeFilters.IndexOf(nodeFilter) == 0;
            nodeFilter.RemoveCommand = new RelayCommand<NodeFilter>(RemoveNodeFilter);
            nodeFilter.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(NodeFilter.IsActive))
                {
                    OnPropertyChanged(nameof(IsActive));
                }
            };
        }
    }

    private void NodeFilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    private void FilterControlModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IsActive))
        {
            OnPropertyChanged(nameof(IsActive));
        }
    }

    public void AddNodeFilter()
    {
        var filter = new NodeFilter
        {
            RemoveCommand = new RelayCommand<NodeFilter>(RemoveNodeFilter),
            Operation = NodeOperation.UNION,
            Comparison = NodeComparison.Contains
        };
        
        NodeFilters.Add(filter);
    }

    public void AddNodeFilter(string node, string property, string value)
    {
        var filter = new NodeFilter
        {
            Node = node,
            Property = property,
            Value = value,
            Operation = NodeOperation.UNION,
            Comparison = NodeComparison.Contains

        };

        NodeFilters.Add(filter);
    }

    public void AddNodeFilter(string property)
    {
        var filter = new NodeFilter
        {
            Property = property,
            Operation = NodeOperation.UNION,
            Comparison = NodeComparison.Contains
        };

        NodeFilters.Add(filter);
    }

    public void AddNodeFilterEquals(string property, string value)
    {
        var filter = new NodeFilter
        {
            Property = property,
            Value = value,
            Operation = NodeOperation.UNION,
            Comparison = NodeComparison.Equals
        };

        NodeFilters.Add(filter);
    }

    public void RemoveNodeFilter(NodeFilter filter)
    {
        NodeFilters.Remove(filter);
    }

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

    public bool UsePromptEx
    {
        get => _usePromptEx;
        set => SetField(ref _usePromptEx, value);
    }

    public string PromptEx
    {
        get => _promptEx;
        set => SetField(ref _promptEx, value);
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

    public bool UseNegativePromptEx
    {
        get => _useNegativePromptEx;
        set => SetField(ref _useNegativePromptEx, value);
    }

    public string NegativePromptEx
    {
        get => _negativePromptEx;
        set => SetField(ref _negativePromptEx, value);
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

    public string? SeedStart
    {
        get => _seedStart;
        set => SetField(ref _seedStart, value);
    }

    public string? SeedEnd
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

    public string SizeOp
    {
        get => _sizeOp;
        set => SetField(ref _sizeOp, value);
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

    public bool NoAestheticScore
    {
        get => _noAestheticScore;
        set => SetField(ref _noAestheticScore, value);
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

    public bool UseInAlbum
    {
        get => _useInAlbum;
        set => SetField(ref _useInAlbum, value);
    }

    public bool InAlbum
    {
        get => _inAlbum;
        set => SetField(ref _inAlbum, value);
    }

    public bool UseUnavailable
    {
        get => _useUnavailable;
        set => SetField(ref _useUnavailable, value);
    }

    public bool Unavailable
    {
        get => _unavailable;
        set => SetField(ref _unavailable, value);
    }

    public bool IsActive => (UsePrompt ||
                             UsePromptEx ||
                             UseNegativePrompt ||
                             UseNegativePromptEx ||
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
                             UseInAlbum ||
                             UseUnavailable ||
                             NodeFilters.Any(d => d.IsActive));

    public ObservableCollection<NodeFilter> NodeFilters
    {
        get => _nodeFilters;
        set
        {
            SetField(ref _nodeFilters, value);
            RegisterNodeFilters(_nodeFilters);
            _nodeFilters.CollectionChanged += NodeFiltersOnCollectionChanged;
        }
    }

    public void Clear()
    {
        Prompt = String.Empty;
        PromptEx = String.Empty;
        NegativePrompt = String.Empty;
        NegativePromptEx = String.Empty;
        Steps = String.Empty;
        Sampler = String.Empty;
        SeedStart = null;
        SeedEnd = null;
        CFGScale = String.Empty;
        Width = String.Empty;
        Height = String.Empty;
        SizeOp = "pixels";
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
        NoAestheticScore = false;
        AestheticScoreOp = String.Empty;
        AestheticScore = null;
        Path = String.Empty;
        Start = null;
        End = null;
        HyperNet = String.Empty;
        HyperNetStr = 0;
        NoMetadata = false;
        InAlbum = false;

        UsePrompt = false;
        UsePromptEx = false;
        UseNegativePrompt = false;
        UseNegativePromptEx = false;
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
        UseInAlbum = false;
        UseUnavailable = false;
        foreach (var nodeFilter in NodeFilters)
        {
            nodeFilter.IsActive = false;
        }
    }


    public IEnumerable<NameValue<NodeOperation>>? NodeOperations { get; set; }
    public IEnumerable<NameValue<NodeComparison>>? NodePropertyComparisons { get; set; }

}

public class NameValue<T>
{
    public string Name { get; set; }
    public T Value { get; set; }
}

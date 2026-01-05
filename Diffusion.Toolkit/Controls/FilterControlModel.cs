using Diffusion.Common.Query;
using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Diffusion.Common;

namespace Diffusion.Toolkit.Controls;

public class CheckBoxItem : BaseNotify
{
    public string PropertyName { get; set; }

    public bool IsChecked
    {
        get;
        set => SetField(ref field, value);
    }

    public string Name
    {
        get;
        set => SetField(ref field, value);
    }

    public object Value
    {
        get;
        set;
    }
}

public class FilterControlModel : BaseNotify
{
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

        Types = new ObservableCollection<CheckBoxItem>()
        {
            new CheckBoxItem() { PropertyName = "Types", Name = GetLocalizedText("Filter.Metadata.Types.Image"), Value = ImageType.Image },
            new CheckBoxItem() { PropertyName = "Types", Name = GetLocalizedText("Filter.Metadata.Types.Video"), Value = ImageType.Video },
        };

        AddNodeFilter();
    }

    private string GetLocalizedText(string key)
    {
        return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
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
        get;
        set => SetField(ref field, value);
    }

    public string Prompt
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UsePromptEx
    {
        get;
        set => SetField(ref field, value);
    }

    public string PromptEx
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseNegativePrompt
    {
        get;
        set => SetField(ref field, value);
    }

    public string NegativePrompt
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseNegativePromptEx
    {
        get;
        set => SetField(ref field, value);
    }

    public string NegativePromptEx
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseSteps
    {
        get;
        set => SetField(ref field, value);
    }

    public string Steps
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseSampler
    {
        get;
        set => SetField(ref field, value);
    }

    public string Sampler
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseSeed
    {
        get;
        set => SetField(ref field, value);
    }

    public string? SeedStart
    {
        get;
        set => SetField(ref field, value);
    }

    public string? SeedEnd
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseCFGScale
    {
        get;
        set => SetField(ref field, value);
    }

    public string CFGScale
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseSize
    {
        get;
        set => SetField(ref field, value);
    }

    public string SizeOp
    {
        get;
        set => SetField(ref field, value);
    }

    public string Width
    {
        get;
        set => SetField(ref field, value);
    }

    public string Height
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseModelHash
    {
        get;
        set => SetField(ref field, value);
    }

    public string ModelHash
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseModelName
    {
        get;
        set => SetField(ref field, value);
    }

    public string ModelName
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseFavorite
    {
        get;
        set => SetField(ref field, value);
    }

    public bool Favorite
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseRating
    {
        get;
        set => SetField(ref field, value);
    }

    public string RatingOp
    {
        get;
        set => SetField(ref field, value);
    }

    public int? Rating
    {
        get;
        set => SetField(ref field, value);
    }

    public bool Unrated
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseTypes
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<CheckBoxItem> Types
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseNSFW
    {
        get;
        set => SetField(ref field, value);
    }

    public bool NSFW
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseForDeletion
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ForDeletion
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseBatchSize
    {
        get;
        set => SetField(ref field, value);
    }

    public int BatchSize
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseBatchPos
    {
        get;
        set => SetField(ref field, value);
    }

    public int BatchPos
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseAestheticScore
    {
        get;
        set => SetField(ref field, value);
    }

    public bool NoAestheticScore
    {
        get;
        set => SetField(ref field, value);
    }

    public string AestheticScoreOp
    {
        get;
        set => SetField(ref field, value);
    }

    public double? AestheticScore
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UsePath
    {
        get;
        set => SetField(ref field, value);
    }

    public string Path
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseCreationDate
    {
        get;
        set => SetField(ref field, value);
    }

    public DateTime? Start
    {
        get;
        set => SetField(ref field, value);
    }

    public DateTime? End
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseHyperNet
    {
        get;
        set => SetField(ref field, value);
    }

    public string HyperNet
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseHyperNetStr
    {
        get;
        set => SetField(ref field, value);
    }

    public string HyperNetStrOp
    {
        get;
        set => SetField(ref field, value);
    }

    public double HyperNetStr
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseNoMetadata
    {
        get;
        set => SetField(ref field, value);
    }

    public bool NoMetadata
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseInAlbum
    {
        get;
        set => SetField(ref field, value);
    }

    public bool InAlbum
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseUnavailable
    {
        get;
        set => SetField(ref field, value);
    }

    public bool Unavailable
    {
        get;
        set => SetField(ref field, value);
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
                             UseTypes ||
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
        get;
        set
        {
            SetField(ref field, value);
            RegisterNodeFilters(field);
            field.CollectionChanged += NodeFiltersOnCollectionChanged;
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
        foreach (var type in Types)
        {
            type.IsChecked = false;
        }
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
        UseTypes = false;
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

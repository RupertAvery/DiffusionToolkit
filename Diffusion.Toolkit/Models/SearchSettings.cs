using System;
using System.Collections.Generic;
using System.Linq;

namespace Diffusion.Toolkit.Models;

public class SearchSettings : BaseNotify
{
    private string _includeNodeProperties;
    private bool _searchNodes;
    private bool _searchRawData;
    private bool _searchAllProperties;
    private bool _searchNoProperties;

    public string IncludeNodeProperties
    {
        get => _includeNodeProperties;
        set => SetField(ref _includeNodeProperties, value);
    }

    public IEnumerable<string> GetNodePropertiesList()
    {
        return IncludeNodeProperties.Split(new[] { "\n", "\r\n", "," }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public void AddDefaultSearchProperty(string property)
    {
        var properties = IncludeNodeProperties.Split(new[] { "\n", "\r\n", "," }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (!properties.Contains(property.Trim().ToLower()))
        {
            IncludeNodeProperties = string.Join("\n",properties.Append(property));
        }
    }

    public bool SearchRawData

    {
        get => _searchRawData;
        set => SetField(ref _searchRawData, value);
    }

    public bool SearchNoProperties
    {
        get => _searchNoProperties;
        set => SetField(ref _searchNoProperties, value);
    }

    public bool SearchAllProperties
    {
        get => _searchAllProperties;
        set => SetField(ref _searchAllProperties, value);
    }

    public bool SearchNodes
    {
        get => _searchNodes;
        set => SetField(ref _searchNodes, value);
    }
}
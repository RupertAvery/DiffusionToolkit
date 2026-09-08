using System;
using System.Collections.Generic;
using System.Linq;

namespace Diffusion.Toolkit.Models;

public class SearchSettings : BaseNotify
{
    public string IncludeNodeProperties
    {
        get;
        set => SetField(ref field, value);
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
        get;
        set => SetField(ref field, value);
    }

    public bool SearchNoProperties
    {
        get;
        set => SetField(ref field, value);
    }

    public bool SearchAllProperties
    {
        get;
        set => SetField(ref field, value);
    }

    public bool SearchNodes
    {
        get;
        set => SetField(ref field, value);
    }
}
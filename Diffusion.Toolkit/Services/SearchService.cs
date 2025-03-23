using System;
using Diffusion.Database;
using Diffusion.Toolkit.Controls;

namespace Diffusion.Toolkit.Services;

public class SearchService
{
    public event EventHandler<string> SortBy;
    public event EventHandler<string> SortOrder;
    public event EventHandler<SearchFilter> SearchFilter;
    public event EventHandler Search;
    public event EventHandler<SearchView> View;

    public void SetSortBy(string value)
    {
        SortBy?.Invoke(this, value);
    }

    public void SetSortOrder(string value)
    {
        SortOrder?.Invoke(this, value);
    }

    public void SetFilter(SearchFilter value)
    {
        SearchFilter?.Invoke(this, value);
    }

    public SearchService(FilterControlModel filter)
    {
        Filter = filter;
    }

    public FilterControlModel Filter
    {
        get;
        set;
    }

    public void ExecuteSearch()
    {
        Search?.Invoke(this, EventArgs.Empty);
    }

    public void SetView(SearchView view)
    {
        View?.Invoke(this, view);
    }

    public void AddNodeFilter(string property, string value)
    {
        Filter.AddNodeFilter(property, value);
    }
}
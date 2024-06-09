using System;

namespace DiffusionToolkit.AvaloniaApp.Common;

public class SearchManager
{
    public event EventHandler<string> SortBy;
    public event EventHandler<string> SortOrder;
    public event EventHandler<SearchFilter> Filter;
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
        Filter?.Invoke(this, value);
    }

    public void ExecuteSearch()
    {
        Search?.Invoke(this, EventArgs.Empty);
    }

    public void SetView(SearchView view)
    {
        View?.Invoke(this, view);
    }
}

public enum SearchView {
    Search,
    Folders,
    Albums,
    RecycleBin,
}

public class SearchFilter
{
    public string? Query { get; set; }
}
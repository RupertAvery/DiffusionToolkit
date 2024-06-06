using System;

namespace DiffusionToolkit.AvaloniaApp.Common;

public class SearchManager
{
    public event EventHandler<string> SortBy;
    public event EventHandler<string> SortOrder;

    public void SetSortBy(string value)
    {
        SortBy?.Invoke(this, value);
    }

    public void SetSortOrder(string value)
    {
        SortOrder?.Invoke(this, value);
    }
}
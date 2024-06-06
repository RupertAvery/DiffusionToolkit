using System;

namespace DiffusionToolkit.AvaloniaApp.Common;

public class ThumbnailNavigationManager
{
    public event EventHandler Previous;
    public event EventHandler Next;
    public event EventHandler PreviousPage;
    public event EventHandler NextPage;


    public void MovePrevious()
    {
        Previous?.Invoke(this, EventArgs.Empty);
    }
    
    public void MoveNext()
    {
        Next?.Invoke(this, EventArgs.Empty);
    }

    public void MovePreviousPage()
    {

    }

    public void MoveNextPage()
    {

    }

}
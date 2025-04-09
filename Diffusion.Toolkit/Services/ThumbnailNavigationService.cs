using System;
using System.Diagnostics;

namespace Diffusion.Toolkit.Services;

public class ThumbnailNavigationService
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
        //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
        //Debug.WriteLine(t);
        Next?.Invoke(this, EventArgs.Empty);
    }

    public void MovePreviousPage()
    {

    }

    public void MoveNextPage()
    {

    }

}
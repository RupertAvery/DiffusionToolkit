using System;

namespace Diffusion.Toolkit.Models;

public class QueryModel : BaseNotify
{
    public int Id { get; set; }

    public string Name
    {
        get;
        set => SetField(ref field, value);
    }

    public DateTime CreatedDate { get; set; }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }
}
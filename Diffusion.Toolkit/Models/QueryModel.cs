using System;

namespace Diffusion.Toolkit.Models;

public class QueryModel : BaseNotify
{
    private bool _isSelected;
    private string _name;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public DateTime CreatedDate { get; set; }
    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }
}
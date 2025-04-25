using System.Windows.Input;
using Diffusion.Common.Query;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls;

public class NodeFilter : BaseNotify
{
    private string _node;
    private string _property;
    private string _value;
    private bool _isActive;
    private NodeOperation _operation;
    private NodeComparison _comparison;
    private bool _isFirst;
    private NameValue<NodeComparison> _selectedComparison;

    public bool IsActive
    {
        get => _isActive;
        set => SetField(ref _isActive, value);
    }

    public NodeOperation Operation
    {
        get => _operation;
        set => SetField(ref _operation, value);
    }

    public string Node
    {
        get => _node;
        set => SetField(ref _node, value);
    }

    public string Property
    {
        get => _property;
        set => SetField(ref _property, value);
    }


    public NameValue<NodeComparison> SelectedComparison
    {
        get => _selectedComparison;
        set => SetField(ref _selectedComparison, value);

    }

    public NodeComparison Comparison
    {
        get => _comparison;
        set => SetField(ref _comparison, value);
    }

    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    public ICommand RemoveCommand { get; set; }

    public bool IsFirst
    {
        get => _isFirst;
        set => SetField(ref _isFirst, value);
    }
}
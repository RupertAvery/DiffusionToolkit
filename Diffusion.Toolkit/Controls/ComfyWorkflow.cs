using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Diffusion.ComfyUI;
using Diffusion.IO;

namespace Diffusion.Toolkit.Controls;

public partial class ComfyWorkflow : UserControl
{
    private StackPanel _workflow;
    private Dictionary<int, ComfyNode> _children;

    public ComfyWorkflow()
    {
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is IReadOnlyCollection<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if(_children.TryGetValue(node.GetHashCode(), out var comfyNode))
                {
                    comfyNode.DataContext = node;
                }
            }
        }
    }

    public void BuildWorkflow(IReadOnlyCollection<Node> nodes)
    {
        var workflow  = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Background = new SolidColorBrush(Colors.Transparent),
            Focusable = true
        };

        _children = new Dictionary<int, ComfyNode>();

        foreach (var node in nodes)
        {
            var control = new ComfyNode();
            control.BuildNode(node);
            workflow.Children.Add(control);
            _children.Add(node.GetHashCode(), control);
        }

        AddChild(workflow);
    }
}
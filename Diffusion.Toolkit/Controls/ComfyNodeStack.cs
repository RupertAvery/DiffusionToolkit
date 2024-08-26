using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Diffusion.IO;

namespace Diffusion.Toolkit.Controls;
public partial class ComfyNodeStack : UserControl
{
    private int id;
    private Dictionary<int, ComfyWorkflow> _workflows = new Dictionary<int, ComfyWorkflow>();
    ContentControl _contentControl;

    public ComfyNodeStack()
    {
        Background = new SolidColorBrush(Colors.Transparent);
        DataContextChanged += OnDataContextChanged;
        _contentControl = new ContentControl()
        {
            Background = new SolidColorBrush(Colors.Transparent)
        };
        AddChild(_contentControl);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is IReadOnlyCollection<Node> nodes)
        {
            var hash = 0;

            foreach (var node in nodes)
            {
                hash = (hash * 397) ^ node.GetHashCode();
            }

            if (!_workflows.TryGetValue(hash, out var workflow))
            {
                workflow = new ComfyWorkflow();
                workflow.BuildWorkflow(nodes);
                workflow.DataContext = nodes;
                _workflows.Add(hash, workflow);
            }
            else
            {
                workflow.DataContext = nodes;
                _contentControl.Content = workflow;
            }

        }
        else
        {
            _contentControl.Content = null;
        }
    }
}
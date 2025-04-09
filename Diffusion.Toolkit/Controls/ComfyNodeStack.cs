using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Diffusion.IO;

namespace Diffusion.Toolkit.Controls;
public partial class ComfyNodeStack : UserControl
{
    public static readonly DependencyProperty NodesProperty = DependencyProperty.Register(
        nameof(Nodes),
        typeof(IReadOnlyCollection<Node>),
        typeof(ComfyNodeStack),
        new PropertyMetadata(null, PropertyChangedCallback)
    );

    readonly Dictionary<int, ComfyWorkflow> _workflows = new Dictionary<int, ComfyWorkflow>();

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(Nodes))
        {
            var instance = (ComfyNodeStack)d;

            if (e.NewValue is IReadOnlyCollection<Node> nodes)
            {
                var hash = 0;

                foreach (var node in nodes)
                {
                    hash = (hash * 397) ^ node.GetHashCode();
                }

                if (!instance._workflows.TryGetValue(hash, out var workflow))
                {
                    workflow = new ComfyWorkflow();
                    workflow.BuildWorkflow(nodes);
                    instance._workflows.Add(hash, workflow);
                }

                workflow.DataContext = nodes;
                instance._contentControl.Content = workflow;
            }
            else
            {
                instance._contentControl.Content = null;
            }
        }
    }

        
    public IReadOnlyCollection<Node> Nodes
    {
        get => (IReadOnlyCollection<Node>)GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }


    private int id;
    ContentControl _contentControl;

    public ComfyNodeStack()
    {
        Background = new SolidColorBrush(Colors.Transparent);

        _contentControl = new ContentControl()
        {
            Background = new SolidColorBrush(Colors.Transparent)
        };

        AddChild(_contentControl);
    }

}
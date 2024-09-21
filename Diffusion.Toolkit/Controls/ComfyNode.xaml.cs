using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Diffusion.IO;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for ComfyNode.xaml
    /// </summary>
    public partial class ComfyNode : UserControl
    {
        public int Id { get; private set; }

        private Dictionary<string, TextBox>? _textBoxes;

        public ComfyNode()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        public void BuildNode(Node node)
        {
            _textBoxes = new Dictionary<string, TextBox>();

            foreach (var input in node.Inputs)
            {
                var label = new Label()
                {
                    DataContext = input,
                    FontSize = 14,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(5),
                };
                var textBox = new TextBox()
                {
                    DataContext = input,
                    FontSize = 14,
                    BorderThickness = new Thickness(1),
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    IsReadOnly = true,
                    Padding = new Thickness(5),
                    MaxHeight = 200,
                    Text = input.Value?.ToString()
                };
                var button = new Button()
                {
                    DataContext = input,
                    FontSize = 12,
                    Width = 16,
                    Height = 16,
                    Content = "...",
                };
                label.SetBinding(Label.ContentProperty, "Name");
                textBox.SetValue(Grid.ColumnProperty, 1);
                textBox.SetBinding(TextBox.TextProperty, "Value");
                button.SetValue(Grid.ColumnProperty, 2);
                button.Click += ButtonOnClick;

                var grid = new Grid()
                {
                    DataContext = node,
                    Margin = new Thickness(0, 2, 0, 2),
                    Background = new SolidColorBrush(Colors.Transparent),
                    ColumnDefinitions =
                    {
                        new ColumnDefinition()
                        {
                            Width = new GridLength(100)
                        },
                        new ColumnDefinition(),
                        new ColumnDefinition()
                        {
                            Width = new GridLength(16)
                        },
                    },
                    Children = {
                        label,
                        textBox,
                        button
                    }
                };

                _textBoxes.Add(input.Name, textBox);

                InputsPanel.Children.Add(grid);
            }

            Id = node.GetHashCode();
        }

        private void ButtonOnClick(object sender, RoutedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Node node)
            {
                var hash = node.GetHashCode();

                if (hash != Id)
                {
                    //BuildNode(node);
                }
                else
                {
                    if (_textBoxes != null)
                    {
                        foreach (var input in node.Inputs)
                        {
                            if(_textBoxes.TryGetValue(input.Name, out var textBox))
                            {
                                textBox.DataContext = input;
                            }
                        }
                    }
                }

            }
        }
    }
}

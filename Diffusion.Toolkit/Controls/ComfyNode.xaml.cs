using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Diffusion.ComfyUI;
using Diffusion.IO;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for ComfyNode.xaml
    /// </summary>
    public partial class ComfyNode : UserControl
    {
        private string GetLocalizedText(string key)
        {
            return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
        }

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

            if (node.Inputs != null)
            {

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
                        ContextMenu = new ContextMenu()
                        {

                        }
                    };
                    label.SetBinding(Label.ContentProperty, "Label");
                    textBox.SetValue(Grid.ColumnProperty, 1);
                    textBox.SetBinding(TextBox.TextProperty, "Value");
                    button.SetValue(Grid.ColumnProperty, 2);
                    button.Click += ButtonOnClick;


                    var copyNameMenuItem = new MenuItem() { Header = GetLocalizedText("Metadata.Workflow.ContextMenu.CopyPropertyName") };
                    copyNameMenuItem.DataContext = input;
                    copyNameMenuItem.Click += (sender, args) =>
                    {
                        var bindingExpression = label.GetBindingExpression(Label.ContentProperty);
                        var boundInput = (Input)bindingExpression.ResolvedSource;
                        Clipboard.SetText(boundInput.Name);
                    };

                    var copyValueMenuItem = new MenuItem() { Header = GetLocalizedText("Metadata.Workflow.ContextMenu.CopyPropertyValue") };
                    copyValueMenuItem.DataContext = input;
                    copyValueMenuItem.Click += (sender, args) =>
                    {
                        var bindingExpression = label.GetBindingExpression(Label.ContentProperty);
                        var boundInput = (Input)bindingExpression.ResolvedSource;
                        Clipboard.SetText(boundInput.Value.ToString());
                    };

                    var filterMenuItem = new MenuItem() { Header = GetLocalizedText("Metadata.Workflow.ContextMenu.AddToFilters") };
                    filterMenuItem.DataContext = input;
                    filterMenuItem.Click += (sender, args) =>
                    {
                        var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                        var boundInput = (Input)bindingExpression.ResolvedSource;
                        ServiceLocator.SearchService.AddNodeFilter(boundInput.Name, (string)boundInput.Value);
                    };

                    var searchMenuItem = new MenuItem() { Header = GetLocalizedText("Metadata.Workflow.ContextMenu.AddToDefaultSearch") };
                    searchMenuItem.DataContext = input;
                    searchMenuItem.Click += (sender, args) =>
                    {
                        var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                        var boundInput = (Input)bindingExpression.ResolvedSource;
                        ServiceLocator.SearchService.AddDefaultSearchProperty(boundInput.Name);
                    };

                    button.ContextMenu.Items.Add(copyNameMenuItem);
                    button.ContextMenu.Items.Add(copyValueMenuItem);
                    button.ContextMenu.Items.Add(new Separator());
                    button.ContextMenu.Items.Add(filterMenuItem);
                    button.ContextMenu.Items.Add(searchMenuItem);


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

        }



        private void ButtonOnClick(object sender, RoutedEventArgs e)
        {
            ((Button)sender).ContextMenu.IsOpen = true;
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
                            if (_textBoxes.TryGetValue(input.Name, out var textBox))
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

using Diffusion.Common;
using Diffusion.Common.Query;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for MetadataPanel.xaml
    /// </summary>
    public partial class MetadataPanel : UserControl
    {
        public static readonly DependencyProperty CurrentImageProperty = DependencyProperty.Register(
            nameof(CurrentImage),
            typeof(ImageViewModel),
            typeof(MetadataPanel),
            new PropertyMetadata(default(ImageEntry), PropertyChangedCallback)
        );

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MetadataPanel panel)
            {
                panel.CurrentImage.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(ImageViewModel.ImageTags))
                    {
                        panel.UpdateFilter();
                    }
                };
                panel.UpdateFilter();
            }
        }

        public ImageViewModel CurrentImage
        {
            get => (ImageViewModel)GetValue(CurrentImageProperty);
            set => SetValue(CurrentImageProperty, value);
        }

        public static readonly DependencyProperty MetadataSectionProperty = DependencyProperty.Register(
            nameof(MetadataSection),
            typeof(MetadataSection),
            typeof(MetadataPanel),
            new PropertyMetadata(default(ImageEntry))
        );

        public MetadataSection MetadataSection
        {
            get => (MetadataSection)GetValue(MetadataSectionProperty);
            set => SetValue(MetadataSectionProperty, value);
        }

        public MetadataPanel()
        {
            InitializeComponent();
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            SetMetadataState(AccordionState.Collapsed);
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            SetMetadataState(AccordionState.Expanded);
        }

        private void SetMetadataState(AccordionState state)
        {
            PromptMetadata.State = state;
            NegativePromptMetadata.State = state;
            SeedMetadata.State = state;
            SamplerMetadata.State = state;
            OtherMetadata.State = state;
            ModelMetadata.State = state;
            PathMetadata.State = state;
            AlbumMetadata.State = state;
            DateMetadata.State = state;
        }

        private void AlbumName_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var album = ((Album)((TextBox)sender).DataContext);
            CurrentImage.OpenAlbumCommand?.Execute(album);
        }

        private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private void AddTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            AddTag();
        }

        private void AddTagText_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddTag();
            }
        }

        private void AddTag()
        {
            var tagName = AddTagText.Text.Trim();
            if (tagName.Length > 0)
            {
                ServiceLocator.DataStore.CreateTag(tagName);
                AddTagText.Text = "";
                CurrentImage.ImageTags = ServiceLocator.TagService.GetImageTagViews(CurrentImage.Id);
                ServiceLocator.TagService.LoadTags();
                UpdateFilter();
            }
        }

        private void UpdateFilter()
        {
            if (CurrentImage.ImageTags == null)
            {
                CurrentImage.FilteredTags = null;
                return;
            }
            if (TagFilter.Text is { Length: > 0 })
            {
                var filter = TagFilter.Text.ToLower().Trim();

                CurrentImage.FilteredTags = CurrentImage.ImageTags.Where(d => d.Name.ToLower().Contains(filter)).ToList();
            }
            else
            {
                CurrentImage.FilteredTags = CurrentImage.ImageTags.ToList();
            }
        }

        private void ClearFilter_OnClick(object sender, RoutedEventArgs e)
        {
            TagFilter.Text = "";
            UpdateFilter();
        }

        private void TagFilter_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilter();
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true; // Mark the event as handled to stop tunneling

            // Create a new MouseWheelEventArgs for the bubbling event
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };

            // Raise the new bubbling event on the ListView itself
            if (VisualTreeHelper.GetParent((FrameworkElement)sender) is UIElement parent)
            {
                parent.RaiseEvent(eventArg);
            }
        }

        private void TagFilter_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                TagFilter.Text = "";
                UpdateFilter();
            }
        }
    }
}

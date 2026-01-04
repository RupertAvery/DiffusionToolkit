using Diffusion.Toolkit.Models;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Common;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Services;

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
            new PropertyMetadata(default(ImageEntry))
        );

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
            var tagName = AddTagText.Text.Trim();
            if (tagName.Length > 0)
            {
                ServiceLocator.DataStore.CreateTag(tagName);
                AddTagText.Text = "";
                CurrentImage.ImageTags = ServiceLocator.TagService.GetImageTagViews(CurrentImage.Id);
            }
        }
    }
}

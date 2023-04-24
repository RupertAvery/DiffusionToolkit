using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for PreviewPane.xaml
    /// </summary>
    public partial class PreviewPane : UserControl
    {
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(
                name: nameof(Image),
                propertyType: typeof(ImageViewModel),
                ownerType: typeof(PreviewPane),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: default(ImageViewModel),
                    propertyChangedCallback: PropertyChangedCallback)
            );


        public static readonly DependencyProperty NSFWBlurProperty =
            DependencyProperty.Register(
                name: nameof(NSFWBlur),
                propertyType: typeof(bool),
                ownerType: typeof(PreviewPane),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: false,
                    propertyChangedCallback: PropertyChangedCallback)
            );


        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(Image))
            {

            }
        }

        public bool NSFWBlur
        {
            get => (bool)GetValue(NSFWBlurProperty);
            set => SetValue(NSFWBlurProperty, value);
        }

        public ImageViewModel Image
        {
            get => (ImageViewModel)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }


        private ScrollDragger _scrollDragger;

        public PreviewPane()
        {
            InitializeComponent();

            _scrollDragger = new ScrollDragger(Preview, ScrollViewer);
        }


        public void ResetZoom()
        {
            Preview.LayoutTransform = new ScaleTransform(1, 1);

            ScrollViewer.ScrollToHorizontalOffset(0);
            ScrollViewer.ScrollToVerticalOffset(0);
            this.UpdateLayout();
        }

        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Point mouseAtImage = e.GetPosition(Preview); // ScrollViewer_CanvasMain.TranslatePoint(middleOfScrollViewer, Canvas_Main);
                Point mouseAtScrollViewer = e.GetPosition(ScrollViewer);

                // Calculate the new zoom level based on the mouse wheel delta
                double zoomDelta = e.Delta > 0 ? 0.1 : -0.1;
                
                zoomDelta = Preview.LayoutTransform.Value.M11 * zoomDelta;

                double newZoom = Math.Min(Math.Max(Preview.LayoutTransform.Value.M11 + zoomDelta, 0.1), 10);


                Preview.LayoutTransform = new ScaleTransform(newZoom, newZoom);

                ScrollViewer.ScrollToHorizontalOffset(0);
                ScrollViewer.ScrollToVerticalOffset(0);
                this.UpdateLayout();

                Vector offset = Preview.TranslatePoint(mouseAtImage, ScrollViewer) - mouseAtScrollViewer; // (Vector)middleOfScrollViewer;
                ScrollViewer.ScrollToHorizontalOffset(offset.X);
                ScrollViewer.ScrollToVerticalOffset(offset.Y);
                this.UpdateLayout();

                e.Handled = true;
            }
            else
            {
                Key vkey = e.Delta > 0 ? Key.Left : e.Delta < 0 ? Key.Right : Key.None;

                var ps = PresentationSource.FromVisual((ScrollViewer)sender);

                if (vkey == Key.None)
                {
                    OnPreviewKeyUp(new KeyEventArgs(null, ps, 0, vkey));
                }
                else
                {
                    OnPreviewKeyDown(new KeyEventArgs(null, ps, 0, vkey));
                }

            }
        }
        
        public void ZoomPreview(double zoomDelta)
        {
            var mouseAtScrollViewer = new Point(ScrollViewer.ViewportWidth / 2, ScrollViewer.ViewportHeight / 2);
            Point mouseAtImage = ScrollViewer.TranslatePoint(mouseAtScrollViewer, Preview);

            zoomDelta = Preview.LayoutTransform.Value.M11 * zoomDelta;

            double newZoom = Math.Min(Math.Max(Preview.LayoutTransform.Value.M11 + zoomDelta, 0.1), 10);

            Preview.LayoutTransform = new ScaleTransform(newZoom, newZoom);

            ScrollViewer.ScrollToHorizontalOffset(0);
            ScrollViewer.ScrollToVerticalOffset(0);
            this.UpdateLayout();

            Vector offset = Preview.TranslatePoint(mouseAtImage, ScrollViewer) - mouseAtScrollViewer;
            ScrollViewer.ScrollToHorizontalOffset(offset.X);
            ScrollViewer.ScrollToVerticalOffset(offset.Y);
            this.UpdateLayout();
        }

        private static Key[] _ratings = new[]
        {
            Key.D1,
            Key.D2,
            Key.D3,
            Key.D4,
            Key.D5,
            Key.D6,
            Key.D7,
            Key.D8,
            Key.D9,
            Key.D0,
        };

        public Action<int, bool> NSFW { get; set; }
        public Action<int, bool> Favorite { get; set; }
        public Action<int, int?> Rate { get; set; }
        public Action<int, bool> Delete { get; set; }

        private void ScrollViewer_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.X or Key.Delete && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                Image.ForDeletion = !Image.ForDeletion;
                Delete?.Invoke(Image.Id, Image.ForDeletion);
            }
            if (e.Key == Key.N && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                Image.NSFW = !Image.NSFW;
                NSFW?.Invoke(Image.Id, Image.NSFW);
            }
            if (e.Key == Key.F && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                Image.Favorite = !Image.Favorite;
                Favorite?.Invoke(Image.Id, Image.Favorite);
            }
            if (e.Key == Key.I && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                Image.IsParametersVisible = !Image.IsParametersVisible;
            }
            //if (e.Key == Key.F && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            //{
            //    MainModel.FitToPreview = !MainModel.FitToPreview;
            //}
            if (_ratings.Contains(e.Key) && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                int? rating = e.Key switch
                {
                    Key.D1 => 1,
                    Key.D2 => 2,
                    Key.D3 => 3,
                    Key.D4 => 4,
                    Key.D5 => 5,
                    Key.D6 => 6,
                    Key.D7 => 7,
                    Key.D8 => 8,
                    Key.D9 => 9,
                    Key.D0 => 10,
                };

                if (Image.Rating == rating)
                {
                    rating = null;
                }

                Image.Rating = rating;

                Rate?.Invoke(Image.Id, rating);
            }
            else if (e.Key == Key.D0 && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                ResetZoom();
                e.Handled = true;
            }
            else if (e.Key == Key.OemPlus && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                ZoomPreview(0.1);
                e.Handled = true;
            }
            if (e.Key == Key.OemMinus && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                ZoomPreview(-0.1);
                e.Handled = true;
            }
        }

        public void ToggleInfo()
        {
            Image.IsParametersVisible = !Image.IsParametersVisible;
        }

        private void Popout_OnClick(object sender, RoutedEventArgs e)
        {
            OnPopout?.Invoke();
        }

        public bool IsPopout { get; set; }

        public Action OnPopout { get; set; }
        //public Action OnNext { get; set; }
        //public Action OnPrev { get; set; }
        public MainModel MainModel { get; set; }

        private void ScrollViewer_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Left)
            //{
            //    OnPrev?.Invoke();
            //    e.Handled = true;
            //}

            //if (e.Key == Key.Right)
            //{
            //    OnNext?.Invoke();
            //    e.Handled = true;
            //}
            OnPreviewKeyDown(e);
        }

        private void PreviewPane_OnGotFocus(object sender, RoutedEventArgs e)
        {
            ScrollViewer.Focus();
        }

        private void ScrollViewer_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            OnPreviewKeyUp(e);
        }
    }
}
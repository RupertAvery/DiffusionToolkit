using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using Point = System.Windows.Point;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for PreviewPane.xaml
    /// </summary>
    public partial class PreviewPane : UserControl
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(
                name: nameof(IsLoading),
                propertyType: typeof(bool),
                ownerType: typeof(PreviewPane),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: false,
                    propertyChangedCallback: PropertyChangedCallback)
            );

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
                var preview = ((PreviewPane)d);
                preview.SetHandler((ImageViewModel)e.NewValue);

            }
        }

        private void ImageViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageViewModel.Image))
            {
                ResetView();
            }
        }

        private void SetHandler(ImageViewModel model)
        {
            model.PropertyChanged += ImageViewModelOnPropertyChanged;
        }

        private void ResetView()
        {
            if (ServiceLocator.MainModel.FitToPreview)
            {
                FitToPreview();
            }
            if (ServiceLocator.MainModel.ActualSize)
            {
                ActualSize();
            }
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public bool NSFWBlur
        {
            get => (bool)GetValue(NSFWBlurProperty);
            set => SetValue(NSFWBlurProperty, value);
        }

        public ImageViewModel? Image
        {
            get => (ImageViewModel)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }


        private ScrollDragger _scrollDragger;

        public PreviewPane()
        {
            InitializeComponent();
            InitIcons();
            _scrollDragger = new ScrollDragger(Preview, ScrollViewer, handCursor, grabCursor);
            SizeChanged += OnSizeChanged;

            if (ServiceLocator.MainModel != null)
            {
                ServiceLocator.MainModel.PropertyChanged += MainModelOnPropertyChanged;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ServiceLocator.MainModel is { FitToPreview: true })
            {
                FitToPreview();
            }

            if (ServiceLocator.MainModel is { ActualSize: true })
            {
                ActualSize();
            }
        }

        private void FitToPreview()
        {
            if (Image is { Image: { } })
            {

                var ratio = ActualWidth / ActualHeight;

                double factor;

                var hfactor = ActualHeight / Image.Image.Height;
                var vfactor = ActualWidth / Image.Image.Width;

                factor = Math.Min(hfactor, vfactor);

                Preview.LayoutTransform = new ScaleTransform(factor, factor);

                UpdateLayout();
            }
        }

        private void ActualSize()
        {
            if (Image is { Image: { } })
            {

                Preview.LayoutTransform = new ScaleTransform(1, 1);
                UpdateLayout();
            }
        }

        private void ResetScrollbars()
        {
            ScrollViewer.ScrollToHorizontalOffset(0);
            ScrollViewer.ScrollToVerticalOffset(0);
        }

        private Cursor handCursor;
        private Cursor grabCursor;


        public MainModel MainModel => ServiceLocator.MainModel;

        private void InitIcons()
        {
            try
            {
                Uri handIconUri = new Uri("pack://application:,,,/Icons/hand.cur", UriKind.RelativeOrAbsolute);
                handCursor = new Cursor(Application.GetResourceStream(handIconUri).Stream);
                Uri grabIconUri = new Uri("pack://application:,,,/Icons/grab.cur", UriKind.RelativeOrAbsolute);
                grabCursor = new Cursor(Application.GetResourceStream(grabIconUri).Stream);
                //Unloaded += OnUnloaded;
            }
            catch (Exception e)
            {
            }
        }


        //private void OnUnloaded(object sender, RoutedEventArgs e)
        //{
        //    handCursor.Dispose();
        //    grabCursor.Dispose();
        //}


        public void ResetZoom()
        {
            Preview.LayoutTransform = new ScaleTransform(1, 1);

            ScrollViewer.ScrollToHorizontalOffset(0);
            ScrollViewer.ScrollToVerticalOffset(0);
            this.UpdateLayout();
        }

        private void Zoom(MouseWheelEventArgs e)
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
        }

        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            var active = true;

            var scrollNavigation = ServiceLocator.Settings.ScrollNavigation;



            if (ctrlPressed)
            {
                if (scrollNavigation)
                {
                    ServiceLocator.MainModel.FitToPreview = false;
                    ServiceLocator.MainModel.ActualSize = false;

                    Zoom(e);
                    e.Handled = true;
                }
            }
            else
            {
                if (scrollNavigation)
                {
                    Key vkey = e.Delta > 0 ? Key.Left : e.Delta < 0 ? Key.Right : Key.None;

                    Debug.WriteLine(e.Delta);

                    var ps = PresentationSource.FromVisual((ScrollViewer)sender);

                    switch (vkey)
                    {
                        case Key.Left:
                            ServiceLocator.ThumbnailNavigationService.MovePrevious();
                            break;
                        case Key.Right:
                            ServiceLocator.ThumbnailNavigationService.MoveNext();
                            break;
                    }

                    e.Handled = true;
                }
                else
                {
                    Zoom(e);
                    e.Handled = true;
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
            //if (e.Key == Key.I && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            //{
            //    Image.IsParametersVisible = !Image.IsParametersVisible;
            //}
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
            else if (e.Key == Key.OemPlus)
            {
                ZoomPreview(0.1);
                e.Handled = true;
            }
            if (e.Key == Key.OemMinus)
            {
                ZoomPreview(-0.1);
                e.Handled = true;
            }
        }

        public void ToggleInfo()
        {
            Image.IsParametersVisible = !Image.IsParametersVisible;
        }

        public bool IsPopout { get; set; }

        public Action OnPopout { get; set; }

        private void MainModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainModel.FitToPreview))
            {
                if (ServiceLocator.MainModel.FitToPreview)
                {
                    FitToPreview();
                }
            }
            if (e.PropertyName == nameof(MainModel.ActualSize))
            {
                if (ServiceLocator.MainModel.ActualSize)
                {
                    ActualSize();
                }
            }
        }

        private void ScrollViewer_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            OnPreviewKeyDown(e);
        }

        //private void PreviewPane_OnGotFocus(object sender, RoutedEventArgs e)
        //{
        //    ScrollViewer.Focus();
        //}

        public void SetFocus()
        {
            ScrollViewer.Focus();
        }

        private void ScrollViewer_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            OnPreviewKeyUp(e);
        }

        private void ScrollViewer_OnMouseMove(object sender, MouseEventArgs e)
        {
            Window window = Window.GetWindow(this);

            var ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (e.LeftButton == MouseButtonState.Pressed && !ctrlPressed)
            {
                if (ScrollViewer.ScrollableHeight == 0 && ScrollViewer.ScrollableWidth == 0)
                {
                    DataObject dataObject = new DataObject();
                    dataObject.SetData(DataFormats.FileDrop, new[] { Image.Path });
                    dataObject.SetData("DTCustomDragSource", true);

                    DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Move | DragDropEffects.Copy);

                    window.Cursor = null;

                    e.Handled = true;
                }
            }
        }


        //private void ScrollViewer_OnMouseLeave(object sender, MouseEventArgs e)
        //{
        //    ScrollViewer.Cursor = Cursors.Arrow;
        //}

        //private void ScrollViewer_OnMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    ScrollViewer.Cursor = grabCursor;
        //}

        //private void ScrollViewer_OnMouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    ScrollViewer.Cursor = handCursor;
        //}
    }
}
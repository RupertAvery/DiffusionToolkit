using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Behaviors;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Image = System.Drawing.Image;
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

        private int? originalRating;

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(Image))
            {
                var preview = ((PreviewPane)d);
                var model = (ImageViewModel)e.NewValue;
                preview.SetHandler(model);
                preview.originalRating = model.Rating;

                //if (model.Type == ImageType.Video)
                //{
                //    preview.Player.Source = new Uri(model.Path, UriKind.Absolute);
                //    preview.Player.Play();
                //}
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

        public ICommand CopyPathCommand { get; set; }


        private ScrollDragger? _scrollDragger = null;

        private System.Windows.Controls.Image? Preview = null;
        private System.Windows.Controls.ScrollViewer? ScrollViewer = null;
        private System.Windows.Controls.MediaElement? Player = null;

        public PreviewPane()
        {
            InitializeComponent();
            InitIcons();
            //_scrollDragger = new ScrollDragger(Preview, ScrollViewer, handCursor, grabCursor);
            SizeChanged += OnSizeChanged;


            CopyPathCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyPath);

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
            if (Image is { Image: { }, Type: ImageType.Image })
            {

                var ratio = ActualWidth / ActualHeight;

                double factor;

                var hfactor = ActualHeight / Image.Image.Height;
                var vfactor = ActualWidth / Image.Image.Width;

                factor = Math.Min(hfactor, vfactor);

                Preview?.LayoutTransform = new ScaleTransform(factor, factor);

                UpdateLayout();
            }
        }

        private void ActualSize()
        {
            if (Image is { Image: { }, Type: ImageType.Image })
            {
                Preview?.LayoutTransform = new ScaleTransform(1, 1);
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
        private Action<int, int?> _rate;


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
            if (Image is { Image: { }, Type: ImageType.Image })
            {
                Preview?.LayoutTransform = new ScaleTransform(1, 1);

                ScrollViewer?.ScrollToHorizontalOffset(0);
                ScrollViewer?.ScrollToVerticalOffset(0);
                this.UpdateLayout();
            }

        }

        private void Zoom(MouseWheelEventArgs e)
        {
            if (Image.Type == ImageType.Video) return;

            Point mouseAtImage = e.GetPosition(Preview); // ScrollViewer_CanvasMain.TranslatePoint(middleOfScrollViewer, Canvas_Main);
            Point mouseAtScrollViewer = e.GetPosition(ScrollViewer);

            // Calculate the new zoom level based on the mouse wheel delta
            double zoomDelta = e.Delta > 0 ? 0.1 : -0.1;

            zoomDelta = Preview.LayoutTransform.Value.M11 * zoomDelta;

            double newZoom = Math.Min(Math.Max(Preview.LayoutTransform.Value.M11 + zoomDelta, 0.1), 10);


            Preview.LayoutTransform = new ScaleTransform(newZoom, newZoom);

            ScrollViewer?.ScrollToHorizontalOffset(0);
            ScrollViewer?.ScrollToVerticalOffset(0);
            this.UpdateLayout();

            Vector offset = Preview.TranslatePoint(mouseAtImage, ScrollViewer) - mouseAtScrollViewer; // (Vector)middleOfScrollViewer;
            ScrollViewer?.ScrollToHorizontalOffset(offset.X);
            ScrollViewer?.ScrollToVerticalOffset(offset.Y);
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

                    //var ps = PresentationSource.FromVisual((ScrollViewer)sender);

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
            if (Image.Type == ImageType.Video) return;

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

        private void Grid_OnKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.I && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            //{
            //    Image.IsParametersVisible = !Image.IsParametersVisible;
            //}
            //if (e.Key == Key.F && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            //{
            //    MainModel.FitToPreview = !MainModel.FitToPreview;
            //}

            switch (e.Key)
            {
                case Key.X or Key.Delete when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    Image.ForDeletion = !Image.ForDeletion;
                    ServiceLocator.TaggingService.ForDeletion(this, Image.Id, Image.ForDeletion);
                    break;
                case Key.N when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    Image.NSFW = !Image.NSFW;
                    ServiceLocator.TaggingService.NSFW(this, Image.Id, Image.NSFW);
                    break;
                case Key.F when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    Image.Favorite = !Image.Favorite;
                    ServiceLocator.TaggingService.Favorite(this, Image.Id, Image.Favorite);
                    break;
                case Key.OemTilde:
                    Image.Rating = null;
                    ServiceLocator.TaggingService.Rate(this, Image.Id, null);
                    break;
                case >= Key.D0 and <= Key.D9 when e.KeyboardDevice.Modifiers == ModifierKeys.None:
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

                        ServiceLocator.TaggingService.Rate(this, Image.Id, rating);
                        break;
                    }
                case Key.D0 when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    ResetZoom();
                    e.Handled = true;
                    break;
                case Key.OemPlus:
                    ZoomPreview(0.1);
                    e.Handled = true;
                    break;
                case Key.OemMinus:
                    ZoomPreview(-0.1);
                    e.Handled = true;
                    break;
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

        private void Grid_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            OnPreviewKeyDown(e);
        }

        //private void PreviewPane_OnGotFocus(object sender, RoutedEventArgs e)
        //{
        //    ScrollViewer.Focus();
        //}

        public void SetFocus()
        {
            Grid.Focus();
        }

        private void Grid_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            OnPreviewKeyUp(e);
        }

        private void Grid_OnMouseMove(object sender, MouseEventArgs e)
        {
            Window window = Window.GetWindow(this);

            var ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (e.LeftButton == MouseButtonState.Pressed && !ctrlPressed)
            {
                var canDrag = Image.Type == ImageType.Image
                    ? ScrollViewer is { ScrollableHeight: 0, ScrollableWidth: 0 }
                    : true;

                if (canDrag)
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

        private void NavigatePrevious_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ServiceLocator.ThumbnailNavigationService.MovePrevious();
            e.Handled = true;
        }

        private void NavigateNext_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ServiceLocator.ThumbnailNavigationService.MoveNext();
            e.Handled = true;
        }

        private void Player_OnLoaded(object sender, RoutedEventArgs e)
        {
            _scrollDragger?.Close();

            ScrollViewer = null;
            Preview = null;
            _scrollDragger = null;

            Player = (MediaElement)sender;
        }

        private void Player_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            if (ServiceLocator.Settings.LoopVideo)
            {
                Player?.Position = TimeSpan.FromMilliseconds(1);
            }
        }

        private void ScrollViewer_OnLoaded(object sender, RoutedEventArgs e)
        {
            Player = null;

            ScrollViewer = (ScrollViewer)sender;
            Preview = FindVisualChildren<System.Windows.Controls.Image>(ScrollViewer).First();

            _scrollDragger = new ScrollDragger(Preview, ScrollViewer, handCursor, grabCursor);

            SetFocus();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Diffusion.Toolkit.Classes;
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
                    defaultValue: null,
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
        
        private double _zoomValue = 1.0;

        public PreviewPane()
        {
            InitializeComponent();

            _scrollDragger = new ScrollDragger(Preview, ScrollViewer);
        }


        public void ResetZoom()
        {
            _zoomValue = 1.0;
        }

        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0)
                {
                    _zoomValue += 0.1;
                }
                else
                {
                    _zoomValue -= 0.1;
                }


                ZoomPreview();

                e.Handled = true;
            }
        }
        


        public void ZoomPreview()
        {
            if (_zoomValue < 0.1)
            {
                _zoomValue = 0.1;
            }
            if (_zoomValue > 3)
            {
                _zoomValue = 3;
            }

            ScaleTransform scale = new ScaleTransform(_zoomValue, _zoomValue);
            Preview.LayoutTransform = scale;
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

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.X  || e.Key == Key.Delete) && e.KeyboardDevice.Modifiers == ModifierKeys.None)
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
                _zoomValue = 1;
                ZoomPreview();
                e.Handled = true;
            }
            if (e.Key == Key.OemPlus && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                _zoomValue += 0.1;
                ZoomPreview();
                e.Handled = true;
            }
            if (e.Key == Key.OemMinus && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                _zoomValue -= 0.1;
                ZoomPreview();
                e.Handled = true;
            }
        }

        public void ToggleInfo()
        {
            Image.IsParametersVisible = !Image.IsParametersVisible;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            OnPopout?.Invoke();
        }

        public bool IsPopout { get; set; }

        public Action OnPopout { get; set; }
        public Action OnNext { get; set; }
        public Action OnPrev { get; set; }

        private void ScrollViewer_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                OnPrev?.Invoke();
                e.Handled = true;
            }

            if (e.Key == Key.Right)
            {
                OnNext?.Invoke();
                e.Handled = true;
            }


        }
    }
}



public class ScrollDragger
{
    private readonly ScrollViewer _scrollViewer;
    private readonly UIElement _content;
    private readonly Cursor _dragCursor = Cursors.Hand;
    private double _scrollMouseX;
    private double _scrollMouseY;
    private int _updateCounter = 0;

    public ScrollDragger(UIElement content, ScrollViewer scrollViewer)
    {
        _scrollViewer = scrollViewer;
        _content = content;

        content.MouseLeftButtonDown += scrollViewer_MouseLeftButtonDown;
        content.PreviewMouseMove += scrollViewer_PreviewMouseMove;
        content.PreviewMouseLeftButtonUp += scrollViewer_PreviewMouseLeftButtonUp;
    }

    private void scrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Capture the mouse, reset counter, switch to hand cursor to indicate dragging
        _content.CaptureMouse();
        _updateCounter = 0;
        _scrollViewer.Cursor = _dragCursor;
    }

    private void scrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_content.IsMouseCaptured)
        {
            _updateCounter++;

            // Skip dragging on the first PreviewMouseMove event after the left mouse button goes down. It actually triggers two of these and this ignores both, preventing jumping.
            if (_updateCounter <= 1)
            {
                // Grab starting mouse offset relative to scroll viewer, used to calculate first delta
                _scrollMouseY = e.GetPosition(_scrollViewer).Y;
                _scrollMouseX = e.GetPosition(_scrollViewer).X;
                return;
            }

            // Calculate new vertical offset then scroll to it
            var newVOff = HandleMouseMoveAxisUpdateScroll(_scrollViewer.VerticalOffset, ref _scrollMouseY, e.GetPosition(_scrollViewer).Y, _scrollViewer.ScrollableHeight);
            _scrollViewer.ScrollToVerticalOffset(newVOff);

            // Calculate new horizontal offset and scroll to it
            var newHOff = HandleMouseMoveAxisUpdateScroll(_scrollViewer.HorizontalOffset, ref _scrollMouseX, e.GetPosition(_scrollViewer).X, _scrollViewer.ScrollableWidth);
            _scrollViewer.ScrollToHorizontalOffset(newHOff);
        }
    }

    private double HandleMouseMoveAxisUpdateScroll(double offsetStart, ref double oldScrollMouse, double newScrollMouse, double scrollableMax)
    {
        // How far does the user want to drag since the last update?
        var mouseDelta = oldScrollMouse - newScrollMouse;

        // Add mouse delta to current scroll offset to get the new expected scroll offset
        var newScrollOffset = offsetStart + mouseDelta;

        // Keep the scroll offset from going off the screen
        var newScrollOffsetClamped = newScrollOffset.Clamp(0, scrollableMax);

        // Save the current mouse position in scroll coordinates so that we'll have it for next update
        oldScrollMouse = newScrollMouse;

        return newScrollOffsetClamped;
    }

    private void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _content.ReleaseMouseCapture();
        _updateCounter = 0; // Reset counter, used to prevent jumping at start of drag
        _scrollViewer.Cursor = null;
    }

    public void Unload()
    {
        _content.MouseLeftButtonDown -= scrollViewer_MouseLeftButtonDown;
        _content.PreviewMouseMove -= scrollViewer_PreviewMouseMove;
        _content.PreviewMouseLeftButtonUp -= scrollViewer_PreviewMouseLeftButtonUp;
    }
}

public static class MathExtensions
{
    // Clamp the value between the min and max. Value returned will be min or max if it's below min or above max
    public static double Clamp(this Double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;
using Button = System.Windows.Controls.Button;
using Control = System.Windows.Controls.Control;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;
using Point = System.Windows.Point;
using System.Windows.Media;

namespace Diffusion.Toolkit
{
    public partial class SWWindow : Window
    {
        private bool isMouseButtonDown;
        private Point mouseDownPosition;
        private Point positionBeforeDrag;
        public Grid WindowRoot { get; private set; }
        public Grid LayoutRoot { get; private set; }
        public Button MinimizeButton { get; private set; }
        public Button MaximizeButton { get; private set; }
        public Button RestoreButton { get; private set; }
        public Button CloseButton { get; private set; }
        public Grid HeaderBar { get; private set; }
        public Grid TitleBar { get; private set; }


        public static readonly DependencyProperty MenuWidthProperty =
            DependencyProperty.Register(
                name: nameof(MenuWidth),
                propertyType: typeof(GridLength),
                ownerType: typeof(SWWindow), new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star)));

        public GridLength MenuWidth
        {
            get => (GridLength)GetValue(MenuWidthProperty);
            set => SetValue(MenuWidthProperty, value);
        }


        public static readonly DependencyProperty TitleWidthProperty =
            DependencyProperty.Register(
                name: nameof(TitleWidth),
                propertyType: typeof(GridLength),
                ownerType: typeof(SWWindow), new FrameworkPropertyMetadata(GridLength.Auto));

        private Point previousScreenBounds;
        private bool isManualDrag;

        public GridLength TitleWidth
        {
            get => (GridLength)GetValue(TitleWidthProperty);
            set => SetValue(TitleWidthProperty, value);
        }

        public T GetRequiredTemplateChild<T>(string childName) where T : DependencyObject
        {
            return (T)GetTemplateChild(childName);
        }
        internal static class SystemHelper
        {
            public static int GetCurrentDPI()
            {
                return (int)typeof(SystemParameters).GetProperty
                    ("Dpi", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
            }

            public static double GetCurrentDPIScaleFactor()
            {
                return (double)GetCurrentDPI() / 96;
            }

            //public static Point GetMousePositionWindowsForms()
            //{
            //    System.Drawing.Point point = Control.MousePosition;
            //    return new Point(point.X, point.Y);
            //}
        }

        public SWWindow()
        {
            //double currentDPIScaleFactor =
            //    (double)SystemHelper.GetCurrentDPIScaleFactor();
            //Screen screen =
            //    Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            SizeChanged +=
                new SizeChangedEventHandler(OnSizeChanged);
            //StateChanged += new EventHandler(OnStateChanged);
            //Loaded += new RoutedEventHandler(OnLoaded);
            //Rectangle workingArea = screen.WorkingArea;
            //MaxHeight =
            //    (double)(workingArea.Height + 16) / currentDPIScaleFactor;
            //SystemEvents.DisplaySettingsChanged +=
            //    new EventHandler(SystemEvents_DisplaySettingsChanged);
            //AddHandler(MouseLeftButtonUpEvent,
            //    new MouseButtonEventHandler(OnMouseButtonUp), true);
            //AddHandler(MouseMoveEvent,
            //    new MouseEventHandler(OnMouseMove));
        }

        public override void OnApplyTemplate()
        {
            WindowRoot = GetRequiredTemplateChild<Grid>("WindowRoot");
            LayoutRoot = GetRequiredTemplateChild<Grid>("LayoutRoot");
            MinimizeButton = GetRequiredTemplateChild
                                  <Button>("MinimizeButton");
            MaximizeButton = GetRequiredTemplateChild
                                  <Button>("MaximizeButton");
            RestoreButton = GetRequiredTemplateChild
                                 <Button>("RestoreButton");
            CloseButton = GetRequiredTemplateChild
                               <Button>("CloseButton");
            HeaderBar = GetRequiredTemplateChild<Grid>("PART_HeaderBar");
            TitleBar = GetRequiredTemplateChild<Grid>("PART_TitleBar");

            if (TitleBar != null)
            {
                TitleBar.AddHandler(MouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(OnHeaderBarMouseLeftButtonDown));

                TitleBar.AddHandler(MouseLeftButtonUpEvent,
                    new MouseButtonEventHandler(OnHeaderBarMouseLeftButtonUp));

                TitleBar.AddHandler(MouseMoveEvent,
                    new MouseEventHandler(OnHeaderBarMouseMove));
            }

            if (CloseButton != null)
            {
                CloseButton.Click += CloseButton_Click;
            }

            if (MinimizeButton != null)
            {
                MinimizeButton.Click += MinimizeButton_Click;
            }

            if (RestoreButton != null)
            {
                RestoreButton.Click += RestoreButton_Click;
            }

            if (MaximizeButton != null)
            {
                MaximizeButton.Click += MaximizeButton_Click;
            }

            base.OnApplyTemplate();
        }

        protected virtual void OnHeaderBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && WindowState == WindowState.Maximized)
            {
                Point position = e.GetPosition(this);
                var sceenPosition = PointToScreen(position);

                var vect = mouseDownPosition - position;

                if (vect.Length > 1)
                {
                    var relativeDistance = position.X / ActualWidth;

                    WindowState = WindowState.Normal;

                    var actualDistance = ActualWidth * relativeDistance;

                    try
                    {
                        positionBeforeDrag = new Point(Left, Top);
                        Top = sceenPosition.Y - position.Y;
                        Left = sceenPosition.X - actualDistance;

                        DragMove();
                        //CaptureMouse();
                    }
                    catch
                    {
                    }
                }
            }

        }

        protected virtual void OnHeaderBarMouseLeftButtonUp
            (object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = false;
            //ReleaseMouseCapture();
        }

        protected virtual void OnHeaderBarMouseLeftButtonDown
            (object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            int headerBarHeight = 36;
            int leftmostClickableOffset = 50;

            if (position.X - LayoutRoot.Margin.Left <= leftmostClickableOffset &&
                position.Y <= headerBarHeight)
            {
                if (e.ClickCount != 2)
                {
                    // this.OpenSystemContextMenu(e);
                }
                else
                {
                    Close();
                }
                e.Handled = true;
                return;
            }

            if (e.ClickCount == 2 && (ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip))
            {
                ToggleWindowState();
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                isMouseButtonDown = true;
                mouseDownPosition = position;
            }
            else
            {
                try
                {
                    positionBeforeDrag = new Point(Left, Top);
                    DragMove();
                }
                catch
                {
                }
            }
        }

        protected void ToggleWindowState()
        {
            if (WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected virtual Thickness GetDefaultMarginForDpi()
        {
            int currentDPI = SystemHelper.GetCurrentDPI();

            Thickness thickness = new Thickness(8, 8, 8, 8);

            if (currentDPI == 120)
            {
                thickness = new Thickness(7, 7, 4, 5);
            }
            else if (currentDPI == 144)
            {
                thickness = new Thickness(7, 7, 3, 1);
            }
            else if (currentDPI == 168)
            {
                thickness = new Thickness(6, 6, 2, 0);
            }
            else if (currentDPI == 192)
            {
                thickness = new Thickness(6, 6, 0, 0);
            }
            else if (currentDPI == 240)
            {
                thickness = new Thickness(6, 6, 0, 0);
            }
            return thickness;
        }

        protected virtual Thickness GetFromMinimizedMarginForDpi()
        {
            int currentDPI = SystemHelper.GetCurrentDPI();
            Thickness thickness = new Thickness(7, 7, 5, 7);
            if (currentDPI == 120)
            {
                thickness = new Thickness(6, 6, 4, 6);
            }
            else if (currentDPI == 144)
            {
                thickness = new Thickness(7, 7, 4, 4);
            }
            else if (currentDPI == 168)
            {
                thickness = new Thickness(6, 6, 2, 2);
            }
            else if (currentDPI == 192)
            {
                thickness = new Thickness(6, 6, 2, 2);
            }
            else if (currentDPI == 240)
            {
                thickness = new Thickness(6, 6, 0, 0);
            }
            return thickness;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            double width = (double)screen.WorkingArea.Width;
            Rectangle workingArea = screen.WorkingArea;
            previousScreenBounds = new Point(width,
                                     (double)workingArea.Height);
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            double width = (double)screen.WorkingArea.Width;
            Rectangle workingArea = screen.WorkingArea;
            previousScreenBounds = new Point(width,
                                     (double)workingArea.Height);
            RefreshWindowState();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                HeightBeforeMaximize = ActualHeight;
                WidthBeforeMaximize = ActualWidth;
                return;
            }
            if (WindowState == WindowState.Maximized)
            {
                Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
                if (previousScreenBounds.X != (double)screen.WorkingArea.Width ||
                previousScreenBounds.Y != (double)screen.WorkingArea.Height)
                {
                    double width = (double)screen.WorkingArea.Width;
                    Rectangle workingArea = screen.WorkingArea;
                    previousScreenBounds = new Point(width,
                                           (double)workingArea.Height);
                    RefreshWindowState();
                }
            }
        }

        public double WidthBeforeMaximize { get; set; }

        public double HeightBeforeMaximize { get; set; }

        private void OnStateChanged(object sender, EventArgs e)
        {
            Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            Thickness thickness = new Thickness(0);
            if (WindowState != WindowState.Maximized)
            {

                double currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
                Rectangle workingArea = screen.WorkingArea;
                MaxHeight = (double)(workingArea.Height + 16) / currentDPIScaleFactor;
                MaxWidth = double.PositiveInfinity;

                if (WindowState != WindowState.Maximized)
                {
                    //this.SetMaximizeButtonsVisibility(Visibility.Visible, Visibility.Collapsed);
                }
            }
            else
            {

                thickness = GetDefaultMarginForDpi();
                if (PreviousState == WindowState.Minimized ||
                Left == positionBeforeDrag.X &&
                Top == positionBeforeDrag.Y)
                {
                    thickness = GetFromMinimizedMarginForDpi();
                }

                //this.SetMaximizeButtonsVisibility(Visibility.Collapsed, Visibility.Visible);
            }

            //LayoutRoot.Margin = thickness;
            PreviousState = WindowState;
        }

        public WindowState PreviousState { get; set; }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouseButtonDown)
            {
                return;
            }

            double currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
            Point position = e.GetPosition(this);
            System.Diagnostics.Debug.WriteLine(position);
            Point screen = PointToScreen(position);
            double x = mouseDownPosition.X - position.X;
            double y = mouseDownPosition.Y - position.Y;
            if (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) > 1)
            {
                double actualWidth = mouseDownPosition.X;

                if (mouseDownPosition.X <= 0)
                {
                    actualWidth = 0;
                }
                else if (mouseDownPosition.X >= ActualWidth)
                {
                    actualWidth = WidthBeforeMaximize;
                }

                if (WindowState == WindowState.Maximized)
                {
                    ToggleWindowState();
                    Top = (screen.Y - position.Y) / currentDPIScaleFactor;
                    Left = (screen.X - position.X) / currentDPIScaleFactor;
                    CaptureMouse();
                }

                isManualDrag = true;

                Top = (screen.Y - mouseDownPosition.Y) / currentDPIScaleFactor;
                Left = (screen.X - actualWidth) / currentDPIScaleFactor;
            }
        }

        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = false;
            isManualDrag = false;
            ReleaseMouseCapture();
        }

        private void RefreshWindowState()
        {
            if (WindowState == WindowState.Maximized)
            {
                ToggleWindowState();
                ToggleWindowState();
            }
        }

    }
}

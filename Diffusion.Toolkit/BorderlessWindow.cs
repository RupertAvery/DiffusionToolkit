using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Button = System.Windows.Controls.Button;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;
using Point = System.Windows.Point;

namespace Diffusion.Toolkit
{
    public class BorderlessWindow : Window
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

        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register(
                name: nameof(IsFullScreen),
                propertyType: typeof(bool),
                ownerType: typeof(BorderlessWindow), new FrameworkPropertyMetadata());

        public static readonly DependencyProperty TitleVisibilityProperty =
            DependencyProperty.Register(
                name: nameof(TitleVisibility),
                propertyType: typeof(Visibility),
                ownerType: typeof(BorderlessWindow), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty MenuWidthProperty =
            DependencyProperty.Register(
                name: nameof(MenuWidth),
                propertyType: typeof(GridLength),
                ownerType: typeof(BorderlessWindow), new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star)));

        public bool IsFullScreen
        {
            get => (bool)GetValue(IsFullScreenProperty);
            set => SetValue(IsFullScreenProperty, value);
        }
        
        public Visibility TitleVisibility
        {
            get => (Visibility)GetValue(TitleVisibilityProperty);
            set => SetValue(TitleVisibilityProperty, value);
        }

        public GridLength MenuWidth
        {
            get => (GridLength)GetValue(MenuWidthProperty);
            set => SetValue(MenuWidthProperty, value);
        }
        

        public static readonly DependencyProperty TitleContentProperty =
            DependencyProperty.Register(
                name: nameof(TitleContent),
                propertyType: typeof(object),
                ownerType: typeof(BorderlessWindow), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty TitleWidthProperty =
            DependencyProperty.Register(
                name: nameof(TitleWidth),
                propertyType: typeof(GridLength),
                ownerType: typeof(BorderlessWindow), new FrameworkPropertyMetadata(GridLength.Auto));

        public static readonly DependencyProperty ExtraButtonWidthProperty =
            DependencyProperty.Register(
                name: nameof(ExtraButtonWidth),
                propertyType: typeof(GridLength),
                ownerType: typeof(BorderlessWindow), new FrameworkPropertyMetadata(GridLength.Auto));

        private Point previousScreenBounds;
        private bool isManualDrag;

        public GridLength TitleWidth
        {
            get => (GridLength)GetValue(TitleWidthProperty);
            set => SetValue(TitleWidthProperty, value);
        }
        
        public object TitleContent
        {
            get => (object)GetValue(TitleContentProperty);
            set => SetValue(TitleContentProperty, value);
        }

        public GridLength ExtraButtonWidth
        {
            get => (GridLength)GetValue(ExtraButtonWidthProperty);
            set => SetValue(ExtraButtonWidthProperty, value);
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

        public BorderlessWindow()
        {
            //double currentDPIScaleFactor =
            //    (double)SystemHelper.GetCurrentDPIScaleFactor();
            //Screen screen =
            //    Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            SizeChanged +=
                OnSizeChanged;
            StateChanged += OnStateChanged;

            Loaded += OnLoaded;
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

                if (vect.Length > 1 && !IsFullScreen)
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

            if (IsFullScreen) return;

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
            double width = screen.WorkingArea.Width;
            Rectangle workingArea = screen.WorkingArea;
            previousScreenBounds = new Point(width,
                                     workingArea.Height);

            SetMaximizeButtonsVisibility(true, false);
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            double width = screen.WorkingArea.Width;
            Rectangle workingArea = screen.WorkingArea;
            previousScreenBounds = new Point(width,
                                     workingArea.Height);
            RefreshWindowState();
        }


        private WindowState _lastWindowState;
        private WindowStyle _lastWindowStyle;
        private ResizeMode _lastResizeMode;

        public void SetFullScreen(bool mode)
        {
            IsFullScreen = mode;

            if (IsFullScreen)
            {
                Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);

                _lastWindowState = WindowState;
                _lastWindowStyle = WindowStyle;
                _lastResizeMode = ResizeMode;

                Visibility = Visibility.Collapsed;
                ResizeMode = ResizeMode.NoResize;
                //WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;

                var workingArea = screen.Bounds;

                //Topmost = true;

                Left = workingArea.Left;
                Top = workingArea.Top;
                Width = workingArea.Width;
                Height = workingArea.Height;

                Visibility = Visibility.Visible;

                HideButtons();
            }
            else
            {
                //Topmost = false;
                ResizeMode = _lastResizeMode;
                WindowStyle = _lastWindowStyle;
                WindowState = _lastWindowState;

                ShowButtons();
            }
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
                if (previousScreenBounds.X != screen.WorkingArea.Width ||
                previousScreenBounds.Y != screen.WorkingArea.Height)
                {
                    double width = screen.WorkingArea.Width;
                    Rectangle workingArea = screen.WorkingArea;
                    previousScreenBounds = new Point(width,
                                           workingArea.Height);
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

                double currentDPIScaleFactor = SystemHelper.GetCurrentDPIScaleFactor();
                Rectangle workingArea = screen.WorkingArea;
                MaxHeight = double.PositiveInfinity;  //(workingArea.Height + 16) / currentDPIScaleFactor;
                MaxWidth = double.PositiveInfinity;

                if (WindowState != WindowState.Maximized)
                {
                    SetMaximizeButtonsVisibility(true, false);
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

                SetMaximizeButtonsVisibility(false, true);
            }

            //LayoutRoot.Margin = thickness;
            PreviousState = WindowState;
        }

        Visibility _restoreButtonVisibility;
        Visibility _maximizeButtonVisibility;
        Visibility _minimizeButtonVisibility;

        private void ShowButtons()
        {
            CloseButton.Visibility = Visibility.Visible;
            RestoreButton.Visibility = _restoreButtonVisibility;
            MaximizeButton.Visibility = _maximizeButtonVisibility;
            MinimizeButton.Visibility = _minimizeButtonVisibility;
        }

        private void HideButtons()
        {
            _restoreButtonVisibility = RestoreButton.Visibility;
            _maximizeButtonVisibility = MaximizeButton.Visibility;
            _minimizeButtonVisibility = MinimizeButton.Visibility;

            CloseButton.Visibility = Visibility.Collapsed;
            RestoreButton.Visibility = Visibility.Collapsed;
            MaximizeButton.Visibility = Visibility.Collapsed;
            MinimizeButton.Visibility = Visibility.Collapsed;
        }


        private void SetMaximizeButtonsVisibility(bool maximizeButton, bool restoreButton, bool minimizeButton = true)
        {
            if (WindowStyle == WindowStyle.ToolWindow)
            {
                maximizeButton = false;
                restoreButton = false;
                minimizeButton = false;
            }

            RestoreButton.Visibility = restoreButton ? Visibility.Visible : Visibility.Collapsed;
            MaximizeButton.Visibility = maximizeButton ? Visibility.Visible : Visibility.Collapsed;
            MinimizeButton.Visibility = minimizeButton ? Visibility.Visible : Visibility.Collapsed;
        }

        public WindowState PreviousState { get; set; }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouseButtonDown)
            {
                return;
            }

            double currentDPIScaleFactor = SystemHelper.GetCurrentDPIScaleFactor();
            Point position = e.GetPosition(this);
            //Debug.WriteLine(position);
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



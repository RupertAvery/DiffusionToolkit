using Diffusion.Toolkit.Behaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Diffusion.Toolkit.Controls
{
    public enum AccordionState
    {
        Expanded,
        Collapsed
    }

    /// <summary>
    /// Interaction logic for AccordionControl.xaml
    /// </summary>
    public partial class AccordionControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ButtonAreaContentProperty =
            DependencyProperty.Register(
                nameof(ButtonAreaContent),
                typeof(object),
                typeof(AccordionControl),
                new UIPropertyMetadata(null)
                );

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(string),
                typeof(AccordionControl),
                new UIPropertyMetadata(null)
            );


        public static readonly DependencyProperty ContainerHeightProperty =
            DependencyProperty.Register(
                nameof(ContainerHeight),
                typeof(double),
                typeof(AccordionControl),
                new UIPropertyMetadata(PropertyChangedCallback)
            );

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                nameof(State),
                typeof(AccordionState),
                typeof(AccordionControl),
                new UIPropertyMetadata(PropertyChangedCallback)
            );

        public static readonly DependencyProperty CanResizeProperty =
            DependencyProperty.Register(
                nameof(CanResize),
                typeof(bool),
                typeof(AccordionControl),
                new UIPropertyMetadata(PropertyChangedCallback)
            );

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(HeaderBackground),
                typeof(Brush),
                typeof(AccordionControl),
                new UIPropertyMetadata(null)
            );


        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case nameof(State):
                    ((AccordionControl)d).SetState();
                    break;
                case nameof(ContainerHeight):
                    ((AccordionControl)d).SetHeight((double)e.NewValue);
                    break;
                case nameof(CanResize):
                    ((AccordionControl)d).SetCanResize((bool)e.NewValue);
                    break;
            }
        }

        public Brush HeaderBackground
        {

            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        public AccordionState State
        {
            get => (AccordionState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public object ButtonAreaContent
        {
            get => (object)GetValue(ButtonAreaContentProperty);
            set => SetValue(ButtonAreaContentProperty, value);
        }

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public Visibility ContentVisibility
        {
            get => _contentVisibility;
            set => SetField(ref _contentVisibility, value);
        }

        public double ContainerHeight
        {
            get => (double)GetValue(ContainerHeightProperty);
            set => SetValue(ContainerHeightProperty, value);
        }

        public bool CanResize
        {
            get => (bool)GetValue(CanResizeProperty);
            set => SetValue(CanResizeProperty, value);
        }

        public AccordionControl()
        {
            InitializeComponent();
            SetState();
            SetCanResize(false);
            ContainerHeight = Double.PositiveInfinity;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private Visibility _contentVisibility;

        void SetState()
        {
            ContentVisibility = State switch
            {
                AccordionState.Collapsed => Visibility.Collapsed,
                AccordionState.Expanded => Visibility.Visible
            };
        }

        void SetHeight(double height)
        {
            void SetHeightInternal(double height)
            {
                _scrollViewer = GetVisualChild<ScrollViewer>(this);
                if (_scrollViewer != null)
                {
                    var presenter = GetVisualChild<ContentPresenter>(_scrollViewer);
                    if (presenter != null)
                    {
                        _child = (FrameworkElement)VisualTreeHelper.GetChild(presenter, 0);
                        _scrollViewer.MaxHeight = height;
                        if (double.IsPositiveInfinity(height))
                        {
                            DTBehaviors.SetIsScrollDisabled(_scrollViewer, true);
                        }
                        else
                        {
                            DTBehaviors.SetIsScrollDisabled(_scrollViewer, false);
                        }
                    }
                }
       
            }

            if (this.IsLoaded)
            {
                SetHeightInternal(height);
            }
            else
            {
                this.Loaded += (sender, args) =>
                {
                    SetHeightInternal(height);
                };
            }
        }

        void SetCanResize(bool canResize)
        {
            void SetCanResizeInternal(bool canResize)
            {
                if (!canResize)
                {
                    _scrollViewer = GetVisualChild<ScrollViewer>(this);
                    if (_scrollViewer != null)
                    {
                        var presenter = GetVisualChild<ContentPresenter>(_scrollViewer);
                        if (presenter != null)
                        {
                            _scrollViewer.MaxHeight = Double.PositiveInfinity;
                            DTBehaviors.SetIsScrollDisabled(_scrollViewer, true);
                        }
                    }
                }
                else
                {
                    _scrollViewer = GetVisualChild<ScrollViewer>(this);
                    if (_scrollViewer != null)
                    {
                        var presenter = GetVisualChild<ContentPresenter>(_scrollViewer);
                        if (presenter != null)
                        {
                            _scrollViewer.MaxHeight = ContainerHeight;
                            //_scrollViewer.Height = _child.ActualHeight;
                            DTBehaviors.SetIsScrollDisabled(_scrollViewer, false);
                        }
                    }

                }
            }


            if (this.IsLoaded)
            {
                SetCanResizeInternal(canResize);
            }
            else
            {
                this.Loaded += (sender, args) =>
                {
                    SetCanResizeInternal(canResize);
                };
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (e is MouseEventArgs { LeftButton: MouseButtonState.Pressed } || e.Source is Button)
            {
                State = State switch
                {
                    AccordionState.Collapsed => AccordionState.Expanded,
                    AccordionState.Expanded => AccordionState.Collapsed
                };

                SetState();
                e.Handled = true;
            }

        }

        bool _isResizing = false;
        FrameworkElement? _scrollViewer = null;
        FrameworkElement? _child = null;
        private Point _mouseCoords;

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isResizing = true;
            _scrollViewer = GetVisualChild<ScrollViewer>(this);
            var presenter = GetVisualChild<ContentPresenter>(_scrollViewer);
            _child = (FrameworkElement)VisualTreeHelper.GetChild(presenter, 0);

            if (_child == null)
            {
                _child = GetVisualChild<Grid>(_scrollViewer);
            }

            _mouseCoords = e.GetPosition(this);
            Mouse.Capture((UIElement)sender);
        }

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var mouseCoords = e.GetPosition(this);

                    if (_scrollViewer.MaxHeight == Double.PositiveInfinity)
                    {
                        _scrollViewer.MaxHeight = _scrollViewer.ActualHeight;
                    }

                    var currentHeight = _scrollViewer.MaxHeight;

                    currentHeight += mouseCoords.Y - _mouseCoords.Y;

                    if (currentHeight >= _child.ActualHeight)
                    {
                        _scrollViewer.MaxHeight = Double.PositiveInfinity;
                        DTBehaviors.SetIsScrollDisabled(_scrollViewer, true);
                    }
                    else if (currentHeight > 0)
                    {
                        _scrollViewer.MaxHeight = currentHeight;
                        DTBehaviors.SetIsScrollDisabled(_scrollViewer, false);
                    }
                    else
                    {
                        _scrollViewer.MaxHeight = 0;
                    }

                    _mouseCoords = e.GetPosition(this);
                }
            }
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isResizing = false;
            Mouse.Capture(null);
            ContainerHeight = _scrollViewer.MaxHeight;
        }

        private static T? GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T? child = default(T);

            if (parent != null)
            {
                int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < numVisuals; i++)
                {
                    Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                    child = v as T;
                    if (child == null)
                    {
                        child = GetVisualChild<T>(v);
                    }
                    if (child != null)
                    {
                        break;
                    }
                }
            }

            return child;
        }
    }
}

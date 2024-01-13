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

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                nameof(State),
                typeof(AccordionState),
                typeof(AccordionControl),
                new UIPropertyMetadata(PropertyChangedCallback)
            );

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(State))
            {
                ((AccordionControl)d).SetState();
            }
        }

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(HeaderBackground),
                typeof(Brush),
                typeof(AccordionControl),
                new UIPropertyMetadata(null)
            );

        private float _rotation;

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

        public float Rotation
        {
            get => _rotation;
            set => SetField(ref _rotation, value);
        }

        public Visibility ContentVisibility
        {
            get => _contentVisibility;
            set => SetField(ref _contentVisibility, value);
        }

        public AccordionControl()
        {
            InitializeComponent();
            SetState();
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

            Rotation = State switch
            {
                AccordionState.Collapsed => 0,
                AccordionState.Expanded => 90
            };

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
    }
}

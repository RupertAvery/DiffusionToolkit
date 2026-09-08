using System.Windows;
using System.Windows.Controls;

namespace Diffusion.Toolkit.Behaviors
{

    public static partial class DTBehaviors
    {
        public static readonly DependencyProperty SelectOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "SelectOnFocus",
                typeof(bool),
                typeof(DTBehaviors),
                new PropertyMetadata(SelectOnFocusPropertyChanged)
                );

        public static bool GetSelectOnFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectOnFocusProperty);
        }

        public static void SetSelectOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectOnFocusProperty, value);
        }

        private static void SelectOnFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                var value = GetSelectOnFocus(d);
                if (value)
                {
                    element.GotFocus += GotFocusHandler;
                }
                else
                {
                    element.GotFocus -= GotFocusHandler;
                }
            }
        }

        private static void GotFocusHandler(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectionStart = 0;
                textBox.SelectionLength = textBox.Text.Length;
            }

        }
    }
}

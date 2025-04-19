using System.Windows;

namespace Diffusion.Toolkit.Behaviors
{
    public static partial class DTBehaviors
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(DTBehaviors),
                new PropertyMetadata(IsFocusedPropertyChanged)
                );

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void IsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.Focus();
                element.BringIntoView();
            }
        }
    }
}

using System.Windows;
using System.Windows.Data;

namespace Diffusion.Toolkit.Behaviors
{
    public static partial class DTBehaviors
    {
        public static readonly DependencyProperty IsMouseOverProperty =
            DependencyProperty.RegisterAttached(
                "IsMouseOver",
                typeof(bool),
                typeof(DTBehaviors),
                new PropertyMetadata(IsMouseOverPropertyChanged)
            );

        public static bool GetIsMouseOver(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMouseOverProperty);
        }

        public static void SetIsMouseOver(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMouseOverProperty, value);
        }
        
        private static void IsMouseOverPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
 
        }



        public static readonly DependencyProperty IsMouseOverBindingProperty =
            DependencyProperty.RegisterAttached(
                "IsMouseOverBinding",
                typeof(object),
                typeof(DTBehaviors)
            );

        public static object GetIsMouseOverBinding(DependencyObject obj)
        {
            return (object)obj.GetValue(IsMouseOverBindingProperty);
        }

        public static void SetIsMouseOverBinding(DependencyObject obj, object value)
        {
            obj.SetValue(IsMouseOverBindingProperty, value);
        }

        public static readonly DependencyProperty IsMouseOverParameterProperty =
            DependencyProperty.RegisterAttached(
                "IsMouseOverParameter",
                typeof(object),
                typeof(DTBehaviors)
            );

        public static object GetIsMouseOverParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(IsMouseOverParameterProperty);
        }

        public static void SetIsMouseOverParameter(DependencyObject obj, object value)
        {
            obj.SetValue(IsMouseOverParameterProperty, value);
        }
    }
}

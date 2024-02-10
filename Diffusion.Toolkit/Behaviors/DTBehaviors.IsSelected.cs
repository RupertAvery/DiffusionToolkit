using System.Windows;

namespace Diffusion.Toolkit.Behaviors
{
    public static partial class DTBehaviors
    {

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached(
                "IsSelected", 
                typeof(bool), 
                typeof(DTBehaviors), 
                new PropertyMetadata(IsSelectedPropertyChanged)
                );

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        private static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}

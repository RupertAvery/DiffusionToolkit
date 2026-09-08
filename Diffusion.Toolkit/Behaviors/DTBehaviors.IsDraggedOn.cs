using System.Windows;

namespace Diffusion.Toolkit.Behaviors
{
    public static partial class DTBehaviors
    {
        public static readonly DependencyProperty IsDraggedOnProperty =
            DependencyProperty.RegisterAttached(
                "IsDraggedOn",
                typeof(bool),
                typeof(DTBehaviors),
               new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public static bool GetIsDraggedOn(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDraggedOnProperty);
        }

        public static void SetIsDraggedOn(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDraggedOnProperty, value);
        }

        private static void IsDraggedOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}

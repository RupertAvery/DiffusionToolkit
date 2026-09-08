using System.Windows;

namespace Diffusion.Toolkit.Behaviors
{

    public static partial class DTBehaviors
    {
        public static readonly DependencyProperty UseIsDraggedHandlersProperty =
            DependencyProperty.RegisterAttached(
                "UseIsDraggedHandlers", 
                typeof(bool), 
                typeof(DTBehaviors), 
                new PropertyMetadata(UseIsDraggedHandlersPropertyChanged)
                );

        public static bool GetUseIsDraggedHandlers(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseIsDraggedHandlersProperty);
        }

        public static void SetUseIsDraggedHandlers(DependencyObject obj, bool value)
        {
            obj.SetValue(UseIsDraggedHandlersProperty, value);
        }

        private static void UseIsDraggedHandlersPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                var value = GetUseIsDraggedHandlers(d);
                if (value)
                {
                    element.DragEnter += DragEnterHandler;
                    element.DragLeave += DragLeaveHandler;
                    element.Drop += DropHandler;
                }
                else
                {
                    element.DragEnter -= DragEnterHandler;
                    element.DragLeave -= DragLeaveHandler;
                    element.Drop -= DropHandler;
                }
            }
        }

        private static void DragEnterHandler(object sender, DragEventArgs e)
        {
            SetIsDraggedOn((DependencyObject)sender, true);
        }

        private static void DragLeaveHandler(object sender, DragEventArgs e)
        {
            SetIsDraggedOn((DependencyObject)sender, false);
        }
        private static void DropHandler(object sender, DragEventArgs e)
        {
            SetIsDraggedOn((DependencyObject)sender, false);
        }
    }
}

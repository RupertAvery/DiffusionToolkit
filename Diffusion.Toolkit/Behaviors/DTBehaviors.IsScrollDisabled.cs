using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Diffusion.Toolkit.Behaviors
{
    public static partial class DTBehaviors
    {
        public static readonly DependencyProperty IsScrollDisabledProperty =
            DependencyProperty.RegisterAttached(
                "IsScrollDisabled", 
                typeof(bool), 
                typeof(DTBehaviors), 
                new PropertyMetadata(IsScrollDisabledPropertyChanged)
                );

        public static bool GetIsScrollDisabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsScrollDisabledProperty);
        }

        public static void SetIsScrollDisabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsScrollDisabledProperty, value);
        }

        private static void IsScrollDisabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer element)
            {
                ScrollViewer parent = null;

                void ElementOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
                {
                    if (!e.Handled)
                    {
                        e.Handled = true;
                        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                        eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                        eventArg.Source = sender;
                        parent.RaiseEvent(eventArg);
                    }
                }

                if ((bool)e.NewValue == false)
                {
                    element.PreviewMouseWheel -= ElementOnPreviewMouseWheel;
                }
                else
                {
                    element.Loaded += delegate
                    {
                        parent = GetVisualParent<ScrollViewer>(element);


                        element.PreviewMouseWheel += ElementOnPreviewMouseWheel;
                    };
                };
            }
        }
        
        private static T GetVisualParent<T>(DependencyObject child) where T : Visual
        {
            var parent = default(T);

            var v = (Visual?)VisualTreeHelper.GetParent(child);

            parent = v as T;

            if (parent == null && v != null)
            {
                parent = GetVisualParent<T>(v);
            }

            return parent;
        }
    }
}

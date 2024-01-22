using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Diffusion.Toolkit.Behaviors
{
    public static class DTBehaviors
    {
        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanFocusOnLoad.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(DTBehaviors), new PropertyMetadata(IsSelectedPropertyChanged));

        private static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var element = d as FrameworkElement;
            //if (element != null)
            //{
            //    element.Loaded += delegate
            //    {
            //        element.Focus();
            //    };
            //}
        }

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanFocusOnLoad.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(DTBehaviors), new PropertyMetadata(IsFocusedPropertyChanged));

        private static void IsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element != null)
            {
                element.BringIntoView();
            }
        }

        public static bool GetIsDraggedOn(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDraggedOnProperty);
        }

        public static void SetIsDraggedOn(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDraggedOnProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanFocusOnLoad.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDraggedOnProperty =
            DependencyProperty.RegisterAttached("IsDraggedOn", typeof(bool), typeof(DTBehaviors), new PropertyMetadata(IsDraggedOnPropertyChanged));

        private static void IsDraggedOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }


        public static bool GetUseIsDraggedHandlers(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseIsDraggedHandlersProperty);
        }

        public static void SetUseIsDraggedHandlers(DependencyObject obj, bool value)
        {
            obj.SetValue(UseIsDraggedHandlersProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanFocusOnLoad.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseIsDraggedHandlersProperty =
            DependencyProperty.RegisterAttached("UseIsDraggedHandlers", typeof(bool), typeof(DTBehaviors), new PropertyMetadata(UseIsDraggedHandlersPropertyChanged));

        private static void UseIsDraggedHandlersPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;

            if (element != null)
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

using System;
using Diffusion.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Diffusion.Toolkit.Behaviors
{
    public static partial class DTBehaviors
    {
        public static readonly DependencyProperty UseIsMouseOverHandlersProperty =
            DependencyProperty.RegisterAttached(
                "UseIsMouseOverHandlers", 
                typeof(bool), 
                typeof(DTBehaviors), 
                new PropertyMetadata(UseIsMouseOverHandlersPropertyChanged)
                );

        public static bool GetUseIsMouseOverHandlers(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseIsMouseOverHandlersProperty);
        }

        public static void SetUseIsMouseOverHandlers(DependencyObject obj, bool value)
        {
            obj.SetValue(UseIsMouseOverHandlersProperty, value);
        }

        private static void UseIsMouseOverHandlersPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                var value = GetUseIsMouseOverHandlers(d);
                if (value)
                {
                    element.MouseMove += MouseMoveHandler;
                    element.MouseEnter += MouseEnterHandler;
                    element.MouseLeave += MouseLeaveHandler;
                }
                else
                {
                    element.MouseMove -= MouseMoveHandler;
                    element.MouseEnter -= MouseEnterHandler;
                    element.MouseLeave -= MouseLeaveHandler;
                }
            }
        }

        private static void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            var bindingExpression = ((FrameworkElement)sender).GetBindingExpression(IsMouseOverBindingProperty);
            var boundInput = (object)bindingExpression.ResolvedSource;
            var boundInputType = boundInput.GetType();
            var boundInputProperty = boundInputType.GetProperty(bindingExpression.ResolvedSourcePropertyName);

            var parameterValue = GetIsMouseOverParameter((DependencyObject)sender);

            if (parameterValue != null && boundInputProperty.PropertyType != null)
            {
                var convertedValue = ConvertToPropertyType(parameterValue, boundInputProperty.PropertyType);
                boundInputProperty.SetValue(boundInput, convertedValue);
            }
            else
            {
                boundInputProperty.SetValue(boundInput, parameterValue);
            }


            SetIsMouseOver((DependencyObject)sender, true);
        }

        private static void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            // var binding = GetIsMouseOverBinding((DependencyObject)sender);

            //var bindingExpression = ((FrameworkElement)sender).GetBindingExpression(IsMouseOverBindingProperty);
            //var boundInput = (object)bindingExpression.ResolvedSource;
            //var boundInputType = boundInput.GetType();
            //var boundInputProperty = boundInputType.GetProperty(bindingExpression.ResolvedSourcePropertyName);

            //var parameterValue = GetIsMouseOverParameter((DependencyObject)sender);

            //if (parameterValue != null && boundInputProperty.PropertyType != null)
            //{
            //    var convertedValue = ConvertToPropertyType(parameterValue, boundInputProperty.PropertyType);
            //    boundInputProperty.SetValue(boundInput, convertedValue);
            //}
            //else
            //{
            //    boundInputProperty.SetValue(boundInput, parameterValue);
            //}


            SetIsMouseOver((DependencyObject)sender, true);
        }

        static object? ConvertToPropertyType(object? value, Type targetType)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                // Handle null/empty: return null for nullable, default for non-nullable
                return Nullable.GetUnderlyingType(targetType) != null ? null : Activator.CreateInstance(targetType);
            }

            // Unwrap nullable
            Type nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Special handling for enums
            if (nonNullableType.IsEnum)
            {
                return Enum.Parse(nonNullableType, value.ToString()!, ignoreCase: true);
            }

            return Convert.ChangeType(value, nonNullableType);
        }

        private static void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            SetIsMouseOver((DependencyObject)sender, false);
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Diffusion.Toolkit.Behaviors;

public static class ScrollSpeed
{
    public static double GetScrollSpeed(DependencyObject obj)
    {
        return (double)obj.GetValue(ScrollSpeedProperty);
    }

    public static void SetScrollSpeed(DependencyObject obj, double value)
    {
        obj.SetValue(ScrollSpeedProperty, value);
    }

    public static readonly DependencyProperty ScrollSpeedProperty =
        DependencyProperty.RegisterAttached(
            "ScrollSpeed",
            typeof(double),
            typeof(ScrollSpeed),
            new FrameworkPropertyMetadata(
                1.0,
                FrameworkPropertyMetadataOptions.Inherits & FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(OnScrollSpeedChanged)));

    public static DependencyObject GetScrollViewer(DependencyObject o)
    {
        // Return the DependencyObject if it is a ScrollViewer
        if (o is ScrollViewer)
        { return o; }

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
        {
            var child = VisualTreeHelper.GetChild(o, i);

            var result = GetScrollViewer(child);
            if (result == null)
            {
                continue;
            }
            else
            {
                return result;
            }
        }

        return null;
    }

    private static void OnScrollSpeedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var host = o as UIElement;
        host.PreviewMouseWheel += new MouseWheelEventHandler(OnPreviewMouseWheelScrolled);
    }

    private static void OnPreviewMouseWheelScrolled(object sender, MouseWheelEventArgs e)
    {
        DependencyObject scrollHost = sender as DependencyObject;

        double scrollSpeed = (double)(scrollHost).GetValue(ScrollSpeedProperty);

        ScrollViewer scrollViewer = GetScrollViewer(scrollHost) as ScrollViewer;

        if (scrollViewer != null)
        {
            double offset = scrollViewer.VerticalOffset - (e.Delta * scrollSpeed / 6);
            if (offset < 0)
            {
                scrollViewer.ScrollToVerticalOffset(0);
            }
            else if (offset > scrollViewer.ExtentHeight)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
            }
            else
            {
                scrollViewer.ScrollToVerticalOffset(offset);
            }

            e.Handled = true;
        }
        else
        {
            throw new NotSupportedException("ScrollSpeed Attached Property is not attached to an element containing a ScrollViewer.");
        }
    }
}
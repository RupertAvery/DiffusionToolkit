using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit;

public class VirtualImage : Image
{
    private static void OnPathChanged(DependencyObject defectImageControl, DependencyPropertyChangedEventArgs eventArgs)
    {
        var control = (VirtualImage)defectImageControl;
        control.Path = (string)eventArgs.NewValue;

        //using var fs = new FileStream(control._path, FileMode.Open, FileAccess.Read,
        //    FileShare.Read);

        ////memoryStream.Position = 0;
        //var bitmap = BitmapFrame.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        //control.Source = bitmap;
    }


    //public MemoryStream memoryStream = null;
    public static readonly DependencyProperty PathProperty =
        DependencyProperty.Register(
            name: "Path",
            propertyType: typeof(string),
            ownerType: typeof(VirtualImage),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnPathChanged))
        );
    
    public string? Path
    {
        get => (string)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public VirtualImage() : base()
    {
        IsVisibleChanged += new DependencyPropertyChangedEventHandler(Handler);
    }

    void Handler(object sender, DependencyPropertyChangedEventArgs e)
    {
        var c = (VirtualImage)sender;

        if (IsVisible)
        {
            if(c.Path == null) return;

            using var fs = new FileStream(c.Path, FileMode.Open, FileAccess.Read,
                FileShare.Read);

            //memoryStream.Position = 0;
            var bitmap = BitmapFrame.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            Source = bitmap;
        }
        else
        {
            Source = null;
        }
    }
}


public sealed class MyScrollViewer : ScrollViewer
{
    public static readonly DependencyProperty IsInViewportProperty =
        DependencyProperty.RegisterAttached("IsInViewport", typeof(bool), typeof(MyScrollViewer));

    public static bool GetIsInViewport(UIElement element)
    {
        return (bool)element.GetValue(IsInViewportProperty);
    }

    public static void SetIsInViewport(UIElement element, bool value)
    {
        element.SetValue(IsInViewportProperty, value);
    }

    protected override void OnScrollChanged(ScrollChangedEventArgs e)
    {
        base.OnScrollChanged(e);

        var panel = Content as Panel;
        if (panel == null)
        {
            return;
        }

        Rect viewport = new Rect(new Point(0, 0), RenderSize);

        foreach (UIElement child in panel.Children)
        {
            if (!child.IsVisible)
            {
                SetIsInViewport(child, false);
                continue;
            }

            GeneralTransform transform = child.TransformToAncestor(this);
            Rect childBounds = transform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
            SetIsInViewport(child, viewport.IntersectsWith(childBounds));
        }
    }
}

public interface INavigatorService
{
    void Goto(string url);
    void Back();
    void SetPages(Dictionary<string, Page> pages);
}


public class NavigatorService : INavigatorService
{
    private Dictionary<string, Page> _pages;
    private readonly Stack<string> _history;
    private string _currentUrl;
    public Action<Page> OnNavigate { get; set; }

    public NavigatorService()
    {
        _history = new Stack<string>();
    }

    public void SetPages(Dictionary<string, Page> pages)
    {
        _pages = pages;
    }

    public void Goto(string url)
    {
        if (_currentUrl != null)
        {
            _history.Push(_currentUrl);
        }
        _currentUrl = url;
        Navigate();
    }

    public void Back()
    {
        _currentUrl = _history.Pop();
        Navigate();
    }

    private void Navigate()
    {
        OnNavigate?.Invoke(_pages[_currentUrl]);
    }
}
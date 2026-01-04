using System.Collections.Generic;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using Diffusion.Toolkit.Thumbnails;

namespace Diffusion.Toolkit.Controls;

public class ThumbView
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public BitmapSource BitmapSource { get; set; }
}

public class ThumbnailPanel : FrameworkElement
{
    public static readonly DependencyProperty ScrollHeightProperty = DependencyProperty.Register(
        nameof(ScrollHeight),
        typeof(double),
        typeof(ThumbnailPanel),
        new PropertyMetadata(0d, PropertyChangedCallback)
    );

    public static readonly DependencyProperty ScrollTopProperty = DependencyProperty.Register(
        nameof(ScrollTop),
        typeof(double),
        typeof(ThumbnailPanel),
        new PropertyMetadata(0d, PropertyChangedCallback)
    ); 

    public static readonly DependencyProperty ImageEntriesProperty = DependencyProperty.Register(
        nameof(ImageEntries),
        typeof(IReadOnlyCollection<ImageEntry>),
        typeof(ThumbnailPanel),
        new PropertyMetadata(null, PropertyChangedCallback)
    );

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(ImageEntries))
        {
            ((ThumbnailPanel)d).GenerateThumbViews();
            //Task.Run(() =>
            //{
            //if (ThumbnailCache.Instance.TryGetThumbnail(images.Path, 256, out var thumbnail))
            //{
            //    drawingContext.DrawImage(thumbnail, new Rect(new Point(x, y - ScrollTop), new Size(images.Thumbnail.PixelWidth, images.Thumbnail.PixelHeight)));
            //}
            //});

            //((ThumbnailPanel)d).InvalidateVisual();
        }
        //if (e.Property.Name == nameof(ScrollTop))
        //{
        //    ((ThumbnailPanel)d).InvalidateVisual();
        //}
        //throw new System.NotImplementedException();
    }

    private int _scrollTop;
    private IReadOnlyCollection<ImageEntry>? _imageEntries;

    public IReadOnlyCollection<ImageEntry>? ImageEntries
    {
        get => (IReadOnlyCollection<ImageEntry>?)GetValue(ImageEntriesProperty);
        set => SetValue(ImageEntriesProperty, value);
    }

    public double ScrollTop
    {
        get => (double)GetValue(ScrollTopProperty);
        set => SetValue(ScrollTopProperty, value);
    }

    public double ScrollHeight
    {
        get => (double)GetValue(ScrollHeightProperty);
        set => SetValue(ScrollHeightProperty, value);
    }

    public ThumbnailPanel()
    {
        ThumbnailCache.CreateInstance(0, 0);
        ServiceLocator.ThumbnailService.Size = 256;
        ServiceLocator.ThumbnailService.StartAsync();
    }

    private List<ThumbView> _thumbViews = new List<ThumbView>();
    private int maxSize = 0;
    private void GenerateThumbViews()
    {
        _thumbViews = new List<ThumbView>(ImageEntries.Count);
        for (var j = 0; j < ImageEntries.Count; j++)
        {
            _thumbViews.Add(new ThumbView());
        }

        var thumbsPerRow = ActualWidth / (256 + 5 + 5);
        var rows = ImageEntries.Count / thumbsPerRow;

        Dispatcher.Invoke(() =>
        {
            ScrollHeight = rows * (256 + 5 + 5);
        });

        var i = 0;
        foreach (var images in ImageEntries)
        {
            //_thumbViews[i].X = 0;
            //_thumbViews[i].Y = 0;
            var i1 = i;
            ServiceLocator.ThumbnailService.QueueAsync(new ThumbnailJob()
            {
                Width = 256,
                Height = 256,
                Path = images.Path,
                Type = images.Type,
                BatchId = 0,
                EntryType = EntryType.File
            }, result =>
            {


                _thumbViews[i1].Width = result.Image.PixelWidth;
                _thumbViews[i1].Height = result.Image.PixelHeight;
                _thumbViews[i1].BitmapSource = result.Image;
                CalculateScrollHeight();
                //Dispatcher.Invoke(() => { InvalidateVisual(); });
            });
            i++;
        }
    }

    object _lock = new object();


    void CalculateScrollHeight()
    {
        var iconWidth = 256;
        var iconHeight = 256;

        var marginX = 5;
        var marginY = 5;

        var x = marginX;
        var y = marginY;

        foreach (var thumbView in _thumbViews)
        {
            x += thumbView.Width + marginX * 2;
            if (x + thumbView.Width >= ActualWidth)
            {
                x = 0;
                y += thumbView.Height + marginY * 2;
            }

        }

        Dispatcher.Invoke(() =>
        {
            ScrollHeight = y - ActualHeight * 30;
        });


    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        CalculateScrollHeight();

    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        lock (_lock)
        {


            var iconWidth = 256;
            var iconHeight = 256;

            var marginX = 5;
            var marginY = 5;

            var x = marginX;
            var y = marginY;

            var maxWidth = ActualWidth;

            if (ImageEntries == null) return;


            foreach (var thumbView in _thumbViews)
            {
                if (y + thumbView.Height > ScrollTop && y < ScrollTop + ActualHeight && thumbView.BitmapSource != null)
                {
                    drawingContext.DrawImage(thumbView.BitmapSource, new Rect(new Point(x, y - ScrollTop), new Size(thumbView.Width, thumbView.Height)));
                }

                x += thumbView.Width + marginX;
                if (x + thumbView.Width >= ActualWidth)
                {
                    x = 0;
                    y += thumbView.Height + marginY;
                }
            }

        }

    }

    public void ScrollTo(double value)
    {
        ScrollTop = value;
        InvalidateVisual();
    }
}
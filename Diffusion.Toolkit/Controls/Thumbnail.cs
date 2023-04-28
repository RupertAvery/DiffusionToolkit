using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Diffusion.Toolkit.Controls;

public class Thumbnail : FrameworkElement
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(BitmapSource), typeof(Thumbnail),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(ImageEntry), typeof(Thumbnail),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var image = (ImageEntry)e.NewValue;
        if (image != null)
        {
            image.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(ImageEntry.ForDeletion):
                        var thumb = d as Thumbnail;
                        thumb.InvalidateVisual();
                        break;
                }
            };
        }
    }

    public BitmapSource? Source
    {
        get => (BitmapSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public ImageEntry? Data
    {
        get => (ImageEntry)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (Source == null)
            return;

        var aspectRatio = Source.Width / Source.Height;

        var factor = 1.0;

        if (aspectRatio > 1)
        {
            factor = Width / Source.Width;
        }
        else
        {
            factor = Height / Source.Height;
        }

        double width = Source.Width * factor;
        double height = Source.Height * factor;


        var rect = new Rect((Width - width) / 2, (Height - height) / 2, width, height);

        if (Data.ForDeletion)
        {
            drawingContext.PushOpacity(0.2);
        }

        drawingContext.DrawImage(Source, rect);

        if (Data.ForDeletion)
        {
            drawingContext.Pop();
        }
    }
}
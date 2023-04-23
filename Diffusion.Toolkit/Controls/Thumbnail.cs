using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit.Controls;

public class Thumbnail : FrameworkElement
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(BitmapSource), typeof(Thumbnail),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public BitmapSource Source
    {
        get => (BitmapSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
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

        drawingContext.DrawImage(Source, rect);
    }
}
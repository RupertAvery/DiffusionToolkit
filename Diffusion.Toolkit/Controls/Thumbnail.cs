using Diffusion.Toolkit.Localization;
using System.Globalization;
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


    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(Thumbnail),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        switch (e.Property.Name)
        {
            case nameof(Data):
                var image = (ImageEntry)e.NewValue;
                if (image != null)
                {
                    image.PropertyChanged += (sender, args) =>
                    {
                        var thumb = d as Thumbnail;
                        switch (args.PropertyName)
                        {
                            case nameof(ImageEntry.ForDeletion):
                                thumb.InvalidateVisual();
                                break;
                            case nameof(ImageEntry.Unavailable):
                                thumb.InvalidateVisual();
                                break;
                        }
                    };
                }
                break;
        }

    }

    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
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

    private string GetLocalizedText(string key)
    {
        return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (Data.Unavailable)
        {
            var formattedText = new FormattedText(GetLocalizedText("Thumbnail.Unavailable"),
                CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                12,
                Foreground,
                1
            );

            double centerX = (ActualWidth - formattedText.Width) / 2;
            double centerY = (ActualHeight - formattedText.Height) / 2;

            drawingContext.DrawText(formattedText, new Point(centerX, centerY));

            var margin = 5;

            var borderRect = new Rect(margin, margin, Width- margin, Height - margin);

            var pen = new Pen(Foreground, 1);

            drawingContext.DrawRoundedRectangle(Brushes.Transparent, pen, borderRect, 3, 3);


        }

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
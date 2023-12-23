using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Diffusion.Toolkit.Themes;

namespace Diffusion.Toolkit.Controls;

public class ThumbnailIcons : FrameworkElement
{
    private static BitmapImage? _starIcon;
    private static BitmapImage? _heartIcon;
    private static BitmapImage? _darkAlbumIcon;
    private static BitmapImage? _lightAlbumIcon;
    private static Typeface _typeFace = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(ImageEntry), typeof(ThumbnailIcons),
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
                    case nameof(ImageEntry.AlbumCount):
                    case nameof(ImageEntry.ForDeletion):
                    case nameof(ImageEntry.Favorite):
                    case nameof(ImageEntry.Rating):
                        var thumb = d as ThumbnailIcons;
                        thumb.InvalidateVisual();
                        break;
                }
            };
        }
    }

    public ImageEntry? Data
    {
        get => (ImageEntry)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    static ThumbnailIcons()
    {
        InitIcons();
    }

    private static void InitIcons()
    {
        Uri darkAlbumIconUri = new Uri("pack://application:,,,/Icons/Dark/gallery-32.png", UriKind.RelativeOrAbsolute);
        _darkAlbumIcon = new BitmapImage(darkAlbumIconUri);
        Uri lightAlbumIconUri = new Uri("pack://application:,,,/Icons/Light/gallery-32.png", UriKind.RelativeOrAbsolute);
        _lightAlbumIcon = new BitmapImage(lightAlbumIconUri);
        Uri heartIconUri = new Uri("pack://application:,,,/Icons/blue-heart-32.png", UriKind.RelativeOrAbsolute);
        _heartIcon = new BitmapImage(heartIconUri);
        Uri starIconUri = new Uri("pack://application:,,,/Icons/star-32.png", UriKind.RelativeOrAbsolute);
        _starIcon = new BitmapImage(starIconUri);
        //Unloaded += OnUnloaded;
    }


    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (Data == null)
            return;

        var x = 0;
        var y = 0;

        if (Data.AlbumCount > 0)
        {
            if (ThemeManager.CurrentTheme == "Dark")
            {
                drawingContext.DrawImage(_darkAlbumIcon, new Rect(new Point(x, y), new Size(24, 24)));
            }
            else if (ThemeManager.CurrentTheme == "Light")
            {
                drawingContext.DrawImage(_lightAlbumIcon, new Rect(new Point(x, y), new Size(24, 24)));
            }
            x += 24;
        }

        if (Data.Favorite)
        {
            drawingContext.DrawImage(_heartIcon, new Rect(new Point(x, y), new Size(24, 24)));
            x += 24;
        }

        if (Data.Rating.HasValue)
        {
            drawingContext.DrawImage(_starIcon, new Rect(new Point(x, y), new Size(24, 24)));
            var value = Data.Rating.Value.ToString();
            var formattedText = new FormattedText(value, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeFace, 14, Brushes.Black, null, TextFormattingMode.Display, 92)
            {
                TextAlignment = TextAlignment.Center
            };
            drawingContext.DrawText(formattedText, new Point(x + 16 - formattedText.WidthIncludingTrailingWhitespace / 2, y + 5));
            x += 24;
        }


    }
}
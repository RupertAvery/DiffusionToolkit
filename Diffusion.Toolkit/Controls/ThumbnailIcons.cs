using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Diffusion.Toolkit.Themes;

namespace Diffusion.Toolkit.Controls;

public class ThumbnailIcons : FrameworkElement
{
    private static BitmapImage? _starIcon;
    private static BitmapImage? _heartIcon;
    private static BitmapImage? _errorIcon;
    private static BitmapImage? _darkAlbumIcon;
    private static BitmapImage? _lightAlbumIcon;
    private static BitmapImage? _darkTrashIcon;
    private static BitmapImage? _lightTrashIcon;
    private static BitmapImage? _darkHideIcon;
    private static BitmapImage? _lightHideIcon;

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
                    case nameof(ImageEntry.NSFW):
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

    private static Uri GetUri(string path)
    {
        return new Uri($"pack://application:,,,{path}", UriKind.RelativeOrAbsolute);
    }

    private static void InitIcons()
    {
        Uri darkTrashIconUri = GetUri("/Icons/Dark/trash-32.png");
        _darkTrashIcon = new BitmapImage(darkTrashIconUri);
        Uri lightTrashIconUri = GetUri("/Icons/Light/trash-32.png");
        _lightTrashIcon = new BitmapImage(lightTrashIconUri);
        Uri darkAlbumIconUri = GetUri("/Icons/Dark/gallery-32.png");
        _darkAlbumIcon = new BitmapImage(darkAlbumIconUri);
        Uri lightAlbumIconUri = GetUri("/Icons/Light/gallery-32.png");
        _lightAlbumIcon = new BitmapImage(lightAlbumIconUri);
        Uri heartIconUri = GetUri("/Icons/blue-heart-32.png");
        _heartIcon = new BitmapImage(heartIconUri);
        Uri starIconUri = GetUri("/Icons/star-32.png");
        _starIcon = new BitmapImage(starIconUri);
        Uri darkHideIconUri = GetUri("/Icons/Dark/hide-24.png");
        _darkHideIcon = new BitmapImage(darkHideIconUri);
        Uri lightHideIconUri = GetUri("/Icons/Light/hide-24.png");
        _lightHideIcon = new BitmapImage(lightHideIconUri);
        Uri errorIconUri = GetUri("/Icons/error-32.png");
        _errorIcon = new BitmapImage(errorIconUri);
    }


    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (Data == null)
            return;

        var x = 0;
        var y = 0;
        const int xOffset = 22;

        //if (Data.ForDeletion)
        //{
        //    if (ThemeManager.CurrentTheme == "Dark")
        //    {
        //        drawingContext.DrawImage(_darkTrashIcon, new Rect(new Point(x, y), new Size(24, 24)));
        //    }
        //    else if (ThemeManager.CurrentTheme == "Light")
        //    {
        //        drawingContext.DrawImage(_lightTrashIcon, new Rect(new Point(x, y), new Size(24, 24)));
        //    }
        //    x += xOffset;
        //}
        if (Data.HasError)
        {
            drawingContext.DrawImage(_errorIcon, new Rect(new Point(x, y), new Size(24, 24)));
            x += xOffset;
        }


        if (Data.NSFW)
        {
            if (ThemeManager.CurrentTheme == "Dark")
            {
                drawingContext.DrawImage(_darkHideIcon, new Rect(new Point(x, y), new Size(24, 24)));
            }
            else if (ThemeManager.CurrentTheme == "Light")
            {
                drawingContext.DrawImage(_lightHideIcon, new Rect(new Point(x, y), new Size(24, 24)));
            }
            x += xOffset;
        }

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
            x += xOffset;
        }

        if (Data.Favorite)
        {
            drawingContext.DrawImage(_heartIcon, new Rect(new Point(x, y), new Size(24, 24)));
            x += xOffset;
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
            x += xOffset;
        }


    }
}
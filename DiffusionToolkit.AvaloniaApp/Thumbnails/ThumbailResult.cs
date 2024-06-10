using Avalonia.Media.Imaging;

namespace DiffusionToolkit.AvaloniaApp.Thumbnails;

public class ThumbailResult
{
    public bool Success { get; }
    public Bitmap? Image { get; }

    public ThumbailResult(Bitmap image)
    {
        Image = image;
        Success = true;
    }

    private ThumbailResult(bool success)
    {
        Success = success;
    }

    public static ThumbailResult FromBitmap(Bitmap bitmap)
    {
        return new ThumbailResult(bitmap);
    }

    public static ThumbailResult Failed => new ThumbailResult(false);

}
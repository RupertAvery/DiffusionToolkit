using Avalonia.Input;
using Avalonia.Interactivity;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public class DragStartEventArgs : RoutedEventArgs
{
    public DragStartEventArgs()
    {
        RoutedEvent = ThumbnailControl.DragStartEvent;
    }

    public PointerEventArgs PointerEventArgs { get; set; }
}
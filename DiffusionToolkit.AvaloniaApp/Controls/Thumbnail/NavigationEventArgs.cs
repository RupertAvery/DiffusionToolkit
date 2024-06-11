using Avalonia.Interactivity;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public class NavigationEventArgs : RoutedEventArgs
{
    public NavigationEventArgs()
    {
        RoutedEvent = ThumbnailControl.NavigationChangedEvent;
    }
    public NavigationState NavigationState { get; set; }
}
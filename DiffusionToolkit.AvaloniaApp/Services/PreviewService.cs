using System;
using Avalonia.Controls;
using DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

namespace DiffusionToolkit.AvaloniaApp.Services;

public class PreviewService
{
    private PreviewWindow? _previewWindow;

    private Window _owner;

    public void SetOwner(Window owner)
    {
        _owner = owner;
    }
    public void UpdatePreview(ThumbnailViewModel thumbnail)
    {
        if (_previewWindow != null)
        {
            _previewWindow.LoadImage(thumbnail);
        }
    }

    public void ShowPreview(ThumbnailViewModel thumbnail, bool fullScreen = false)
    {
        if (_previewWindow == null)
        {
            _previewWindow = new PreviewWindow();
            _previewWindow.Closed += PreviewWindowOnClosed;
            _previewWindow.LoadImage(thumbnail);
            _previewWindow.Show(_owner);

            if (fullScreen)
            {
                _previewWindow.FullScreen();
            }
        }
        else
        {
            _previewWindow.LoadImage(thumbnail);
        }
    }

    private void PreviewWindowOnClosed(object? sender, EventArgs e)
    {
        _previewWindow = null;
    }
}
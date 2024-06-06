using System;
using Avalonia.Controls;

namespace DiffusionToolkit.AvaloniaApp.Common;

public class PreviewManager
{
    private PreviewWindow? _previewWindow;

    private Window _owner;

    public void SetOwner(Window owner)
    {
        _owner = owner;
    }
    public void UpdatePreview(string path)
    {
        if (_previewWindow != null)
        {
            _previewWindow.LoadImage(path);
        }
    }

    public void ShowPreview(string path, bool fullScreen = false)
    {
        if (_previewWindow == null)
        {
            _previewWindow = new PreviewWindow();
            _previewWindow.Closed += PreviewWindowOnClosed;
            _previewWindow.LoadImage(path);
            _previewWindow.Show(_owner);

            if (fullScreen)
            {
                _previewWindow.WindowState = WindowState.FullScreen;
            }
        }
        else
        {
            _previewWindow.LoadImage(path);
        }
    }

    private void PreviewWindowOnClosed(object? sender, EventArgs e)
    {
        _previewWindow = null;
    }
}
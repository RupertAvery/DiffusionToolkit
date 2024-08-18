using System;
using System.Windows;
using Diffusion.Toolkit.Controls;

namespace Diffusion.Toolkit.Services;

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
            _previewWindow = new PreviewWindow(ServiceLocator.DataStore, ServiceLocator.MainModel);
            _previewWindow.Owner = _owner;
            _previewWindow.Closed += PreviewWindowOnClosed;
            _previewWindow.LoadImage(thumbnail);
            _previewWindow.Show();

            if (fullScreen)
            {
                _previewWindow.ShowFullScreen();
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
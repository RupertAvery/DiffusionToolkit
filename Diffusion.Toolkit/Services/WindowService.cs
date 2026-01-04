using System.Windows;

namespace Diffusion.Toolkit.Services;

public class WindowService
{
    private Window _window;

    public Window CurrentWindow => _window;

    public void SetWindow(Window window)
    {
        _window = window;
    }
}
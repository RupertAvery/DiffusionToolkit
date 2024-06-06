using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace DiffusionToolkit.AvaloniaApp;

public class Settings
{
    public IEnumerable<string> IncludedFolders { get; set; }
    public IEnumerable<string> ExcludedFolders { get; set; }
    public int IconSize { get; set; } = 256;
    public bool HideNSFW { get; set; } = true;
    public bool RecurseFolders { get; set; }
    public int PageSize { get; set; } = 250;
    public WindowPosition? Preview { get; set; } 
}

public class WindowPosition
{
    public WindowState WindowState { get; set; }
    public Size ClientSize { get; set; }
    public PixelPoint Position { get; set; }
    public Size MaxClientSize { get; set; }
    public PixelPoint MaxPosition { get; set; }
}
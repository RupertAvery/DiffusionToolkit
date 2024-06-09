using System;
using Avalonia.Media;
using Diffusion.Database;

namespace DiffusionToolkit.AvaloniaApp.ViewModels;

public class NavigationMenuItem : ViewModelBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Key { get; set; }
    public StreamGeometry Icon { get; set; }
    public bool IsDisabled { get; set; }
    public Action Action { get; set; }
}
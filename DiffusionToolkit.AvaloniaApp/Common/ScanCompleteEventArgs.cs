namespace DiffusionToolkit.AvaloniaApp.Common;

public class ScanCompleteEventArgs
{
    public bool Cancelled { get; set; }
    public string Message { get; set; }
    public int Added { get; set; }
    public int Scanned { get; set; }
    public int Removed { get; set; }
    public float ElapsedTime { get; set; }
}
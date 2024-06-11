namespace DiffusionToolkit.AvaloniaApp.Services;

public class ScanProgressEventArgs
{
    public string Message { get; set; }
    public int Progress { get; set; }
    public int Total { get; set; }
}
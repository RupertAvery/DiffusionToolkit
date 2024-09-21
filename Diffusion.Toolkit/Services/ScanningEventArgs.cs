namespace Diffusion.Toolkit.Services;

public class ScanningEventArgs
{
    public int TotalCount { get; set; }
    public int ProgressCount { get; set; }
    public ScanningEventType Type { get; set;}
    public string Message { get; set; }
    public int ElapsedTime { get; set; }
}
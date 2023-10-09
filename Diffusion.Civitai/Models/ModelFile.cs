namespace Diffusion.Civitai.Models;


public class ModelFile
{
    public int Id { get; set; }
    public double SizeKB { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public FileMetadata Metadata { get; set; }
    public string PickleScanResult { get; set; }
    public string PickleScanMessage { get; set; }
    public string VirusScanResult { get; set; }
    public string VirusScanMessage { get; set; }
    public DateTime? ScannedAt { get; set; }
    public Hashes Hashes { get; set; }
    public string DownloadUrl { get; set; }
    public bool Primary { get; set; }
}
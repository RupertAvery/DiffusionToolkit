namespace Diffusion.Toolkit.Services;

public class FolderChange
{
    public string Path { get; set; }
    public string NewPath { get; set; }
    public ChangeType ChangeType { get; set; }
}
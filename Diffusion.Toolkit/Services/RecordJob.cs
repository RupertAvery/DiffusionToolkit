using Diffusion.IO;

namespace Diffusion.Toolkit.Services;

public class RecordJob
{
    public FileParameters FileParameters { get; set; }
    public bool StoreMetadata { get; set; }
    public bool StoreWorkflow { get; set; }
}
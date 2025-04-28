using System.Collections.Generic;

namespace Diffusion.Toolkit.Configuration;

public interface IScanOptions
{
    string FileExtensions { get; set; }

    bool StoreMetadata { get; set; }

    bool StoreWorkflow { get; set; }
    bool ScanUnavailable { get; set; }
}
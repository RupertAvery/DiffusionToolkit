using System.Collections.Generic;

namespace Diffusion.Toolkit.Configuration;

public interface IScanOptions
{

    // List<string> ImagePaths { get; set; }

    List<string> ExcludePaths { get; set; }

    string FileExtensions { get; set; }

    bool StoreMetadata { get; set; }

    bool StoreWorkflow { get; set; }
    bool ScanUnavailable { get; set; }
}
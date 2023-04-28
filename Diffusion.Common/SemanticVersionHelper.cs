using System.IO;

namespace Diffusion.Common;

public class SemanticVersionHelper
{
    public static SemanticVersion GetLocalVersion(string? path = null)
    {
        var localVersion = new SemanticVersion();

        var versionPath = path == null? "version.txt" : Path.Combine(path, "version.txt");

        if (File.Exists("version.txt"))
        {
            SemanticVersion.TryParse(File.ReadAllText("version.txt"), out localVersion);
        }

        return localVersion;
    }
}
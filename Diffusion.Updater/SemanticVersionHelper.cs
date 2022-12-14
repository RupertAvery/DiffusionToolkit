namespace Diffusion.Updater;

public class SemanticVersionHelper
{
    public static SemanticVersion GetLocalVersion()
    {
        var localVersion = new SemanticVersion();

        if (File.Exists("version.txt"))
        {
            SemanticVersion.TryParse(File.ReadAllText("version.txt"), out localVersion);
        }

        return localVersion;
    }
}
using System.Collections.Generic;

namespace Diffusion.Toolkit;

public class Settings
{
    public Settings()
    {
        ImagePaths = new List<string>();
        FileExtensions = ".png, .jpg";
    }

    public List<string> ImagePaths { get; set; }
    public string ModelRootPath { get; set; }
    public string FileExtensions { get; set; }
}
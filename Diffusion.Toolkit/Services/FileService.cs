using System.IO;
using Diffusion.Common;

namespace Diffusion.Toolkit.Services;

public class FileService
{
    public void Delete(string path)
    {
        if (ServiceLocator.Settings.PermanentlyDelete)
        {
            File.Delete(path);
        }
        else
        {
            Win32FileAPI.Recycle(path);
        }
    }
}
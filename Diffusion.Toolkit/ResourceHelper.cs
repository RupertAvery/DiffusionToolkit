using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Diffusion.Toolkit;

public static class ResourceHelper
{
    public static string GetString(string resourcePath)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        using (Stream resourceStream = assembly.GetManifestResourceStream(resourcePath))
        {
            if (resourceStream == null)
            {
                throw new Exception(string.Format(
                    "Unable to find embedded resource with path of '{0}'.",
                    resourcePath));
            }

            using (StreamReader resourceReader = new StreamReader(resourceStream))
            {
                return resourceReader.ReadToEnd();
            }
        }
    }

    public static IEnumerable<string> GetResources(string resourcePath)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        return assembly.GetManifestResourceNames().Where(d=>d.StartsWith(resourcePath));
    }
}
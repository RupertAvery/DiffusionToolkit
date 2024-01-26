using System;
using System.IO;

namespace Diffusion.Common;

public static class AppInfo
{
    private const string AppName = "DiffusionToolkit";
    public static string AppDir { get; }
    public static SemanticVersion Version => SemanticVersionHelper.GetLocalVersion();
    public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiffusionToolkit");


    static AppInfo()
    {
        AppDir = AppDomain.CurrentDomain.BaseDirectory;

        if (AppDir.EndsWith("\\"))
        {
            AppDir = AppDir.Substring(0, AppDir.Length - 1);
        }
    }


}
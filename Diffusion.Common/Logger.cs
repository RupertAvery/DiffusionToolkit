using System;
using System.IO;

namespace Diffusion.Common;

public class Logger
{
    private static readonly object _lock = new object();

    public static void Log(string message)
    {
        lock (_lock)
        {
            File.AppendAllText("DiffusionToolkit.log", $"{DateTime.Now:G}: {message}\r\n");
        }
    }

    public static void Log(Exception exception)
    {
        lock (_lock)
        {
            File.AppendAllText("DiffusionToolkit.log", $"{DateTime.Now:G}: {exception}\r\n");
        }
    }
}
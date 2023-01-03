using System;
using System.IO;

public class Logger
{

    public static void Log(string message)
    {
        File.AppendAllText("DiffusionToolkit.log", $"{DateTime.Now:G}: {message}\r\n");
    }

}
using System;
using System.IO;

namespace Diffusion.Common;

public static class DragAndDrop
{
    public static string DragFiles = "DTDragFiles";
    public static string DragFolders= "DTDragFolders";
}

public static class FileUtility
{
    public static bool IsValidFilename(string filename)
    {
        string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "..", "." };

        if (Array.IndexOf(reservedNames, filename.ToUpper()) != -1)
        {
            return false;
        }

        char[] invalidChars = Path.GetInvalidFileNameChars();

        if (filename.IndexOfAny(invalidChars) != -1)
        {
            return false;
        }

        if (filename.Trim() != filename)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(filename))
        {
            return false;
        }

        return true;
    }
}
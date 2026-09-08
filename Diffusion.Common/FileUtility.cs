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
    public static (float ScaledSize, string FormattedSize) ToIECPrefix(long size)
    {
        float fsize = size;

        var ssize = $"{fsize:n} B";

        if (fsize > 1073741824)
        {
            fsize /= 1073741824;
            ssize = $"{fsize:n2} GiB";
        }
        else if (fsize > 1048576)
        {
            fsize /= 1048576;
            ssize = $"{fsize:n2} MiB";
        }
        else if (fsize > 1024)
        {
            fsize /= 1024;
            ssize = $"{fsize:n2} KiB";
        }

        return (fsize, ssize);
    }


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
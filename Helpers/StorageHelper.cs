using Avalonia.Platform.Storage;
using System;
using System.IO;

namespace PSPSuite.Helpers;

public static class StorageHelper
{
    public static string GetLocalPath(this IStorageItem item)
    {
        string? localPath = item.TryGetLocalPath();
        if (string.IsNullOrEmpty(localPath) && item.Path != null)
        {
            localPath = item.Path.IsAbsoluteUri ? item.Path.LocalPath : item.Path.OriginalString;
        }

        if (string.IsNullOrEmpty(localPath)) throw new FileNotFoundException("File not found or corrupted!");
        string unescapedPath = Uri.UnescapeDataString(localPath);
        try
        {
            return Path.GetFullPath(unescapedPath);
        }
        catch
        {
            return unescapedPath;
        }
    }
}
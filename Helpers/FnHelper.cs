using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Transactions;
using YoutubeDLSharp;

namespace PSPSuite.Helpers;

public static class FnHelper
{
    public static int GetOperatingSystemChosenFreePort()
    {
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            if (socket.LocalEndPoint is IPEndPoint ipEndPoint)
            {
                return ipEndPoint.Port;
            }

            return Constants.DEFAULT_INT_ERROR_VALUE;
        }
    }

    public static void DeleteFolderIfExists(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            var dir = new DirectoryInfo(folderPath);
            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                file.Attributes = FileAttributes.Normal;
            }

            foreach (var subDir in dir.GetDirectories("*", SearchOption.AllDirectories))
            {
                subDir.Attributes = FileAttributes.Normal;
            }

            dir.Attributes = FileAttributes.Normal;
            dir.Delete(recursive: true);
        }
    }


    public static void DeleteAllContent(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        var di = new DirectoryInfo(folderPath);
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }

        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete(true);
        }
    }


    public static void CopyDirectoryContents(string sourceDir, string targetDir)
    {
        if (!Directory.Exists(sourceDir)) return;
        Directory.CreateDirectory(targetDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(targetDir, fileName);
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(targetDir, dirName);
            CopyDirectoryContents(subDir, destSubDir);
        }
    }


    public static bool IsDirectoryEmpty(string path)
    {
        if (!Directory.Exists(path)) return true;
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }


    public static string GetDenoBinary()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "deno.exe";
        return "deno";
    }

    public static bool IsUrl(string uriName)
    {
        // Source - https://stackoverflow.com/a/7581824
        // Posted by Arabela Paslaru, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-09-10, License - CC BY-SA 3.0

        bool result = Uri.TryCreate(uriName, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        return result;
    }

    public static TimeSpan? FormatFloatToTimeSpan(float? value)
    {
        // Source - https://stackoverflow.com/a/45402292
        // Posted by jeanfrg, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-09-10, License - CC BY-SA 3.0
        if (!value.HasValue) return null;
        TimeSpan span = TimeSpan.FromSeconds((double)(new decimal(value.Value)));

        return span;
    }

    public static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        string normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();

        foreach (char c in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString()
                 .Normalize(System.Text.NormalizationForm.FormC)
                 .Replace('Đ', 'D')
                 .Replace('đ', 'd');
    }
}
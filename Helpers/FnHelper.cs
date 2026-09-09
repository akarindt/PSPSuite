using System.IO;
using System.Net;
using System.Net.Sockets;

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
        if(!Directory.Exists(folderPath)) return;

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
}
using PSPSuite.Data;
using PSPSuite.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PSPSuite.Dependencies;

public class Ytdlp : DependencyItem
{
    public override string DownloadUrl => "https://github.com/yt-dlp/yt-dlp";

    public override int Order => (int)Constants.DepsOrder.YTDLP;

    public override async Task DownloadItemAsync()
    {
        var savedDir = Path.Combine(Constants.DEPENDENCIES_FOLDER);
        if (!Directory.Exists(savedDir)) Directory.CreateDirectory(savedDir);

        if (!File.Exists(Path.Combine(savedDir, YoutubeDLSharp.Utils.FfmpegBinaryName))) await YoutubeDLSharp.Utils.DownloadFFmpeg(savedDir);
        Console.WriteLine("[Ytdlp_DownloadItemAsync]:: FFmpeg - Download success");

        if (!File.Exists(Path.Combine(savedDir, YoutubeDLSharp.Utils.FfprobeBinaryName))) await YoutubeDLSharp.Utils.DownloadFFprobe(savedDir);
        Console.WriteLine("[Ytdlp_DownloadItemAsync]:: FFprobe - Download success");

        if (!File.Exists(Path.Combine(savedDir, YoutubeDLSharp.Utils.YtDlpBinaryName))) await YoutubeDLSharp.Utils.DownloadYtDlp(savedDir);
        Console.WriteLine("[Ytdlp_DownloadItemAsync]:: yt-dlp - Download success");
    }

    public override async Task Init()
    {
        await Task.CompletedTask;
        Console.WriteLine("[Ytdlp_Init]::Success");
    }
}
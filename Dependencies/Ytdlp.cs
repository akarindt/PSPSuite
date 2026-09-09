using System;
using System.IO;
using System.Threading.Tasks;
using PSPSuite.Data;
using PSPSuite.Helpers;

namespace PSPSuite.Dependencies;

public class Ytdlp : DependencyItem
{
    public override string DownloadUrl => "https://github.com/yt-dlp/yt-dlp";

    public override int Order => (int)Constants.DepsOrder.YTDLP;

    public override async Task DownloadItemAsync()
    {
        var ytdlpPath = Path.Combine(Constants.DEPENDENCIES_FOLDER, "yt-dlp");
        if(!Directory.Exists(ytdlpPath)) Directory.CreateDirectory(ytdlpPath);

        await YoutubeDLSharp.Utils.DownloadFFmpeg(ytdlpPath);
        Console.WriteLine("[Ytdlp_DownloadItemAsync]:: FFmpeg - Download success");

        await YoutubeDLSharp.Utils.DownloadFFprobe(ytdlpPath);
        Console.WriteLine("[Ytdlp_DownloadItemAsync]:: FFprobe - Download success");

        await YoutubeDLSharp.Utils.DownloadYtDlp(ytdlpPath);
        Console.WriteLine("[Ytdlp_DownloadItemAsync]:: yt-dlp - Download success");

    }

    public override async Task ExecuteAsync() => await Task.CompletedTask;
}
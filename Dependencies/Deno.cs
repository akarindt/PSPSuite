using PSPSuite.Data;
using PSPSuite.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PSPSuite.Dependencies;

public class Deno : DependencyItem
{
    public override string DownloadUrl => "https://deno.com/";

    public override int Order => (int)Constants.DepsOrder.DENO;

    public override async Task DownloadItemAsync()
    {
        var savedDir = Constants.DEPENDENCIES_FOLDER;
        if (!File.Exists(Path.Combine(savedDir, FnHelper.GetDenoBinary()))) await YoutubeDLSharp.Utils.DownloadDeno(savedDir);
        Console.WriteLine("[Deno_DownloadItemAsync]::Deno - Download success");
    }

    public override async Task Init()
    {
        await Task.CompletedTask;
        Console.WriteLine("[Deno_Init]::Success");
    }
}
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using PSPSuite.Data;
using PSPSuite.Helpers;

namespace PSPSuite.Dependencies;

public class Deno : DependencyItem
{
    public override string DownloadUrl => "https://deno.com/";

    public override int Order => (int)Constants.DepsOrder.DENO;

    public override async Task DownloadItemAsync()
    {
        await YoutubeDLSharp.Utils.DownloadDeno(Constants.DEPENDENCIES_FOLDER);
        Console.WriteLine("[Deno_DownloadItemAsync]::Deno - Download success");
    }

    public override async Task ExecuteAsync() => await Task.CompletedTask;
}
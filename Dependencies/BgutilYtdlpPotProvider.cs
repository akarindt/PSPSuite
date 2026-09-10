using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using PSPSuite.Data;
using PSPSuite.Helpers;

namespace PSPSuite.Dependencies;

public class BgutilYtdlpPotProvider : DependencyItem
{
    public override string DownloadUrl => "https://github.com/Brainicism/bgutil-ytdlp-pot-provider.git";

    private string _baseDir = Constants.DEPENDENCIES_FOLDER;
    private string _folderName = "bgutil-ytdlp-pot-provider";
    private string _combinedPath => Path.Combine(_baseDir, _folderName);
    private string _repoPluginFolder => Path.Combine(_combinedPath, "plugin", "yt_dlp_plugins");
    private string _pluginFolder => Path.Combine(_baseDir, "yt-dlp-plugins", _folderName);
    private string _repoServerFolder => Path.Combine(_combinedPath, "server");
    private string _serverFolder => Path.Combine(_baseDir, $"{_folderName}-server");
    public override int Order => (int)Constants.DepsOrder.BGUTIL_YTDLP;
    private Process? _serverProcess;
    public override async Task DownloadItemAsync()
    {
        FnHelper.DeleteFolderIfExists(_combinedPath);
        if (!FnHelper.IsDirectoryEmpty(_pluginFolder) && !FnHelper.IsDirectoryEmpty(_serverFolder)) return;

        string baseDir = Constants.DEPENDENCIES_FOLDER;
        Console.WriteLine("[BgutilYtdlpPotProvider_DownloadItemAsync]::Pulling repo...");

        await Task.Run(async () =>
        {
            Repository.Clone(DownloadUrl, _combinedPath);

            if (!Directory.Exists(_combinedPath)) throw new Exception("Deps not found!");

            // Copy plugin to yt-dlp folder
            // Clear old files before copy new files
            FnHelper.DeleteAllContent(Path.Combine(_pluginFolder));
            FnHelper.CopyDirectoryContents(_repoPluginFolder, _pluginFolder);

            // Copy server files
            // Clear old files before copy new files
            FnHelper.DeleteAllContent(_serverFolder);
            FnHelper.CopyDirectoryContents(_repoServerFolder, _serverFolder);
        });

        FnHelper.DeleteFolderIfExists(_combinedPath);
        Console.WriteLine("[BgutilYtdlpPotProvider_DownloadItemAsync]::Success!");
    }

    public override async Task Init()
    {
        string baseDir = Constants.DEPENDENCIES_FOLDER;
        string denoPath = Path.Combine(baseDir, FnHelper.GetDenoBinary());
        
        if (FnHelper.IsDirectoryEmpty(Path.Combine(_serverFolder, "node_modules")))
        {
            using (var installProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = denoPath,
                    Arguments = "install --allow-scripts=npm:canvas --frozen",
                    WorkingDirectory = _serverFolder,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            })
            {
                installProcess.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null) Console.WriteLine($"[BgutilYtdlpPotProvider_Init]::Install process - {e.Data}");
                };

                installProcess.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null) Console.WriteLine($"[BgutilYtdlpPotProvider_Init]::Install process - {e.Data}");
                };

                installProcess.Exited += (s, e) =>
                {
                    Console.WriteLine("[BgutilYtdlpPotProvider_Init]::Install process - success");
                };

                installProcess.Start();
                installProcess.BeginErrorReadLine();
                installProcess.BeginOutputReadLine();
                await installProcess.WaitForExitAsync();
            }
        }

        StopServer();

        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = denoPath,
                Arguments = $"run --allow-env --allow-net --allow-ffi=. --allow-read=. ./src/main.ts -p {Constants.POT_SERVER_PORT}",
                WorkingDirectory = _serverFolder,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        _serverProcess.OutputDataReceived += (s, e) =>
           {
               if (e.Data != null) Console.WriteLine($"[BgutilYtdlpPotProvider_Init]::Server process - {e.Data}");
           };

        _serverProcess.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null) Console.WriteLine($"[BgutilYtdlpPotProvider_Init]::Server process - {e.Data}");
        };

        _serverProcess.Start();
        _serverProcess.BeginErrorReadLine();
        _serverProcess.BeginOutputReadLine();
    }


    private void StopServer()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            try
            {
                _serverProcess.Kill(entireProcessTree: true);
                _serverProcess.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BgutilYtdlpPotProvider]::Failed to kill server process: {ex.Message}");
            }
            finally
            {
                _serverProcess = null;
            }
        }
    }

    public override async Task Cleanup()
    {
        await base.Cleanup();
        StopServer();
    }
}
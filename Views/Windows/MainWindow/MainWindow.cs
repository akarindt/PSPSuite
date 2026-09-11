using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PSPSuite.Data;
using PSPSuite.Helpers;
using PSPSuite.Modules;
using YoutubeDLSharp;

namespace PSPSuite.Views.Windows;

public partial class MainWindow
{
    private readonly IDependencyLoader _depLoader;
    private readonly IEnumerable<GenericModule> _modules;

    public MainWindow(IDependencyLoader depLoader, IEnumerable<GenericModule> modules)
    {
        _drivePath.Text = UsbWatcher.CurrentPspPath;
        UsbWatcher.OnPspPathChanged += (path) =>
        {
            _drivePath.Text = path;
        };

        _depLoader = depLoader;
        _modules = modules;
    }

    protected override async Task InitAsync()
    {
        _queueSendBtn.Click += async (s, e) => await QueueSendBtn_Clicked(s, e);
        _browseBtn.Click += async (s, e) => await BrowseBtn_Clicked(s, e);
        _mainTabControl.LoadModule(_modules);

        Console.SetOut(new TextWriterExtend(text =>
        {
            Dispatcher.Post(() =>
            {
                _logTextBlock.Text += text;
                _logScrollViewer.ScrollToEnd();
            });
        }));

        UsbWatcher.InitUsbListener();
        this.SubscribeModuleEvents();

        await base.InitAsync();
        await _depLoader.LoadDepsAsync();

        this.Closed += Clean;
    }

    public async Task BrowseBtn_Clicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder",
            AllowMultiple = false
        });


        var folder = folders.FirstOrDefault();
        if (folder == null) return;
        _drivePath.Text = folder.Path.LocalPath;
    }

    public async Task QueueSendBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        if (_mainTabControl.SelectedContent is Music musicModule) await SendMusic();
    }

    private async Task SendMusic()
    {
        if (_queueListData.Count <= 0)
        {
            await MessageBox.Err("Error", "Queue is empty!");
            return;
        }

        if (_drivePath.Text == null || _drivePath.Text.Trim() == "")
        {
            await MessageBox.Err("Error", "Drive path is empty!");
            return;
        }

        if (!Directory.Exists(_drivePath.Text.Trim()))
        {
            await MessageBox.Err("Error", "Drive path not found!");
            return;
        }

        var localList = _queueListData.Where(x => x.IsLocal).ToList();
        var ytList = _queueListData.Where(x => !x.IsLocal).ToList();

        var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".local_music");
        var ytPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".yt_music");
        if (!Directory.Exists(localPath)) Directory.CreateDirectory(localPath);
        if (!Directory.Exists(ytPath)) Directory.CreateDirectory(ytPath);

        string ffmpegPath = Path.Combine(Constants.DEPENDENCIES_FOLDER, YoutubeDLSharp.Utils.FfmpegBinaryName);
        string ytdlpPath = Path.Combine(Constants.DEPENDENCIES_FOLDER, YoutubeDLSharp.Utils.YtDlpBinaryName);
        string outputFolder = _drivePath.Text.Trim().TrimEnd('\\');

        if (localList.Count > 0)
        {
            await Task.Run(async () =>
            {
                foreach (var file in localList)
                {
                    var tempFilePath = Path.Combine(localPath, Path.GetFileName(file.FilePath));
                    File.Copy(file.FilePath, tempFilePath, overwrite: true);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string powershellPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");
                    string script = @"
                        param(
                            [string]$Ffmpeg,
                            [string]$InputFolder,
                            [string]$OutputFolder
                        )

                        Get-ChildItem -LiteralPath $InputFolder -File |
                            Where-Object {
                                $_.Extension -in @('.mp3', '.wav', '.wma', '.aac', '.ogg', '.flac', '.m4a')
                            } |
                            ForEach-Object {
                                $output = Join-Path $OutputFolder ($_.BaseName + '.mp3')
                                & $Ffmpeg -y -i $_.FullName -map 0:a -map 0:v? -c:v copy -disposition:v attached_pic -b:a 192k $output
                            }
                    ";

                    using var localProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = powershellPath,
                            WorkingDirectory = localPath,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
                        }
                    };

                    localProcess.StartInfo.ArgumentList.Add("-NoProfile");
                    localProcess.StartInfo.ArgumentList.Add("-NonInteractive");
                    localProcess.StartInfo.ArgumentList.Add("-ExecutionPolicy");
                    localProcess.StartInfo.ArgumentList.Add("Bypass");
                    localProcess.StartInfo.ArgumentList.Add("-Command");

                    string psCommand = $"& {{ {script} }} -Ffmpeg '{ffmpegPath}' -InputFolder '{localPath}' -OutputFolder '{outputFolder}'";
                    localProcess.StartInfo.ArgumentList.Add(psCommand);

                    localProcess.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null) Console.WriteLine($"[MainWindow_SendMusic]::FFmpeg process - {e.Data}");
                    };

                    localProcess.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null) Console.WriteLine($"[MainWindow_SendMusic]::FFmpeg process - {e.Data}");
                    };

                    localProcess.Start();
                    localProcess.BeginOutputReadLine();
                    localProcess.BeginErrorReadLine();

                    await localProcess.WaitForExitAsync();
                    Console.WriteLine($"[MainWindow_SendMusic]::Done");
                }
            });
        }

        if (ytList.Count > 0)
        {
            string combinedLink = string.Join(" ", ytList.Select(x => $"\"{x.FilePath}\""));
            using (var ytProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytdlpPath,
                    WorkingDirectory = ytPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    Arguments = $"-f \"ba/b\" -x --audio-format mp3 --audio-quality 192K --embed-thumbnail --convert-thumbnails jpg --add-metadata -o \"{outputFolder}\\%(title)s.%(ext)s\" --no-overwrites --sleep-requests 1 --parse-metadata \"YT Music:%(album_artist)s\" --extractor-args \"youtubepot-bgutilhttp:base_url=http://127.0.0.1:{Constants.POT_SERVER_PORT}\" {combinedLink}"
                }
            })
            {
                ytProcess.OutputDataReceived += (s, e) =>
                   {
                       if (e.Data != null) Console.WriteLine($"[MainWindow_SendMusic]::FFmpeg process - {e.Data}");
                   };

                ytProcess.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null) Console.WriteLine($"[MainWindow_SendMusic]::FFmpeg process - {e.Data}");
                };

                ytProcess.Start();
                ytProcess.BeginOutputReadLine();
                ytProcess.BeginErrorReadLine();

                await ytProcess.WaitForExitAsync();
                Console.WriteLine($"[MainWindow_SendMusic]::Done");
            }
        }

        //Clean up


        return;
    }

    private void Clean(object? sender, EventArgs e)
    {
        this.Closed -= Clean;
        _ = _depLoader.Cleanup();
    }

    protected override void SubscribeModuleEvents()
    {
        foreach (var item in _mainTabControl.Items)
        {
            if (item is TabItem tab && tab.Content is GenericModule module)
            {
                module.SendToQueueRequested += OnSendToQueueRequested;
            }
        }
    }

    protected override void OnSendToQueueRequested(object? sender, IList items)
    {
        _queueList.ItemsSource = null;
        _queueListData.Clear();

        if (items is List<Audio> audioList)
        {
            foreach (var audio in audioList)
            {
                _queueListData.Add(new QueueItem
                {
                    FileName = audio.FileName,
                    FilePath = audio.FilePath,
                    FileType = QueueItemType.MUSIC,
                    Status = QueueItemStatus.READY,
                    IsLocal = audio.IsLocal
                });
            }
        }

        _queueList.ItemsSource = _queueListData;
    }
}
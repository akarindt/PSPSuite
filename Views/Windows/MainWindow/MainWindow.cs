using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PSPSuite.Data;
using PSPSuite.Helpers;
using PSPSuite.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSPSuite.Views.Windows;

public partial class MainWindow
{
    private readonly IDependencyLoader _depLoader;
    private readonly IEnumerable<GenericModule> _modules;

    public MainWindow(IDependencyLoader depLoader, IEnumerable<GenericModule> modules)
    {
        SetDrivePaths(UsbWatcher.CurrentPspPaths);
        UsbWatcher.OnPspPathChanged += SetDrivePaths;

        _depLoader = depLoader;
        _modules = modules;
    }

    private string DrivePath => _drivePath.SelectedItem as string ?? string.Empty;

    private void SetDrivePaths(string[] paths)
    {
        var selected = DrivePath;
        _drivePath.Items.Clear();
        foreach (var path in paths) _drivePath.Items.Add(path);
        _drivePath.SelectedItem = paths.Contains(selected) ? selected : paths.FirstOrDefault();
    }

    protected override async Task InitAsync()
    {
        _queueSendBtn.Click += async (s, e) => await QueueSendBtn_Clicked(s, e);
        _browseBtn.Click += async (s, e) => await BrowseBtn_Clicked(s, e);
        _cookiesBrowseBtn.Click += async (s, e) => await CookiesBrowseBtn_Clicked(s, e);
        _mainTabControl.LoadModule(_modules);
        _mainTabControl.SelectionChanged += MainTabControl_SelectionChanged;

        Console.SetOut(new TextWriterExtend(text =>
        {
            Dispatcher.Post(() =>
            {
                _logTextBlock.AppendText(text);
                _logTextBlock.CaretOffset = _logTextBlock.Document.TextLength;
                _logTextBlock.ScrollToEnd();
            });
        }));

        UsbWatcher.InitUsbListener();
        this.SubscribeModuleEvents();

        await _depLoader.LoadDepsAsync();
        this.Closed += Clean;
        await base.InitAsync();
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
        var picked = Uri.UnescapeDataString(folder.GetLocalPath());
        if (!_drivePath.Items.Contains(picked)) _drivePath.Items.Add(picked);
        _drivePath.SelectedItem = picked;
    }

    public async Task CookiesBrowseBtn_Clicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.OpenFilePickerWithResultAsync(new FilePickerOpenOptions
        {
            Title = "Select Cookies.txt",
            FileTypeFilter = [FilePickerFileTypes.TextPlain],
            AllowMultiple = false
        });

        if (file.Files.Count <= 0) return;
        _cookiesPath.Text = file.Files[0].GetLocalPath();
        GlobalVar.CookiesTxtFilePath = _cookiesPath.Text;
    }

    private async Task<bool> CheckSend()
    {
        if (_queueListData.Count <= 0)
        {
            await MessageBox.Err("Error", "Queue is empty!");
            return false;
        }

        if (DrivePath.Trim() == "")
        {
            await MessageBox.Err("Error", "Drive path is empty!");
            return false;
        }

        if (!Directory.Exists(DrivePath.Trim()))
        {
            await MessageBox.Err("Error", "Drive path not found!");
            return false;
        }

        return true;
    }

    public async Task QueueSendBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        if (_mainTabControl.SelectedContent is Music) await SendMusic();
        if (_mainTabControl.SelectedContent is Playlist) await SendPlaylist();
        if (_mainTabControl.SelectedContent is Video) await SendVideo();
    }

    private string GetOutputFolder() => DrivePath.Trim().TrimEnd('\\');

    private static bool IsCookiesFileValid(string? cookiesFile) => !string.IsNullOrWhiteSpace(cookiesFile) && File.Exists(cookiesFile);

    private void ShowCookiesNotFound() => Dispatcher.Post(async () => await MessageBox.Err("Error", "cookies.txt not found!"));

    private async Task SendVideo()
    {
        if (!await CheckSend()) return;

        var localList = _queueListData.Where(x => x.IsLocal).ToList();
        var ytList = _queueListData.Where(x => !x.IsLocal).ToList();
        string outputFolder = GetOutputFolder();
        string cookiesFile = GlobalVar.CookiesTxtFilePath;

        if (localList.Count > 0)
        {
            await Task.Run(() => ConvertLocalVideos(localList, outputFolder));
            Console.WriteLine($"[MainWindow_SendVideo]::Done");
        }

        if (ytList.Count <= 0) return;
        if (!IsCookiesFileValid(cookiesFile))
        {
            ShowCookiesNotFound();
            return;
        }

        await Task.Run(() => DownloadYtVideos(ytList, outputFolder, cookiesFile));
        Console.WriteLine($"[MainWindow_SendVideo]::YouTube Done");
    }

    private async Task SendMusic()
    {
        if (!await CheckSend()) return;

        var localList = _queueListData.Where(x => x.IsLocal).ToList();
        var ytList = _queueListData.Where(x => !x.IsLocal).ToList();
        string outputFolder = GetOutputFolder();
        string cookiesFile = GlobalVar.CookiesTxtFilePath;

        if (localList.Count > 0)
        {
            await Task.Run(() => ConvertLocalMusic(localList, outputFolder));
            Console.WriteLine($"[MainWindow_SendMusic]::Done");
        }

        if (ytList.Count <= 0) return;
        if (!IsCookiesFileValid(cookiesFile))
        {
            ShowCookiesNotFound();
            return;
        }

        await Task.Run(() => DownloadYtAudio(ytList.Select(x => x.FilePath), outputFolder, cookiesFile, "[MainWindow_SendMusic]::FFmpeg process"));
        Console.WriteLine($"[MainWindow_SendMusic]::Done");
    }

    private async Task SendPlaylist()
    {
        if (!await CheckSend()) return;
        if (!IsCookiesFileValid(GlobalVar.CookiesTxtFilePath))
        {
            await MessageBox.Err("Error", "cookies.txt not found!");
            return;
        }

        string outputFolder = GetOutputFolder();
        string cookiesFile = GlobalVar.CookiesTxtFilePath;
        var urls = _queueListData.Select(x => x.FilePath).ToList();

        await Task.Run(() => DownloadYtAudio(urls, outputFolder, cookiesFile, "[MainWindow_SendPlaylist]::YTDlp process"));
        Console.WriteLine($"[MainWindow_SendPlaylist]::Done");
    }

    private static void ConvertLocalVideos(List<QueueItem> localList, string outputFolder)
    {
        foreach (var file in localList)
        {
            if (!File.Exists(file.FilePath)) continue;
            Directory.CreateDirectory(Path.Combine(outputFolder, file.FolderName));

            string extension = Path.GetExtension(file.FilePath);
            string cleanFileName = FnHelper.NormalizeText(Path.GetFileNameWithoutExtension(file.FilePath)) + extension;
            string targetFilePath = Path.Combine(outputFolder, file.FolderName, cleanFileName);

            if (File.Exists(targetFilePath)) continue;
            if (!Constants.VIDEO_PATTERNS.Contains(extension)) continue;

            RunProcess(Constants.FFMPEG_PATH,
                $"-y -i \"{file.FilePath}\" -vf \"scale=480:272:force_original_aspect_ratio=decrease,pad=480:272:(ow-iw)/2:(oh-ih)/2\" -c:v libx264 -profile:v baseline -level 3.0 -pix_fmt yuv420p -b:v 768k -ar 44100 -ac 2 -b:a 128k \"{targetFilePath}\"",
                "[MainWindow_SendVideo]::FFmpeg process");
        }
    }

    private static void DownloadYtVideos(List<QueueItem> ytList, string outputFolder, string cookiesFile)
    {
        string tempYtDir = Path.Combine(outputFolder, "_temp_yt");
        Directory.CreateDirectory(tempYtDir);

        string linkTxtPath = Path.Combine(tempYtDir, "links.txt");
        File.WriteAllText(linkTxtPath, string.Join(Environment.NewLine, ytList.Select(x => x.FilePath)));

        string ytOutputFormat = Path.Combine(tempYtDir, "%(title)s.tmp.%(ext)s");
        RunProcess(Constants.YTDLP_PATH,
            $"-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\" " +
            $"--merge-output-format mp4 " +
            $"-a \"{linkTxtPath}\" " +
            $"--cookies \"{cookiesFile}\" " +
            $"--postprocessor-args \"ffmpeg:-c:v libx264 -profile:v baseline -level 3.0 -pix_fmt yuv420p -vf scale=480:272:force_original_aspect_ratio=decrease,pad=480:272:(ow-iw)/2:(oh-ih)/2 -b:v 768k -c:a aac -ac 2 -b:a 128k -ar 48000\" " +
            $"-o \"{ytOutputFormat}\" " +
            $"--sleep-requests 1 " +
            $"--extractor-args \"youtubepot-bgutilhttp:base_url=http://127.0.0.1:{Constants.POT_SERVER_PORT}\"",
            "[MainWindow_SendVideo]::YTDLP process");

        if (File.Exists(linkTxtPath)) File.Delete(linkTxtPath);
        MoveDownloadedVideos(ytList, tempYtDir, outputFolder);

        try
        {
            if (Directory.Exists(tempYtDir)) Directory.Delete(tempYtDir, true);
        }
        catch { }
    }

    private static void MoveDownloadedVideos(List<QueueItem> ytList, string tempYtDir, string outputFolder)
    {
        var tmpFiles = Directory.EnumerateFiles(tempYtDir, "*.tmp.mp4").ToList();
        for (int i = 0; i < tmpFiles.Count; i++)
        {
            string tmpFile = tmpFiles[i];
            string folderName = (i < ytList.Count) ? ytList[i].FolderName : ytList.Last().FolderName;
            string targetDir = Path.Combine(outputFolder, folderName);
            Directory.CreateDirectory(targetDir);

            string finalFilePath = Path.Combine(targetDir, GetCleanNameFromTmp(tmpFile) + ".mp4");
            if (File.Exists(finalFilePath))
            {
                File.Delete(tmpFile);
                continue;
            }
            File.Move(tmpFile, finalFilePath);
        }
    }

    private static void ConvertLocalMusic(List<QueueItem> localList, string outputFolder)
    {
        foreach (var file in localList)
        {
            string extension = Path.GetExtension(file.FilePath);
            string cleanFileName = FnHelper.NormalizeText(Path.GetFileNameWithoutExtension(file.FilePath)) + extension;
            string targetFilePath = Path.Combine(outputFolder, cleanFileName);

            if (File.Exists(targetFilePath)) continue;
            if (Constants.AUDIO_PATTERNS.Contains(extension)) continue;

            RunProcess(Constants.FFMPEG_PATH,
                $"-y -i \"{file.FilePath}\" -map 0:a -map 0:v? -c:v copy -disposition:v attached_pic -b:a 192k \"{targetFilePath}\"",
                "[MainWindow_SendMusic]::FFmpeg process");
            NormalizeTags(targetFilePath);
        }
    }

    private static void DownloadYtAudio(IEnumerable<string> urls, string outputFolder, string cookiesFile, string logPrefix)
    {
        string linkTxtPath = Path.Combine(outputFolder, "links.txt");
        File.WriteAllText(linkTxtPath, string.Join(Environment.NewLine, urls));

        string ytOutputFormat = Path.Combine(outputFolder, "%(title)s.tmp.%(ext)s");
        RunProcess(Constants.YTDLP_PATH,
            $"-f \"ba/b\" -i -x --audio-format mp3 -a \"{linkTxtPath}\" --cookies \"{cookiesFile}\" --audio-quality 192K --embed-thumbnail --convert-thumbnails jpg --ppa \"ThumbnailsConvertor+ffmpeg:-vf scale=-1:300,crop=300:300\" --add-metadata -o \"{ytOutputFormat}\" --sleep-requests 1 --parse-metadata \"YT Music:%(album_artist)s\" --extractor-args \"youtubepot-bgutilhttp:base_url=http://127.0.0.1:{Constants.POT_SERVER_PORT}\"",
            logPrefix);

        File.Delete(linkTxtPath);

        var tmpFiles = Directory.EnumerateFiles(outputFolder, "*.tmp.mp3").ToList();
        foreach (var tmpFile in tmpFiles)
        {
            string finalFilePath = Path.Combine(outputFolder, GetCleanNameFromTmp(tmpFile) + ".mp3");
            if (File.Exists(finalFilePath)) continue;

            NormalizeTags(tmpFile);
            File.Move(tmpFile, finalFilePath);
        }
    }

    // yt-dlp writes "<title>.tmp.<ext>"; drop the ".tmp" marker and normalize the title
    private static string GetCleanNameFromTmp(string tmpFile)
    {
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(tmpFile);
        return FnHelper.NormalizeText(fileNameWithoutExt[..^4]);
    }

    private static void NormalizeTags(string filePath)
    {
        using var tfile = TagLib.File.Create(filePath);
        if (!string.IsNullOrEmpty(tfile.Tag.Title)) tfile.Tag.Title = FnHelper.NormalizeText(tfile.Tag.Title);
        if (tfile.Tag.Performers?.Length > 0) tfile.Tag.Performers = tfile.Tag.Performers.Select(FnHelper.NormalizeText).ToArray();
        if (tfile.Tag.AlbumArtists?.Length > 0) tfile.Tag.AlbumArtists = tfile.Tag.AlbumArtists.Select(FnHelper.NormalizeText).ToArray();
        if (!string.IsNullOrEmpty(tfile.Tag.Album)) tfile.Tag.Album = FnHelper.NormalizeText(tfile.Tag.Album);
        tfile.Save();
    }

    private static void RunProcess(string fileName, string arguments, string logPrefix)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }
        };

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null) Console.WriteLine($"{logPrefix} - {e.Data}");
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null) Console.WriteLine($"{logPrefix} - {e.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }

    private void Clean(object? sender, EventArgs e)
    {
        this.Closed -= Clean;
        _ = _depLoader.Cleanup();
    }

    private void MainTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _queueListData.Clear();
        _queueList.ItemsSource = null;
        _queueList.ItemsSource = _queueListData;

        if (_mainTabControl.SelectedContent is Playlist or Music)
        {
            UsbWatcher.SetActiveCategory(PspContentCategory.MUSIC);
        }
        else if (_mainTabControl.SelectedContent is Video)
        {
            UsbWatcher.SetActiveCategory(PspContentCategory.VIDEO);
        }
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

        if (items is List<Media> mediaList)
        {
            foreach (var media in mediaList)
            {
                _queueListData.Add(new QueueItem
                {
                    FileName = media.FileName,
                    FilePath = media.FilePath,
                    IsLocal = media.IsLocal,
                    FolderName = media.FolderName
                });
            }
        }

        _queueList.ItemsSource = _queueListData;
    }
}

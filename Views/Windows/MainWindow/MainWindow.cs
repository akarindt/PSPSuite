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
        _drivePath.Text = folder.Path.LocalPath;
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
        _cookiesPath.Text = file.Files[0].Path.LocalPath;
        GlobalVar.CookiesTxtFilePath = _cookiesPath.Text;
    }

    private async Task<bool> CheckSend()
    {
        if (_queueListData.Count <= 0)
        {
            await MessageBox.Err("Error", "Queue is empty!");
            return false;
        }

        if (_drivePath.Text == null || _drivePath.Text.Trim() == "")
        {
            await MessageBox.Err("Error", "Drive path is empty!");
            return false;
        }

        if (!Directory.Exists(_drivePath.Text.Trim()))
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
    }

    private async Task SendMusic()
    {
        if (!await CheckSend()) return;

        var localList = _queueListData.Where(x => x.IsLocal).ToList();
        var ytList = _queueListData.Where(x => !x.IsLocal).ToList();

        string ffmpegPath = Path.Combine(Constants.DEPENDENCIES_FOLDER, YoutubeDLSharp.Utils.FfmpegBinaryName);
        string ytdlpPath = Path.Combine(Constants.DEPENDENCIES_FOLDER, YoutubeDLSharp.Utils.YtDlpBinaryName);

#pragma warning disable CS8602
        string outputFolder = _drivePath.Text.Trim().TrimEnd('\\');
        string cookiesFile = GlobalVar.CookiesTxtFilePath;
#pragma warning restore CS8602

        if (localList.Count > 0)
        {
            await Task.Run(() =>
            {
                foreach (var file in localList)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FilePath);
                    string extension = Path.GetExtension(file.FilePath);
                    string cleanFileName = FnHelper.NormalizeText(fileNameWithoutExt) + extension;
                    string targetFilePath = Path.Combine(outputFolder, cleanFileName);

                    if (File.Exists(targetFilePath)) continue;
                    var isAudioOnly = !Constants.AUDIO_PATTERNS.Contains(extension);
                    if (!isAudioOnly) continue;


                    var ffmpegArgs = $"-y -i \"{file.FilePath}\" -map 0:a -map 0:v? -c:v copy -disposition:v attached_pic -b:a 192k \"{targetFilePath}\"";
                    using var ffmpegProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = ffmpegPath,
                            Arguments = ffmpegArgs,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
                        }
                    };

                    ffmpegProcess.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null) Console.WriteLine($"[MainWindow_SendMusic]::FFmpeg process - {e.Data}");
                    };

                    ffmpegProcess.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null) Console.WriteLine($"[MainWindow_SendMusic]::FFmpeg process - {e.Data}");
                    };

                    ffmpegProcess.Start();
                    ffmpegProcess.BeginOutputReadLine();
                    ffmpegProcess.BeginErrorReadLine();
                    ffmpegProcess.WaitForExit();

                    using (var tfile = TagLib.File.Create(targetFilePath))
                    {
                        if (!string.IsNullOrEmpty(tfile.Tag.Title)) tfile.Tag.Title = FnHelper.NormalizeText(tfile.Tag.Title);
                        if (tfile.Tag.Performers?.Length > 0) tfile.Tag.Performers = tfile.Tag.Performers.Select(FnHelper.NormalizeText).ToArray();
                        if (tfile.Tag.AlbumArtists?.Length > 0) tfile.Tag.AlbumArtists = tfile.Tag.AlbumArtists.Select(FnHelper.NormalizeText).ToArray();
                        if (!string.IsNullOrEmpty(tfile.Tag.Album)) tfile.Tag.Album = FnHelper.NormalizeText(tfile.Tag.Album);
                        tfile.Save();
                    }
                }

                Console.WriteLine($"[MainWindow_SendMusic]::Done");
            });
        }

        if (ytList.Count > 0)
        {
            await Task.Run(() =>
            {
                if (!File.Exists(GlobalVar.CookiesTxtFilePath) || GlobalVar.CookiesTxtFilePath == null || GlobalVar.CookiesTxtFilePath.Trim() == "")
                {
                    Dispatcher.Post(async () => await MessageBox.Err("Error", "cookies.txt not found!"));
                    return;
                }

                string linkTxtPath = Path.Combine(outputFolder, "links.txt");
                File.WriteAllText(linkTxtPath, string.Join(Environment.NewLine, ytList.Select(x => x.FilePath)));

                string ytOutputFormat = Path.Combine(outputFolder, "%(title)s.tmp.%(ext)s");
                using var ytProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ytdlpPath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        Arguments = $"-f \"ba/b\" -i -x --audio-format mp3 -a \"{linkTxtPath}\" --cookies \"{cookiesFile}\" --audio-quality 192K --embed-thumbnail --convert-thumbnails jpg --ppa \"ThumbnailsConvertor+ffmpeg:-vf scale=-1:300,crop=300:300\" --add-metadata -o \"{ytOutputFormat}\" --sleep-requests 1 --parse-metadata \"YT Music:%(album_artist)s\" --extractor-args \"youtubepot-bgutilhttp:base_url=http://127.0.0.1:{Constants.POT_SERVER_PORT}\""
                    }
                };

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

                ytProcess.WaitForExit();

                File.Delete(linkTxtPath);

                var tmpFiles = Directory.EnumerateFiles(outputFolder, "*.tmp.mp3").ToList();
                foreach (var tmpFile in tmpFiles)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(tmpFile);
                    string cleanFileName = fileNameWithoutExt.Substring(0, fileNameWithoutExt.Length - 4);
                    cleanFileName = FnHelper.NormalizeText(cleanFileName);
                    string finalFileName = cleanFileName + ".mp3";
                    string finalFilePath = Path.Combine(outputFolder, finalFileName);

                    if (File.Exists(finalFilePath)) continue;

                    using (var tfile = TagLib.File.Create(tmpFile))
                    {
                        if (!string.IsNullOrEmpty(tfile.Tag.Title)) tfile.Tag.Title = FnHelper.NormalizeText(tfile.Tag.Title);
                        if (tfile.Tag.Performers?.Length > 0) tfile.Tag.Performers = tfile.Tag.Performers.Select(FnHelper.NormalizeText).ToArray();
                        if (tfile.Tag.AlbumArtists?.Length > 0) tfile.Tag.AlbumArtists = tfile.Tag.AlbumArtists.Select(FnHelper.NormalizeText).ToArray();
                        if (!string.IsNullOrEmpty(tfile.Tag.Album)) tfile.Tag.Album = FnHelper.NormalizeText(tfile.Tag.Album);
                        tfile.Save();
                    }

                    File.Move(tmpFile, finalFilePath);
                }

                Console.WriteLine($"[MainWindow_SendMusic]::Done");
            });
        }

        return;
    }

    private async Task SendPlaylist()
    {
        if (!await CheckSend()) return;

        string ytdlpPath = Path.Combine(Constants.DEPENDENCIES_FOLDER, YoutubeDLSharp.Utils.YtDlpBinaryName);

#pragma warning disable CS8602
        string outputFolder = _drivePath.Text.Trim().TrimEnd('\\');
        string cookiesFile = GlobalVar.CookiesTxtFilePath;
#pragma warning restore CS8602

        if (!File.Exists(GlobalVar.CookiesTxtFilePath) || GlobalVar.CookiesTxtFilePath == null || GlobalVar.CookiesTxtFilePath.Trim() == "")
        {
            await MessageBox.Err("Error", "cookies.txt not found!");
            return;
        }

        string linkTxtPath = Path.Combine(outputFolder, "links.txt");
        File.WriteAllText(linkTxtPath, string.Join(Environment.NewLine, _queueListData.Select(x => x.FilePath)));

        await Task.Run(() =>
        {
            string ytOutputFormat = Path.Combine(outputFolder, "%(title)s.tmp.%(ext)s");

            using var ytProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytdlpPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    Arguments = $"-f \"ba/b\" -i -x --audio-format mp3 -a \"{linkTxtPath}\" --cookies \"{cookiesFile}\" --audio-quality 192K --embed-thumbnail --convert-thumbnails jpg --ppa \"ThumbnailsConvertor+ffmpeg:-vf scale=-1:300,crop=300:300\" --add-metadata -o \"{ytOutputFormat}\" --sleep-requests 1 --parse-metadata \"YT Music:%(album_artist)s\" --extractor-args \"youtubepot-bgutilhttp:base_url=http://127.0.0.1:{Constants.POT_SERVER_PORT}\""
                }
            };

            ytProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null) Console.WriteLine($"[MainWindow_SendPlaylist]::FFmpeg process - {e.Data}");
            };

            ytProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null) Console.WriteLine($"[MainWindow_SendPlaylist]::FFmpeg process - {e.Data}");
            };

            ytProcess.Start();
            ytProcess.BeginOutputReadLine();
            ytProcess.BeginErrorReadLine();

            ytProcess.WaitForExit();

            File.Delete(linkTxtPath);

            var tmpFiles = Directory.EnumerateFiles(outputFolder, "*.tmp.mp3").ToList();
            foreach (var tmpFile in tmpFiles)
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(tmpFile);
                string cleanFileName = fileNameWithoutExt.Substring(0, fileNameWithoutExt.Length - 4);
                cleanFileName = FnHelper.NormalizeText(cleanFileName);
                string finalFileName = cleanFileName + ".mp3";
                string finalFilePath = Path.Combine(outputFolder, finalFileName);

                if (File.Exists(finalFilePath)) continue;

                using (var tfile = TagLib.File.Create(tmpFile))
                {
                    if (!string.IsNullOrEmpty(tfile.Tag.Title)) tfile.Tag.Title = FnHelper.NormalizeText(tfile.Tag.Title);
                    if (tfile.Tag.Performers?.Length > 0) tfile.Tag.Performers = tfile.Tag.Performers.Select(FnHelper.NormalizeText).ToArray();
                    if (tfile.Tag.AlbumArtists?.Length > 0) tfile.Tag.AlbumArtists = tfile.Tag.AlbumArtists.Select(FnHelper.NormalizeText).ToArray();
                    if (!string.IsNullOrEmpty(tfile.Tag.Album)) tfile.Tag.Album = FnHelper.NormalizeText(tfile.Tag.Album);
                    tfile.Save();
                }

                File.Move(tmpFile, finalFilePath);
            }

            Console.WriteLine($"[MainWindow_SendPlaylist]::Done");
        });
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
                    FileType = QueueItemType.MUSIC,
                    Status = QueueItemStatus.READY,
                    IsLocal = media.IsLocal
                });
            }
        }

        _queueList.ItemsSource = _queueListData;
    }
}
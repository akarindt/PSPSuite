using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace PSPSuite.Modules;

public partial class Music
{
    private ManagementEventWatcher? _usbWatcherWin;
    private FileSystemWatcher? _usbWatcherUnix;
    public Music() { }

    public async Task OnBrowseFolderClick(object? sender, RoutedEventArgs e)
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

    private void InitUsbListener()
    {
#if DEBUG
        Console.WriteLine("[Music_InitUsbListener]::Function called");
#endif
        CheckAndSetPspPath();
        if (OperatingSystem.IsWindows())
        {
            StartWindowsUsbWatcher();
            return;
        }


        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            StartUnixUsbWatcher();
            return;
        }
    }

    [SupportedOSPlatform("windows")]
    private void StartWindowsUsbWatcher()
    {
        var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");
        _usbWatcherWin = new ManagementEventWatcher(query);

        _usbWatcherWin.EventArrived += (sender, e) => Dispatcher.Post(CheckAndSetPspPath);
        _usbWatcherWin.Start();
    }


    private void StartUnixUsbWatcher()
    {
        // IDK this since i'm not using Linux or Mac
    }

    private void CheckAndSetPspPath()
    {
        var removableDrives = DriveInfo.GetDrives()
            .Where(d => (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed) && d.IsReady);

        foreach (var drive in removableDrives)
        {
            string pspMusicPath = Path.Combine(drive.RootDirectory.FullName, "PSP", "MUSIC");
            string rootMusicPath = Path.Combine(drive.RootDirectory.FullName, "MUSIC");

            if (Directory.Exists(pspMusicPath))
            {
                _drivePath.Text = pspMusicPath;
                return;
            }

            if (Directory.Exists(rootMusicPath))
            {
                _drivePath.Text = rootMusicPath;
                return;
            }
        }

        _drivePath.Text = string.Empty;
    }

    public void Cleanup()
    {
        if (OperatingSystem.IsWindows() && _usbWatcherWin != null)
        {
            _usbWatcherWin.Stop();
            _usbWatcherWin.Dispose();
        }

        if (_usbWatcherUnix != null)
        {
            _usbWatcherUnix.EnableRaisingEvents = false;
            _usbWatcherUnix.Dispose();
        }

#if DEBUG
        Console.WriteLine("[Music_CleanUpUsbWatcher]::Dispose called");
#endif
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Cleanup();
        base.OnDetachedFromVisualTree(e);
    }

}
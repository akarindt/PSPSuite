using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace PSPSuite.Helpers;

public static class UsbWatcher
{
    private static bool _isInitialized;
    private static ManagementEventWatcher? _usbWatcherWin;
    private static CancellationTokenSource? _unixCts;
    public static event Action<string>? OnPspPathChanged;
    public static string CurrentPspPath { get; private set; } = string.Empty;
    private static readonly int DEFAULT_DELAY = 3000;

    public static void InitUsbListener()
    {
#if DEBUG
        Console.WriteLine($"[UsbWatcher_InitUsbListener]::Initialized: {_isInitialized}");
#endif
        if (_isInitialized) return;

        UpdateAndNotifyPath();

        if (OperatingSystem.IsWindows())
        {
            StartWindowsUsbWatcher();
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            StartUnixUsbWatcher();
        }

        AppDomain.CurrentDomain.ProcessExit += (s, e) => Cleanup();
        _isInitialized = true;

#if DEBUG
        Console.WriteLine($"[UsbWatcher_InitUsbListener]::Initialize success!");
#endif
    }

    [SupportedOSPlatform("windows")]
    private static void StartWindowsUsbWatcher()
    {
        var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");
        _usbWatcherWin = new ManagementEventWatcher(query);

        _usbWatcherWin.EventArrived += (sender, e) => Dispatcher.UIThread.Post(UpdateAndNotifyPath);
        _usbWatcherWin.Start();
    }

    private static void StartUnixUsbWatcher()
    {
        _unixCts = new CancellationTokenSource();
        var token = _unixCts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                Dispatcher.UIThread.Post(UpdateAndNotifyPath);
                await Task.Delay(DEFAULT_DELAY, token);
            }
        }, token);
    }

    private static void UpdateAndNotifyPath()
    {
#if DEBUG
        Console.WriteLine($"[UsbWatcher_UpdateAndNotifyPath]::Function called!");
#endif

        var newPath = CheckAndGetPspPath();
        if (CurrentPspPath != newPath)
        {
            CurrentPspPath = newPath;
            OnPspPathChanged?.Invoke(CurrentPspPath);
        }
    }

    public static string CheckAndGetPspPath()
    {
        var removableDrives = DriveInfo.GetDrives()
            .Where(d => (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed) && d.IsReady);

        foreach (var drive in removableDrives)
        {
            string pspMusicPath = Path.Combine(drive.RootDirectory.FullName, "PSP", "MUSIC");
            string rootMusicPath = Path.Combine(drive.RootDirectory.FullName, "MUSIC");

            if (Directory.Exists(pspMusicPath)) return pspMusicPath;
            if (Directory.Exists(rootMusicPath)) return rootMusicPath;
        }

        return string.Empty;
    }

    public static void Cleanup()
    {
#if DEBUG
        Console.WriteLine($"[UsbWatcher_Cleanup]::Cleanup started: {_isInitialized}");
#endif
        if (!_isInitialized) return;

        if (OperatingSystem.IsWindows() && _usbWatcherWin != null)
        {
            _usbWatcherWin.Stop();
            _usbWatcherWin.Dispose();
            _usbWatcherWin = null;
        }

        if (_unixCts != null)
        {
            _unixCts.Cancel();
            _unixCts.Dispose();
            _unixCts = null;
        }

        _isInitialized = false;

#if DEBUG
        Console.WriteLine($"[UsbWatcher_Cleanup]::Cleanup success!");
#endif
    }
}
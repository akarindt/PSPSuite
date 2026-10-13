using Avalonia.Threading;
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Usb.Events;

namespace PSPSuite.Helpers;

public enum PspContentCategory
{
    MUSIC = 0,
    VIDEO = 1,
}

public static class UsbWatcher
{
    private static bool _isInitialized;
    private static IUsbEventWatcher? _usbEventWatcher;
    public static event Action<string>? OnPspPathChanged;
    public static string CurrentPspPath { get; private set; } = string.Empty;
    public static PspContentCategory ActiveCategory { get; private set; } = PspContentCategory.MUSIC;
    public static void InitUsbListener()
    {
#if DEBUG
        Console.WriteLine($"[UsbWatcher_InitUsbListener]::Initialized: {_isInitialized}");
#endif
        if (_isInitialized) return;

        UpdateAndNotifyPath(ActiveCategory);
        StartUsbWatcher();

        AppDomain.CurrentDomain.ProcessExit += (s, e) => Cleanup();
        _isInitialized = true;

#if DEBUG
        Console.WriteLine($"[UsbWatcher_InitUsbListener]::Initialize success!");
#endif
    }

    private static void StartUsbWatcher()
    {
        _usbEventWatcher = new UsbEventWatcher();
        _usbEventWatcher.UsbDeviceAdded += (_, _) => Dispatcher.UIThread.Post(() => UpdateAndNotifyPath(ActiveCategory));
        _usbEventWatcher.UsbDeviceRemoved += (_, _) => Dispatcher.UIThread.Post(() => UpdateAndNotifyPath(ActiveCategory));
        _usbEventWatcher.Start();
    }

    public static void SetActiveCategory(PspContentCategory category)
    {
        if (ActiveCategory != category)
        {
            ActiveCategory = category;
            UpdateAndNotifyPath(category);
        }
    }

    private static void UpdateAndNotifyPath(PspContentCategory category)
    {
#if DEBUG
        Console.WriteLine($"[UsbWatcher_UpdateAndNotifyPath]::Category={category}");
#endif

        var newPath = CheckAndGetPspPath(category);
        if (CurrentPspPath != newPath)
        {
            CurrentPspPath = newPath;
            OnPspPathChanged?.Invoke(CurrentPspPath);
        }
    }

    public static string CheckAndGetPspPath(PspContentCategory category)
    {
        var removableDrives = DriveInfo.GetDrives()
            .Where(d => (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed) && d.IsReady);

        foreach (var drive in removableDrives)
        {
            string basePath = drive.RootDirectory.FullName;

            if (category == PspContentCategory.MUSIC)
            {
                string pspMusicPath = Path.Combine(basePath, "PSP", "MUSIC");
                string rootMusicPath = Path.Combine(basePath, "MUSIC");

                if (Directory.Exists(pspMusicPath)) return pspMusicPath;
                if (Directory.Exists(rootMusicPath)) return rootMusicPath;
            }
            else if (category == PspContentCategory.VIDEO)
            {
                string pspVideoPath = Path.Combine(basePath, "PSP", "VIDEO");
                string rootVideoPath = Path.Combine(basePath, "VIDEO");

                if (Directory.Exists(pspVideoPath)) return pspVideoPath;
                if (Directory.Exists(rootVideoPath)) return rootVideoPath;
            }
        }

        return string.Empty;
    }

    public static void Cleanup()
    {
#if DEBUG
        Console.WriteLine($"[UsbWatcher_Cleanup]::Cleanup started: {_isInitialized}");
#endif
        if (!_isInitialized) return;

        if (_usbEventWatcher != null)
        {
            _usbEventWatcher.Dispose();
        }

        _isInitialized = false;

#if DEBUG
        Console.WriteLine($"[UsbWatcher_Cleanup]::Cleanup success!");
#endif
    }
}
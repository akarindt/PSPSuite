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
    public static event Action<string[]>? OnPspPathChanged;
    public static string[] CurrentPspPaths { get; private set; } = [];
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

        var newPaths = CheckAndGetPspPaths(category);
        if (!CurrentPspPaths.SequenceEqual(newPaths))
        {
            CurrentPspPaths = newPaths;
            OnPspPathChanged?.Invoke(CurrentPspPaths);
        }
    }

    public static string[] CheckAndGetPspPaths(PspContentCategory category)
    {
        var removableDrives = DriveInfo.GetDrives()
            .Where(d => (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed) && d.IsReady);

        string folder = category == PspContentCategory.VIDEO ? "VIDEO" : "MUSIC";

        // One path per drive: PSP\<folder> preferred over root <folder>
        return removableDrives
            .Select(d => d.RootDirectory.FullName)
            .Select(basePath => new[] { Path.Combine(basePath, "PSP", folder), Path.Combine(basePath, folder) }.FirstOrDefault(Directory.Exists))
            .OfType<string>()
            .ToArray();
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
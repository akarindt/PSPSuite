using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PSPSuite.Helpers;

namespace PSPSuite.Views.Windows;

public partial class MainWindow : GenericWindow
{
    public MainWindow()
    {
        _drivePath.Text = UsbWatcher.CurrentPspPath;
        UsbWatcher.OnPspPathChanged += (path) =>
        {
            _drivePath.Text = path;
        };
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
}
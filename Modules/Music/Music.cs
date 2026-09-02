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
using PSPSuite.Helpers;

namespace PSPSuite.Modules;

public partial class Music
{
    public Music()
    {

    }

    public async Task AddLocalBtn_Clicked(object? sender, RoutedEventArgs agrs)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open music files",
            AllowMultiple = true,
            FileTypeFilter = [Constants.FILE_TYPE_AUDIO_ALL]
        });

        if(files.Count <= 0) return;
        
    }
}

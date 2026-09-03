using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PSPSuite.Data;
using PSPSuite.Helpers;

namespace PSPSuite.Modules;

public partial class Music
{
    public Music()
    {

    }

    public void ToQueueBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        var selectedAudios = _audioList.Where(a => a.IsChecked).ToList();
        if (selectedAudios.Count <= 0) return;
        RaiseSendToQueue(selectedAudios);
    }

    public void ClearListBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        _itemsRepeater.ItemsSource = null;
        _audioList.Clear();
        _itemsRepeater.ItemsSource = _audioList;
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

        if (files.Count <= 0) return;

        var fileInfos = new Audio[files.Count];
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        await Parallel.ForEachAsync(Enumerable.Range(0, files.Count), parallelOptions, async (i, ct) =>
        {
            var file = files[i];
            var path = file.Path.LocalPath;
            var basicInfo = await file.GetBasicPropertiesAsync();

            var audioItem = await Task.Run(() =>
            {
                string title = file.Name;
                string artist = "Unknown Artist";
                string album = "Unknown Album";
                TimeSpan duration = TimeSpan.Zero;

                try
                {
                    using var tagFile = TagLib.File.Create(path);

                    duration = tagFile.Properties.Duration;
                    if (!string.IsNullOrWhiteSpace(tagFile.Tag.Title)) title = tagFile.Tag.Title;
                    if (tagFile.Tag.JoinedPerformers?.Length > 0) artist = tagFile.Tag.JoinedPerformers;
                    if (!string.IsNullOrWhiteSpace(tagFile.Tag.Album)) album = tagFile.Tag.Album;
                }
                catch
                {

                }

                return new Audio
                {
                    FileName = title,
                    FilePath = file.Path.LocalPath,
                    Duration = duration,
                    ContributeArtist = artist,
                    Album = album,
                    IsLocal = true,
                    Size = basicInfo.Size,
                    DateCreated = basicInfo.DateCreated,
                    DateModified = basicInfo.DateModified,
                    IsChecked = false
                };
            }, ct);

            file.Dispose();
            fileInfos[i] = audioItem;
        });

        _audioList.Clear();
        foreach (var audio in fileInfos)
        {
            if (audio != null) _audioList.Add(audio);
        }

        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(generation: 2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
    }
}

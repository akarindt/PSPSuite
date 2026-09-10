using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PSPSuite.Data;
using PSPSuite.Helpers;
using PSPSuite.Views.Components;
using YoutubeDLSharp;

namespace PSPSuite.Modules;

public partial class Music
{
    public Music()
    {

    }

    private void ToQueueBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        var selectedAudios = _audioList.Where(a => a.IsChecked).ToList();
        if (selectedAudios.Count <= 0) return;
        RaiseSendToQueue(selectedAudios);
    }

    private void ClearListBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        _itemsRepeater.ItemsSource = null;
        _audioList.Clear();
        _itemsRepeater.ItemsSource = _audioList;
    }

    private async Task AddLocalBtn_Clicked(object? sender, RoutedEventArgs agrs)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open music files",
            AllowMultiple = true,
            FileTypeFilter = [Constants.FILE_TYPE_AUDIO_ALL]
        });

        _selectAllCheckBox.IsChecked = false;
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

        var currentPaths = new HashSet<string>(_audioList.Select(x => x.FilePath), StringComparer.OrdinalIgnoreCase);
        var uniqueList = fileInfos
            .Where(info => !currentPaths.Contains(info.FilePath))
            .ToList();

        foreach (var audio in uniqueList)
        {
            if (audio != null) _audioList.Add(audio);
        }

        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(generation: 2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
    }

    private void ItemsRepeater_ElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is AudioItemControl control && _audioList != null && args.Index < _audioList.Count)
        {
            var currentAudio = _audioList[args.Index];
            control.DataContext = currentAudio;
            control.AudioItem = currentAudio;
        }
    }

    private void SelectAllCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (_audioList.Count <= 0) return;
        foreach (var audio in _audioList)
        {
            audio.IsChecked = _selectAllCheckBox.IsChecked ?? false;
        }
    }

    private async Task SearchBtn_Clicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_urlTextBox.Text)) return;

        string url = _urlTextBox.Text.Trim();
        if (!FnHelper.IsUrl(url)) return;

        Cursor = new Cursor(StandardCursorType.Wait);
        try
        {
            var ytdlp = new YoutubeDL();
            var options = new YoutubeDLSharp.Options.OptionSet();
            options.AddCustomOption<string>("--extractor-args", $"youtubepot-bgutilhttp:base_url=http://127.0.0.1:{Constants.POT_SERVER_PORT}");
            options.AddCustomOption<string>("--sleep-requests", "1");

            var result = await ytdlp.RunVideoDataFetch(url, overrideOptions: options);
            if (!result.Success)
            {
                await MessageBox.Err("Error", "Cannot fetch data");
                return;
            }

            var data = result.Data;
            _audioList.Add(new Audio
            {
                FileName = data.Title,
                FilePath = data.Url,
                DateCreated = data.UploadDate,
                DateModified = data.ModifiedDate,
                Size = null,
                Duration = FnHelper.FormatFloatToTimeSpan(data.Duration),
                ContributeArtist = data.AlbumArtist,
                Album = data.Album,
                IsLocal = false,
                IsChecked = false
            });

            return;
        }
        finally
        {
            Cursor = Cursor.Default;
        }
    }


    protected override async Task InitAsync()
    {
        _clearListBtn.Click += ClearListBtn_Clicked;
        _addLocalBtn.Click += async (s, e) => await AddLocalBtn_Clicked(s, e);
        _toQueueBtn.Click += ToQueueBtn_Clicked;
        _itemsRepeater.ElementPrepared += ItemsRepeater_ElementPrepared;
        _selectAllCheckBox.IsCheckedChanged += SelectAllCheckBox_IsCheckedChanged;
        _searchBtn.Click += async (s, e) => await SearchBtn_Clicked(s, e);

        await base.InitAsync();

    }
}

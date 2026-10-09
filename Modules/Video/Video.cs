using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PSPSuite.Data;
using PSPSuite.Helpers;
using PSPSuite.Views.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using YoutubeDLSharp;

namespace PSPSuite.Modules;

public partial class Video
{
    public Video()
    {

    }

    private void ToQueueBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        var selectedVideos = _videoList.Where(v => v.IsChecked).ToList();
        if (selectedVideos.Count <= 0) return;
        RaiseSendToQueue(selectedVideos);
    }

    private void ClearListBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        _itemsRepeater.ItemsSource = null;
        _videoList.Clear();
        _itemsRepeater.ItemsSource = _videoList;
    }

    private void SelectAllCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (_videoList.Count <= 0) return;
        foreach (var video in _videoList)
        {
            video.IsChecked = _selectAllCheckBox.IsChecked ?? false;
        }
    }

    private bool IsPlaylistMode() => _typeComboBox.SelectedIndex == 1;

    private async Task SearchBtn_Clicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(GlobalVar.CookiesTxtFilePath)) return;
        if (string.IsNullOrEmpty(_urlTextBox.Text)) return;

        string unfilteredUrl = _urlTextBox.Text.Trim();
        if (!FnHelper.IsUrl(unfilteredUrl)) return;

        var uri = new Uri(unfilteredUrl);
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

        Cursor = new Cursor(StandardCursorType.Wait);
        try
        {
            var ytdlp = new YoutubeDL();
            var options = new YoutubeDLSharp.Options.OptionSet();
            options.AddCustomOption<string>("--cookies", GlobalVar.CookiesTxtFilePath);
            options.AddCustomOption<string>("--extractor-args", $"youtubepot-bgutilhttp:base_url=http://127.0.0.1:{Constants.POT_SERVER_PORT}");
            options.AddCustomOption<string>("--sleep-requests", "1");

            string buildUrl(string? param, string fallback)
            {
                if (param == null || param.Trim() == "") return fallback;
                return $"{fallback}={param}";
            }

            if (IsPlaylistMode())
            {
                string? listId = queryParams["list"];
                string url = buildUrl(listId, "https://www.youtube.com/playlist?list");
                if (url.Contains("="))
                {
                    var result = await ytdlp.RunVideoDataFetch(url, overrideOptions: options);
                    if (!result.Success)
                    {
                        await MessageBox.Err("Error", result.ErrorOutput.First());
                        return;
                    }

                    _videoList.Clear();
                    _itemsRepeater.ItemsSource = null;
                    _itemsRepeater.ItemsSource = _videoList;

                    var playlistData = result.Data;
                    foreach (var entry in playlistData.Entries)
                    {
                        _videoList.Add(new Media
                        {
                            FileName = entry.Title,
                            FilePath = entry.Url,
                            DateCreated = entry.UploadDate,
                            DateModified = entry.ModifiedDate,
                            Size = null,
                            Duration = FnHelper.FormatFloatToTimeSpan(entry.Duration),
                            ContributeArtist = entry.AlbumArtist,
                            Album = entry.Album,
                            FolderName = FnHelper.CleanTitleForFolder(entry.Title),
                            IsLocal = false,
                            IsChecked = false
                        });
                    }
                }
            }
            else
            {
                string? vId = queryParams["v"];
                string url = buildUrl(vId, "https://www.youtube.com/watch?v");
                if (url.Contains("="))
                {
                    var result = await ytdlp.RunVideoDataFetch(url, overrideOptions: options);
                    if (!result.Success)
                    {
                        await MessageBox.Err("Error", result.ErrorOutput.First());
                        return;
                    }

                    var data = result.Data;
                    _videoList.Add(new Media
                    {
                        FileName = data.Title,
                        FilePath = url,
                        DateCreated = data.UploadDate,
                        DateModified = data.ModifiedDate,
                        Size = null,
                        Duration = FnHelper.FormatFloatToTimeSpan(data.Duration),
                        ContributeArtist = data.AlbumArtist,
                        Album = data.Album,
                        FolderName = FnHelper.CleanTitleForFolder(data.Title),
                        IsLocal = false,
                        IsChecked = false
                    });
                }
            }

            return;
        }
        finally
        {
            Cursor = Avalonia.Input.Cursor.Default;
        }
    }

    private void ItemsRepeater_ElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is MediaItemControl control && _videoList != null && args.Index < _videoList.Count)
        {
            var currentMedia = _videoList[args.Index];
            control.DataContext = currentMedia;
            control.MediaItem = currentMedia;
        }
    }

    private async Task AddLocalBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open video files",
            AllowMultiple = true,
            FileTypeFilter = [Constants.FILE_TYPE_VIDEO_ALL]
        });

        _selectAllCheckBox.IsChecked = false;
        if (files.Count <= 0) return;

        var fileInfos = new Media[files.Count];
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        await Parallel.ForEachAsync(Enumerable.Range(0, files.Count), parallelOptions, async (i, ct) =>
        {
            var file = files[i];
            var path = file.GetLocalPath();
            var basicInfo = await file.GetBasicPropertiesAsync();

            var videoItem = await Task.Run(() =>
            {
                string title = file.Name;
                TimeSpan duration = TimeSpan.Zero;
                ulong? size = basicInfo.Size;

                try
                {
                    using var tagFile = TagLib.File.Create(path);
                    duration = tagFile.Properties.Duration;
                    if (!string.IsNullOrWhiteSpace(tagFile.Tag.Title)) title = tagFile.Tag.Title;
                }
                catch
                {

                }

                return new Media
                {
                    FileName = title,
                    FilePath = file.GetLocalPath(),
                    Duration = duration,
                    ContributeArtist = string.Empty,
                    Album = string.Empty,
                    IsLocal = true,
                    Size = size,
                    DateCreated = basicInfo.DateCreated,
                    DateModified = basicInfo.DateModified,
                    IsChecked = false,
                    FolderName = FnHelper.GetFolderOrDefault(file.GetLocalPath())
                };
            }, ct);

            file.Dispose();
            fileInfos[i] = videoItem;
        });

        var currentPaths = new HashSet<string>(_videoList.Select(x => x.FilePath), StringComparer.OrdinalIgnoreCase);
        var uniqueList = fileInfos
            .Where(info => !currentPaths.Contains(info.FilePath))
            .ToList();

        foreach (var video in uniqueList)
        {
            if (video != null) _videoList.Add(video);
        }

        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(generation: 2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
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

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PSPSuite.Data;
using PSPSuite.Helpers;
using PSPSuite.Views.Components;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace PSPSuite.Modules;

public partial class Playlist
{
    public Playlist() { }

    private void SelectAllCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (_audioList.Count <= 0) return;
        foreach (var audio in _audioList)
        {
            audio.IsChecked = _selectAllCheckBox.IsChecked ?? false;
        }
    }

    private void ItemsRepeater_ElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is MediaItemControl control && _audioList != null && args.Index < _audioList.Count)
        {
            var currentMedia = _audioList[args.Index];
            control.DataContext = currentMedia;
            control.MediaItem = currentMedia;
        }
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

    private async Task SearchBtn_Clicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(GlobalVar.CookiesTxtFilePath)) return;
        if (string.IsNullOrEmpty(_urlTextBox.Text)) return;


        string unfilteredUrl = _urlTextBox.Text.Trim();
        if (!FnHelper.IsUrl(unfilteredUrl)) return;

        var uri = new Uri(unfilteredUrl);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        string? listId = queryParams["list"];
        if(listId == null || listId.Trim() == "") return;

        string url = $"https://www.youtube.com/playlist?list={listId}";
        Cursor = new Cursor(StandardCursorType.Wait);
        try
        {
            var ytdlp = new YoutubeDL();
            var options = new YoutubeDLSharp.Options.OptionSet();
            options.AddCustomOption<string>("--cookies", GlobalVar.CookiesTxtFilePath);
            options.AddCustomOption<string>("--extractor-args", $"youtubepot-bgutilhttp:base_url=http://127.0.0.1:{Constants.POT_SERVER_PORT}");
            options.AddCustomOption<string>("--sleep-requests", "1");

            var result = await ytdlp.RunVideoDataFetch(url, overrideOptions: options);
            if (!result.Success)
            {
                await MessageBox.Err("Error", result.ErrorOutput.First());
                return;
            }

            _audioList.Clear();
            _itemsRepeater.ItemsSource = null;
            _itemsRepeater.ItemsSource = _audioList;

            VideoData playlistData = result.Data;
            var a = playlistData.Entries.Count();
            foreach(var entry in playlistData.Entries)
            {
                _audioList.Add(new Media
                {
                    FileName = entry.Title,
                    FilePath = entry.Url,
                    DateCreated = entry.UploadDate,
                    DateModified = entry.ModifiedDate,
                    Size = null,
                    Duration = FnHelper.FormatFloatToTimeSpan(entry.Duration),
                    ContributeArtist = entry.AlbumArtist,
                    Album = entry.Album,
                    IsLocal = false,
                    IsChecked = false
                });
            }
        }
        finally
        {
            Cursor = Cursor.Default;
        }
    }

    protected override async Task InitAsync()
    {
        _clearListBtn.Click += ClearListBtn_Clicked;
        _toQueueBtn.Click += ToQueueBtn_Clicked;
        _itemsRepeater.ElementPrepared += ItemsRepeater_ElementPrepared;
        _selectAllCheckBox.IsCheckedChanged += SelectAllCheckBox_IsCheckedChanged;
        _searchBtn.Click += async (s, e) => await SearchBtn_Clicked(s, e);

        await base.InitAsync();
    }
}
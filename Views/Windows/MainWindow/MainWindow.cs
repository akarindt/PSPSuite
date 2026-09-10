using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PSPSuite.Data;
using PSPSuite.Helpers;
using PSPSuite.Modules;

namespace PSPSuite.Views.Windows;

public partial class MainWindow
{
    private readonly IDependencyLoader _depLoader;
    private readonly IEnumerable<GenericModule> _modules;

    public MainWindow(IDependencyLoader depLoader, IEnumerable<GenericModule> modules)
    {
        _drivePath.Text = UsbWatcher.CurrentPspPath;
        UsbWatcher.OnPspPathChanged += (path) =>
        {
            _drivePath.Text = path;
        };

        _depLoader = depLoader;
        _modules = modules;
    }

    protected override async Task InitAsync()
    {
        _queueSendBtn.Click += async (s, e) => await QueueSendBtn_Clicked(s, e);
        _browseBtn.Click += async (s, e) => await BrowseBtn_Clicked(s, e);
        _mainTabControl.LoadModule(_modules);

        Console.SetOut(new TextWriterExtend(text =>
        {
            Dispatcher.Post(() =>
            {
                _logTextBlock.Text += text;
                _logScrollViewer.ScrollToEnd();
            });
        }));

        UsbWatcher.InitUsbListener();
        this.SubscribeModuleEvents();

        await base.InitAsync();
        await _depLoader.LoadDepsAsync();

        this.Closed += Clean;
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

    public async Task QueueSendBtn_Clicked(object? sender, RoutedEventArgs args)
    {
        if (_queueListData.Count <= 0)
        {
            await MessageBox.Err("Error", "Queue is empty!");
            return;
        }
    }

    private void Clean(object? sender, EventArgs e)
    {
        this.Closed -= Clean;
        _ = _depLoader.Cleanup();
    }

    protected override void SubscribeModuleEvents()
    {
        foreach (var item in _mainTabControl.Items)
        {
            if (item is TabItem tab && tab.Content is GenericModule module)
            {
                module.SendToQueueRequested += OnSendToQueueRequested;
            }
        }
    }

    protected override void OnSendToQueueRequested(object? sender, IList items)
    {
        _queueList.ItemsSource = null;
        _queueListData.Clear();
        _queueList.ItemsSource = _queueListData;

        if (items is List<Audio> audioList)
        {
            foreach (var audio in audioList)
            {
                _queueListData.Add(new QueueItem
                {
                    FileName = audio.FileName,
                    FilePath = audio.FilePath,
                    FileType = QueueItemType.MUSIC,
                    Status = QueueItemStatus.READY,
                    IsLocal = audio.IsLocal
                });
            }
        }
    }
}
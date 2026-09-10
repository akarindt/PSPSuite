using System;
using System.Collections;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PSPSuite.Helpers;

namespace PSPSuite.Modules;

public abstract class GenericModule : UserControl
{
    public event EventHandler<IList>? SendToQueueRequested;

    public abstract void BuildUI();

    protected GenericModule()
    {
        Loaded += OnModuleLoaded;
    }

    protected void RaiseSendToQueue(IList items)
    {
        SendToQueueRequested?.Invoke(this, items);
    }

    private async void OnModuleLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnModuleLoaded;

        BuildUI();
        Init();
        await InitAsync();
    }

    protected virtual async Task InitAsync() => await Task.CompletedTask;
    protected virtual void Init() { }
}
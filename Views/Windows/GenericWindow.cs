using Avalonia.Controls;
using Avalonia.Interactivity;
using PSPSuite.Helpers;
using System.Collections;
using System.Threading.Tasks;

namespace PSPSuite.Views.Windows;

public abstract class GenericWindow : Window
{
    protected GenericWindow()
    {
        Title = Constants.APP_NAME;
        MinWidth = Constants.MAIN_SCREEN_W;
        MinHeight = Constants.MAIN_SCREEN_H;
        RequestedThemeVariant = Constants.REQUESTED_THEME_VARIANT;
        Background = Constants.PRIMARY_BACKGROUND_COLOR;

        WindowState = WindowState.Maximized;
        Loaded += OnWindowLoaded;

#if DEBUG
        Loaded += (s, e) => HotReload.OnCodeUpdated += ReloadUI;
        Unloaded += (s, e) => HotReload.OnCodeUpdated -= ReloadUI;
#endif
    }

#if DEBUG
    private void ReloadUI()
    {
        _ = Dispatcher.InvokeAsync(async () =>
        {
            Content = null;
            BuildUI();

            Init();
            await InitAsync();
        });
    }
#endif

    private async void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnWindowLoaded;

        BuildUI();
        Init();
        await InitAsync();
    }

    protected abstract void BuildUI();
    protected abstract void SubscribeModuleEvents();
    protected abstract void OnSendToQueueRequested(object? sender, IList items);
    protected virtual async Task InitAsync() => await Task.CompletedTask;
    protected virtual void Init() { }
}
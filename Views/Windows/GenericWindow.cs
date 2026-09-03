using System.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using PSPSuite.Helpers;

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
        Loaded += (s, e) => BuildUI();

#if DEBUG
        Loaded += (s, e) => HotReload.OnCodeUpdated += ReloadUI;
        Unloaded += (s, e) => HotReload.OnCodeUpdated -= ReloadUI;
#endif
    }

#if DEBUG
    private void ReloadUI()
    {
        _ = Dispatcher.InvokeAsync(() =>
        {
            Content = null;
            BuildUI();
        });
    }
#endif

    public abstract void BuildUI();
    public abstract void SubscribeModuleEvents();
    public abstract void OnSendToQueueRequested(object? sender, IList items);
}
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
        
        Loaded += (s, e) => BuildUI();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        HotReload.OnCodeUpdated += ReloadUI;
    }

    private void ReloadUI()
    {
        _ = Dispatcher.InvokeAsync(() =>
        {
            Content = null;
            BuildUI();
        });
    }

    public abstract void BuildUI();
}
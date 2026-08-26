using Avalonia.Media;
using Avalonia.Styling;

namespace PSPSuite.Helper;

public static class Constants
{
    public static readonly string APP_NAME = "PSPSuite";
    public static readonly int MAIN_SCREEN_W = 1270;
    public static readonly int MAIN_SCREEN_H = 800;
    public static readonly ThemeVariant REQUESTED_THEME_VARIANT = ThemeVariant.Dark;
    public static readonly SolidColorBrush BACKGROUND_COLOR = new(Color.Parse("#1E1E1E"));
}
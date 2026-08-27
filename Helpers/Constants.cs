using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace PSPSuite.Helpers;

public static class Constants
{
    #region Window configs
    public static readonly string APP_NAME = "PSPSuite";
    public static readonly int MAIN_SCREEN_W = 1270;
    public static readonly int MAIN_SCREEN_H = 800;

    #endregion

    #region Colors
    public static readonly ThemeVariant REQUESTED_THEME_VARIANT = ThemeVariant.Dark;
    public static readonly SolidColorBrush PRIMARY_BACKGROUND_COLOR = new(Color.Parse("#252525"));
    public static readonly SolidColorBrush CONTAINER_BACKGROUND_COLOR = new(Color.Parse("#1A1A1A"));
    public static readonly SolidColorBrush PRIMARY_TEXT_COLOR = new(Color.Parse("#CCCCCC"));
    public static readonly SolidColorBrush PRIMARY_BUTTON_COLOR = new(Color.Parse("#0E639C"));
    public static readonly SolidColorBrush PRIMARY_HOVER_COLOR = new(Color.Parse("#007ACC"));

    #endregion

    #region Styles
    public static readonly CornerRadius DEFAULT_CORNER_RADIUS = new(12);
    public static readonly GridLength DEFAULT_SPLITTER_COL_LENGTH = new(5);
    public static readonly GridLength DEFAULT_SPLITTER_ROW_LENGTH = new(5);
    public static readonly GridLength DEFAULT_COL_LENGTH = new(300);
    public static readonly GridLength DEFAULT_ROW_LENGTH = new(200);
    public static readonly Thickness DEFAULT_PADDING = new(10);
    public static readonly Thickness DEFAULT_DIVIDER_PADDING = new(0, 5);
    public static readonly Thickness DEFAULT_MARGIN = new (0, 5);
    public static readonly double DEFAULT_THICKNESS = 1;
    #endregion
}
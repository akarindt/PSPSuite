using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PSPSuite.Attributes;
using PSPSuite.Helpers;
using Avalonia.Layout;

namespace PSPSuite.Modules;

[TabModule("Music", (int)Constants.MODULE_ORDER.MUSIC)]
public partial class Music : GenericModule
{
    private Grid _pathContainer = new();
    private TextBox _drivePath = new();
    private Button _browseBtn = new();
    private Grid _urlContainer = new();
    private TextBox _urlTextBox = new();
    private Button _searchBtn = new();

    public override void BuildUI()
    {
        _pathContainer = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(new GridLength(50)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(100)),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Auto),
            ],
            ColumnSpacing = 10,
            Width = 800,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        _drivePath = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            IsReadOnly = true,
            Focusable = false,
            IsTabStop = false,
            IsHitTestVisible = false,
            Background = Constants.PRIMARY_BACKGROUND_COLOR
        };

        _browseBtn = new Button
        {
            Content = "Browse",
            Width = 100,
            Background = Constants.PRIMARY_BUTTON_COLOR,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Resources =
            {
                ["ButtonBackgroundPointerOver"] = Constants.PRIMARY_HOVER_COLOR,
                ["ButtonBackgroundPressed"] = Constants.PRIMARY_HOVER_COLOR
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        _browseBtn.Click += async (s, e) => await OnBrowseFolderClick(s, e);

        _pathContainer.Children.AddChildren(new TextBlock { Text = "Drive", VerticalAlignment = VerticalAlignment.Center }, 0, 0);
        _pathContainer.Children.AddChildren(_drivePath, 1, 0);
        _pathContainer.Children.AddChildren(_browseBtn, 2, 0);


        _urlContainer = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(new GridLength(50)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(100)),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Auto),
            ],
            ColumnSpacing = 10,
            Width = 800,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        _urlTextBox = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
            PlaceholderText = "Paste Youtube URL..."
        };

        _searchBtn = new Button
        {
            Content = "Search",
            Width = 100,
            Background = Constants.PRIMARY_BUTTON_COLOR,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Resources =
            {
                ["ButtonBackgroundPointerOver"] = Constants.PRIMARY_HOVER_COLOR,
                ["ButtonBackgroundPressed"] = Constants.PRIMARY_HOVER_COLOR
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        _urlContainer.Children.AddChildren(new TextBlock { Text = "Url", VerticalAlignment = VerticalAlignment.Center }, 0, 0);
        _urlContainer.Children.AddChildren(_urlTextBox, 1, 0);
        _urlContainer.Children.AddChildren(_searchBtn, 2, 0);

        DockPanel.SetDock(_pathContainer, Dock.Top);
        DockPanel.SetDock(_urlContainer, Dock.Top);

        Content = new Border
        {
            Child = new DockPanel
            {
                Children =
                {
                    _pathContainer,
                    _urlContainer
                },
                VerticalSpacing = 10
            },
            Padding = Constants.DEFAULT_PADDING
        };

        InitUsbListener();
    }
}

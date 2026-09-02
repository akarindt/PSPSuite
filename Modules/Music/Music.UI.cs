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
    private Grid _urlContainer = new();
    private TextBox _urlTextBox = new();
    private Button _searchBtn = new();
    private Button _addLocalBtn = new();
    private Grid _buttonGrid = new();

    public override void BuildUI()
    {
        _urlContainer = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(new GridLength(50)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Star),
            ],
            ColumnSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Width = double.NaN
        };

        _urlTextBox = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
            PlaceholderText = "Paste Youtube URL..."
        };

        _buttonGrid = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Star),
            ],
            ColumnSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        _searchBtn = new Button
        {
            Content = "Search",
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

        _addLocalBtn = new Button
        {
            Content = "Add local music",
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

        Grid.SetColumn(_searchBtn, 0);
        Grid.SetColumn(_addLocalBtn, 1);
        _buttonGrid.Children.Add(_searchBtn);
        _buttonGrid.Children.Add(_addLocalBtn);

        var label = new TextBlock { Text = "Url", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(label, 0);
        Grid.SetColumn(_urlTextBox, 1);
        Grid.SetColumn(_buttonGrid, 2);

        _urlContainer.Children.Add(label);
        _urlContainer.Children.Add(_urlTextBox);
        _urlContainer.Children.Add(_buttonGrid);

        DockPanel.SetDock(_urlContainer, Dock.Top);

        Content = new Border
        {
            Child = new DockPanel
            {
                Children =
                {
                    _urlContainer
                },
                VerticalSpacing = 10
            },
            Padding = Constants.DEFAULT_PADDING
        };
    }
}
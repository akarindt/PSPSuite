using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PSPSuite.Attributes;
using PSPSuite.Helpers;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using PSPSuite.Data;
using Avalonia.Controls.Templates;
using PSPSuite.Views.Components;

namespace PSPSuite.Modules;

[TabModule("Music", (int)Constants.MODULE_ORDER.MUSIC)]
public partial class Music : GenericModule
{

    private Grid _urlContainer = new();
    private TextBox _urlTextBox = new();
    private Button _searchBtn = new();
    private Button _addLocalBtn = new();
    private Grid _buttonGrid = new();
    private Grid _clearSendBtnGrid = new();
    private readonly ObservableCollection<Audio> _audioList = new();
    private ItemsRepeater _itemsRepeater = new();
    private ScrollViewer _scrollViewer = new();
    private Button _toQueueBtn = new();
    private Button _clearListBtn = new();
    private enum GRID_ROW_SETTING
    {
        URL_CONTAINER = 0,
        SCROLL_VIEWER = 1,
        URL_ROW = 0,
        BUTTON_ROW = 0,
        CLEAR_BTN = 0,
        TO_QUEUE_BTN = 0,
        CLEAR_SEND_BTN_GRID = 2
    }

    private enum GRID_COL_SETTING
    {
        URL_LABEL = 0,
        URL_TEXTBOX = 1,
        BUTTON_GRID = 2,
        SEARCH_BTN = 0,
        ADD_LOCAL_BTN = 1,
        MAIN_COL = 0,
        CLEAR_BTN = 0,
        TO_QUEUE_BTN = 1
    }

    public override void BuildUI()
    {
        _urlContainer = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(Constants.DEFAULT_LABEL_WIDTH),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Star),
            ],
            ColumnSpacing = Constants.DEFAULT_COLUMN_SPACING,
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
            ColumnSpacing = Constants.DEFAULT_COLUMN_SPACING,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        _searchBtn = new Button
        {
            Content = "Add from YT",
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

        _addLocalBtn.Click += async (s, e) => await AddLocalBtn_Clicked(s, e);

        _buttonGrid.Children.AddChildren(_searchBtn, (int)GRID_COL_SETTING.SEARCH_BTN, (int)GRID_ROW_SETTING.BUTTON_ROW);
        _buttonGrid.Children.AddChildren(_addLocalBtn, (int)GRID_COL_SETTING.ADD_LOCAL_BTN, (int)GRID_ROW_SETTING.BUTTON_ROW);

        var label = new TextBlock { Text = "Url", VerticalAlignment = VerticalAlignment.Center };
        _urlContainer.Children.AddChildren(label, (int)GRID_COL_SETTING.URL_LABEL, (int)GRID_ROW_SETTING.URL_ROW);
        _urlContainer.Children.AddChildren(_urlTextBox, (int)GRID_COL_SETTING.URL_TEXTBOX, (int)GRID_ROW_SETTING.URL_ROW);
        _urlContainer.Children.AddChildren(_buttonGrid, (int)GRID_COL_SETTING.BUTTON_GRID, (int)GRID_ROW_SETTING.URL_ROW);

        var _clearSendBtnGrid = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Star)
            ],
            ColumnSpacing = Constants.DEFAULT_COLUMN_SPACING
        };

        _clearListBtn = new Button
        {
            Content = "Clear",
            Background = Constants.PRIMARY_BUTTON_COLOR,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = Constants.DEFAULT_PADDING,
            Resources =
            {
                ["ButtonBackgroundPointerOver"] = Constants.PRIMARY_HOVER_COLOR,
                ["ButtonBackgroundPressed"] = Constants.PRIMARY_HOVER_COLOR
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        _clearListBtn.Click += ClearListBtn_Clicked;

        _toQueueBtn = new Button
        {
            Content = "Send to queue",
            Background = Constants.PRIMARY_BUTTON_COLOR,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = Constants.DEFAULT_PADDING,
            Resources =
            {
                ["ButtonBackgroundPointerOver"] = Constants.PRIMARY_HOVER_COLOR,
                ["ButtonBackgroundPressed"] = Constants.PRIMARY_HOVER_COLOR
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        _toQueueBtn.Click += ToQueueBtn_Clicked;

        _clearSendBtnGrid.Children.AddChildren(_clearListBtn, (int)GRID_COL_SETTING.CLEAR_BTN, (int)GRID_ROW_SETTING.CLEAR_BTN);
        _clearSendBtnGrid.Children.AddChildren(_toQueueBtn, (int)GRID_COL_SETTING.TO_QUEUE_BTN, (int)GRID_ROW_SETTING.TO_QUEUE_BTN);


        var elementFactory = new RecyclingElementFactory();
        elementFactory.SelectTemplateKey += (sender, args) =>
        {
            args.TemplateKey = "AudioItemKey";
        };
        elementFactory.Templates["AudioItemKey"] = new FuncDataTemplate<Audio>((audio, namescope) =>
        {
            return new AudioItemControl
            {
                AudioItem = audio,
                DataContext = audio
            };
        });

        _itemsRepeater = new ItemsRepeater
        {
            ItemsSource = _audioList,
            Layout = new StackLayout { Spacing = Constants.DEFAULT_ITEM_REPEATER_SPACING },
            ItemTemplate = elementFactory
        };

        _scrollViewer = new ScrollViewer
        {
            Content = _itemsRepeater,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        var mainGrid = new Grid
        {
            RowDefinitions = [
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            ],
            RowSpacing = Constants.DEFAULT_ROW_SPACING
        };

        mainGrid.Children.AddChildren(_urlContainer, (int)GRID_COL_SETTING.MAIN_COL, (int)GRID_ROW_SETTING.URL_CONTAINER);
        mainGrid.Children.AddChildren(_scrollViewer, (int)GRID_COL_SETTING.MAIN_COL, (int)GRID_ROW_SETTING.SCROLL_VIEWER);
        mainGrid.Children.AddChildren(_clearSendBtnGrid, (int)GRID_COL_SETTING.MAIN_COL, (int)GRID_ROW_SETTING.CLEAR_SEND_BTN_GRID);

        Content = new Border
        {
            Child = mainGrid,
            Padding = Constants.DEFAULT_PADDING
        };
    }
}
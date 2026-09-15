using Avalonia;
using Avalonia.Controls;
using PSPSuite.Attributes;
using PSPSuite.Helpers;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using PSPSuite.Data;
using Avalonia.Controls.Templates;
using PSPSuite.Views.Components;

namespace PSPSuite.Modules;

[TabModule("Playlist", (int)Constants.ModuleOrder.PLAYLIST)]
public partial class Playlist : GenericModule
{
    private Grid _urlContainer = new();
    private TextBox _urlTextBox = new();
    private Button _searchBtn = new();
    private Button _clearListBtn = new();
    private Button _toQueueBtn = new();
    private ItemsRepeater _itemsRepeater = new();
    private ScrollViewer _scrollViewer = new(); 
    private CheckBox _selectAllCheckBox = new();
    private readonly ObservableCollection<Media> _audioList = new();


    private enum ClearSendBtnGridRowSetting
    {
        CLEAR_BTN = 0,
        TO_QUEUE_BTN = 0,
    }

    private enum ClearSendBtnGridColSetting
    {
        CLEAR_BTN = 0,
        TO_QUEUE_BTN = 1,
    }

    private enum ButtonGridRowSetting
    {
        BUTTON_ROW = 0,
    }

    private enum ButtonGridColSetting
    {
        SEARCH_BTN = 0,
        ADD_LOCAL_BTN = 1,
    }

    private enum UrlContainerRowSetting
    {
        URL_ROW = 0,
    }

    private enum UrlContainerColSetting
    {
        URL_LABEL = 0,
        URL_TEXTBOX = 1,
        BUTTON_GRID = 2,
    }

    private enum MainGridRowSetting
    {
        URL_CONTAINER = 0,
        SELECT_ALL_CHK_BOX = 1,
        SCROLL_VIEWER = 2,
        CLEAR_SEND_BTN_GRID = 3,
    }

    private enum MainGridColSetting
    {
        MAIN_COL = 0,
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
            PlaceholderText = "Paste Youtube playlist URL..."
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

        var label = new TextBlock { Text = "Url", VerticalAlignment = VerticalAlignment.Center };
        _urlContainer.Children.AddChildren(label, (int)UrlContainerColSetting.URL_LABEL, (int)UrlContainerRowSetting.URL_ROW);
        _urlContainer.Children.AddChildren(_urlTextBox, (int)UrlContainerColSetting.URL_TEXTBOX, (int)UrlContainerRowSetting.URL_ROW);
        _urlContainer.Children.AddChildren(_searchBtn, (int)UrlContainerColSetting.BUTTON_GRID, (int)UrlContainerRowSetting.URL_ROW);


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

        _clearSendBtnGrid.Children.AddChildren(_clearListBtn, (int)ClearSendBtnGridColSetting.CLEAR_BTN, (int)ClearSendBtnGridRowSetting.CLEAR_BTN);
        _clearSendBtnGrid.Children.AddChildren(_toQueueBtn, (int)ClearSendBtnGridColSetting.TO_QUEUE_BTN, (int)ClearSendBtnGridRowSetting.TO_QUEUE_BTN);

        var elementFactory = new RecyclingElementFactory();
        elementFactory.SelectTemplateKey += (sender, args) =>
        {
            args.TemplateKey = "AudioItemKey";
        };
        elementFactory.Templates["AudioItemKey"] = new FuncDataTemplate<Media>((media, namescope) =>
        {
            return new MediaItemControl();
        });

        _itemsRepeater = new ItemsRepeater
        {
            ItemsSource = _audioList,
            Layout = new StackLayout { Spacing = Constants.DEFAULT_ITEM_REPEATER_SPACING },
            ItemTemplate = elementFactory
        };

        var repeaterBorder = new Border
        {
            Child = _itemsRepeater,
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Padding = Constants.DEFAULT_PADDING,
        };

        _scrollViewer = new ScrollViewer
        {
            Content = repeaterBorder,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        _selectAllCheckBox = new CheckBox
        {
            Content = "Select all",
        };

        var mainGrid = new Grid
        {
            RowDefinitions = [
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            ],
            RowSpacing = Constants.DEFAULT_ROW_SPACING
        };

        mainGrid.Children.AddChildren(_urlContainer, (int)MainGridColSetting.MAIN_COL, (int)MainGridRowSetting.URL_CONTAINER);
        mainGrid.Children.AddChildren(_selectAllCheckBox, (int)MainGridColSetting.MAIN_COL, (int)MainGridRowSetting.SELECT_ALL_CHK_BOX);
        mainGrid.Children.AddChildren(_scrollViewer, (int)MainGridColSetting.MAIN_COL, (int)MainGridRowSetting.SCROLL_VIEWER);
        mainGrid.Children.AddChildren(_clearSendBtnGrid, (int)MainGridColSetting.MAIN_COL, (int)MainGridRowSetting.CLEAR_SEND_BTN_GRID);

        Content = new Border
        {
            Child = mainGrid,
            Padding = Constants.DEFAULT_PADDING
        };
    }
}
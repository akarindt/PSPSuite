using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using AvaloniaEdit;
using DialogHostAvalonia;
using PSPSuite.Data;
using PSPSuite.Helpers;
using PSPSuite.Views.Components;
using System.Collections.ObjectModel;

namespace PSPSuite.Views.Windows;

public partial class MainWindow : GenericWindow
{
    private readonly Thickness QUEUE_MARGIN_THICKNESS = new(0, 10, 5, 0);
    private readonly Thickness LOG_MARGIN_THICKNESS = new(5, 0, 5, 5);
    private readonly Thickness MAIN_MARGIN_THICKNESS = new(5, 10, 0, 0);
    private readonly Thickness BROWSE_BTN_MARGIN_THICKNESS = new(0, 0, 5, 0);
    private readonly Thickness PATH_CONTAINER_MARGIN_THICKNESS = new(15, 0);
    private readonly Thickness COOKIES_CONTAINER_MARGIN_THICKNESS = new(15, 0, 15, 10);
    private readonly Thickness SEND_BTN_MARGIN_THICKNESS = new(5, 10);

    private enum RootGridRowSetting
    {
        QUEUE = 0,
        H_SPLITTER = 1,
        LOG = 2,
    }

    private enum RootGridColSetting
    {
        MAIN = 0,
        V_SPLITTER = 1,
        QUEUE = 2,
    }

    private enum RootGridSpanColSetting
    {
        LOG_PANEL = 3,
        H_SPLITTER = 3,
    }

    private enum PathContainerRowSetting
    {
        PATH = 0,
    }

    private enum PathContainerColSetting
    {
        PATH_LABEL = 0,
        PATH_INPUT = 1,
        PATH_BROWSE_BUTTON = 2,
    }

    private enum CookiesContainerRowSetting
    {
        PATH = 0,
    }

    private enum CookiesContainerColSetting
    {
        PATH_LABEL = 0,
        PATH_INPUT = 1,
        PATH_BROWSE_BUTTON = 2,
    }

    private TextBlock _queueHeader = new();
    private DividerControl _queuePanelDivider = new();
    private ItemsRepeater _queueList = new();
    private ScrollViewer _queueScrollViewer = new();
    private Button _queueSendBtn = new();
    private Border _queuePanel = new();
    private TextBlock _logPanelHeader = new();
    private DividerControl _logPanelDivider = new();
    private TextEditor _logTextBlock = new();
    private Border _logPanel = new();
    private Grid _cookiesPathContainer = new();
    private TextBox _cookiesPath = new();
    private Button _cookiesBrowseBtn = new();
    private TabControl _mainTabControl = new();
    private Border _mainPanel = new();
    private GridSplitter _vSplitter = new();
    private GridSplitter _hSplitter = new();
    private Panel _cornerFiller = new();
    private Grid _rootGrid = new();
    private Grid _pathContainer = new();
    private TextBox _drivePath = new();
    private Button _browseBtn = new();
    private DockPanel _mainDock = new();

    private readonly ObservableCollection<QueueItem> _queueListData = new();

    protected override void BuildUI()
    {
        _queueHeader = new TextBlock
        {
            Text = "Queue list",
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        DockPanel.SetDock(_queueHeader, Dock.Top);

        _queuePanelDivider = new DividerControl();
        DockPanel.SetDock(_queuePanelDivider, Dock.Top);

        var elementFactory = new RecyclingElementFactory();
        elementFactory.SelectTemplateKey += (sender, args) =>
        {
            args.TemplateKey = "QueueItemKey";
        };
        elementFactory.Templates["QueueItemKey"] = new FuncDataTemplate<QueueItem>((queue, namescope) =>
        {
            return new QueueItemControl
            {
                QueueItem = queue
            };
        });

        _queueList = new ItemsRepeater
        {
            ItemsSource = _queueListData,
            Layout = new StackLayout
            {
                Spacing = Constants.DEFAULT_ITEM_REPEATER_SPACING
            },
            ItemTemplate = elementFactory
        };

        _queueScrollViewer = new ScrollViewer { Content = _queueList };

        _queueSendBtn = new Button
        {
            Content = "Send",
            Margin = SEND_BTN_MARGIN_THICKNESS,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Padding = Constants.DEFAULT_PADDING,
            Background = Constants.PRIMARY_BUTTON_COLOR,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Resources =
            {
                ["ButtonBackgroundPointerOver"] = Constants.PRIMARY_HOVER_COLOR,
                ["ButtonBackgroundPressed"] = Constants.PRIMARY_HOVER_COLOR
            }
        };

        DockPanel.SetDock(_queueSendBtn, Dock.Bottom);

        _queuePanel = new Border
        {
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Background = Constants.CONTAINER_BACKGROUND_COLOR,
            Margin = QUEUE_MARGIN_THICKNESS,
            Padding = Constants.DEFAULT_PADDING,
            Child = new DockPanel
            {
                LastChildFill = true,
                Children =
                {
                    _queueHeader,
                    _queuePanelDivider,
                    _queueSendBtn,
                    _queueScrollViewer,
                }
            }
        };

        _logPanelHeader = new TextBlock
        {
            Text = "Activity Log",
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        DockPanel.SetDock(_logPanelHeader, Dock.Top);

        _logPanelDivider = new DividerControl();
        DockPanel.SetDock(_logPanelDivider, Dock.Top);

        _logTextBlock = new TextEditor
        {
            Text = "",
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            IsReadOnly = true,
            ShowLineNumbers = false,
            WordWrap = true,
            Options = new TextEditorOptions
            {
                AllowScrollBelowDocument = false
            },
            Resources =
            {
                ["SystemControlHighlightListAccentLowBrush"] = Constants.PRIMARY_TEXT_COLOR
            }
        };

        _logPanel = new Border
        {
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Background = Constants.CONTAINER_BACKGROUND_COLOR,
            Margin = LOG_MARGIN_THICKNESS,
            Padding = Constants.DEFAULT_PADDING,
            Child = new DockPanel
            {
                LastChildFill = true,
                Children =
                {
                    _logPanelHeader,
                    _logPanelDivider,
                    _logTextBlock
                },
            }
        };

        _cookiesPathContainer = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(Constants.DEFAULT_LABEL_WIDTH),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Auto),
            ],
            ColumnSpacing = Constants.DEFAULT_COLUMN_SPACING,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = COOKIES_CONTAINER_MARGIN_THICKNESS
        };

        _cookiesPath = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            IsReadOnly = true,
            Focusable = false,
            IsTabStop = false,
            IsHitTestVisible = false,
            Background = Constants.PRIMARY_BACKGROUND_COLOR
        };

        _cookiesBrowseBtn = new Button
        {
            Content = "Browse",
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
            Margin = BROWSE_BTN_MARGIN_THICKNESS
        };

        _cookiesPathContainer.Children.AddChildren(new TextBlock { Text = "Cookies", VerticalAlignment = VerticalAlignment.Center }, (int)CookiesContainerColSetting.PATH_LABEL, (int)CookiesContainerRowSetting.PATH);
        _cookiesPathContainer.Children.AddChildren(_cookiesPath, (int)CookiesContainerColSetting.PATH_INPUT, (int)CookiesContainerRowSetting.PATH);
        _cookiesPathContainer.Children.AddChildren(_cookiesBrowseBtn, (int)CookiesContainerColSetting.PATH_BROWSE_BUTTON, (int)CookiesContainerRowSetting.PATH);
        DockPanel.SetDock(_cookiesPathContainer, Dock.Top);

        _pathContainer = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(Constants.DEFAULT_LABEL_WIDTH),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Auto),
            ],
            ColumnSpacing = Constants.DEFAULT_COLUMN_SPACING,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = PATH_CONTAINER_MARGIN_THICKNESS
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
            Margin = BROWSE_BTN_MARGIN_THICKNESS
        };

        _pathContainer.Children.AddChildren(new TextBlock { Text = "Saved", VerticalAlignment = VerticalAlignment.Center }, (int)PathContainerColSetting.PATH_LABEL, (int)PathContainerRowSetting.PATH);
        _pathContainer.Children.AddChildren(_drivePath, (int)PathContainerColSetting.PATH_INPUT, (int)PathContainerRowSetting.PATH);
        _pathContainer.Children.AddChildren(_browseBtn, (int)PathContainerColSetting.PATH_BROWSE_BUTTON, (int)PathContainerRowSetting.PATH);

        _mainTabControl = new TabControl { };

        DockPanel.SetDock(_pathContainer, Dock.Top);

        _mainDock = new DockPanel
        {
            Children =
            {
                _cookiesPathContainer,
                _pathContainer,
                _mainTabControl
            },
        };

        _mainPanel = new Border
        {
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Background = Constants.CONTAINER_BACKGROUND_COLOR,
            Child = _mainDock,
            Margin = MAIN_MARGIN_THICKNESS,
            Padding = Constants.DEFAULT_PADDING
        };

        _vSplitter = new GridSplitter
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
            ResizeDirection = GridResizeDirection.Columns,
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
        };

        _hSplitter = new GridSplitter
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            ResizeDirection = GridResizeDirection.Rows,
            Background = Constants.PRIMARY_BACKGROUND_COLOR
        };

        _cornerFiller = new Panel { Background = Constants.PRIMARY_BACKGROUND_COLOR };

        _rootGrid = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(Constants.DEFAULT_SPLITTER_COL_LENGTH),
                new ColumnDefinition(Constants.DEFAULT_COL_LENGTH),
            ],
            RowDefinitions =
            [
                new RowDefinition(GridLength.Star),
                new RowDefinition(Constants.DEFAULT_SPLITTER_ROW_LENGTH),
                new RowDefinition(Constants.DEFAULT_ROW_LENGTH),
            ],
        };

        _rootGrid.Children.AddChildren(_queuePanel, (int)RootGridColSetting.QUEUE, (int)RootGridRowSetting.QUEUE);
        _rootGrid.Children.AddChildren(_logPanel, (int)RootGridColSetting.MAIN, (int)RootGridRowSetting.LOG, (int)RootGridSpanColSetting.LOG_PANEL);
        _rootGrid.Children.AddChildren(_mainPanel, (int)RootGridColSetting.MAIN, (int)RootGridRowSetting.QUEUE);
        _rootGrid.Children.AddChildren(_vSplitter, (int)RootGridColSetting.V_SPLITTER, (int)RootGridRowSetting.QUEUE);
        _rootGrid.Children.AddChildren(_hSplitter, (int)RootGridColSetting.V_SPLITTER, (int)RootGridRowSetting.H_SPLITTER, (int)RootGridSpanColSetting.H_SPLITTER);
        _rootGrid.Children.AddChildren(_cornerFiller, (int)RootGridColSetting.V_SPLITTER, (int)RootGridRowSetting.H_SPLITTER);

        var mainDialogHost = new DialogHost { Identifier = Constants.APP_NAME, Content = _rootGrid, Background = Constants.CONTAINER_BACKGROUND_COLOR };
        Content = mainDialogHost;
    }
}
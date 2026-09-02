using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using PSPSuite.Helpers;
using PSPSuite.Views.Components;

namespace PSPSuite.Views.Windows;

public partial class MainWindow
{
    private readonly Thickness _queueMarginThickness = new(0, 10, 5, 0);
    private readonly Thickness _logMarginThickness = new(5, 0, 5, 5);
    private readonly Thickness _mainMarginThickness = new(5, 10, 0, 0);

    private readonly double _pathLabelWidth = 50.0;
    private readonly double _browseBtnWidth = 100.0;
    private readonly double _pathGridSpacing = 10.0;

    private enum GRID_ROW_SETTING
    {
        QUEUE = 0,
        LOG = 2,
        MAIN = 0,
        V_SPLITTER = 0,
        H_SPLITTER = 1,
        CORNER_FILTER = 1
    }

    private enum GRID_COL_SETTING
    {
        QUEUE = 2,
        LOG = 0,
        MAIN = 0,
        V_SPLITTER = 1,
        H_SPLITTER = 0,
        CORNER_FILTER = 1
    }

    private enum GRID_SPAN_COL_SETTING
    {
        LOG = 3,
        H_SPLITTER = 3
    }

    private enum PATH_GRID_COL
    {
        LABEL = 0,
        PATH_INPUT = 1,
        BROWSE_BUTTON = 2
    }

    private readonly int _pathGridRow = 0;

    #region Component declare

    private TextBlock _queueHeader = new();
    private DividerControl _queuePanelDivider = new();
    private ItemsRepeater _queueList = new();
    private ScrollViewer _queueScrollViewer = new();
    private Button _queueSendBtn = new();
    private Border _queuePanel = new();
    private TextBlock _logPanelHeader = new();
    private DividerControl _logPanelDivider = new();
    private SelectableTextBlock _logTextBlock = new();
    private ScrollViewer _logScrollViewer = new();
    private Border _logPanel = new();
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
    #endregion

    public override void BuildUI()
    {

        #region Queue panel
        _queueHeader = new TextBlock
        {
            Text = "Queue list",
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        DockPanel.SetDock(_queueHeader, Dock.Top);

        _queuePanelDivider = new DividerControl();
        DockPanel.SetDock(_queuePanelDivider, Dock.Top);

        _queueList = new ItemsRepeater
        {
            Layout = new StackLayout
            {

            }
        };

        _queueScrollViewer = new ScrollViewer { Content = _queueList };

        _queueSendBtn = new Button
        {
            Content = "Send",
            Margin = Constants.DEFAULT_MARGIN,
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
            Margin = _queueMarginThickness,
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
        #endregion

        #region Log panel

        _logPanelHeader = new TextBlock
        {
            Text = "Activity Log",
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        DockPanel.SetDock(_logPanelHeader, Dock.Top);

        _logPanelDivider = new DividerControl();
        DockPanel.SetDock(_logPanelDivider, Dock.Top);

        _logTextBlock = new SelectableTextBlock
        {
            Text = "",
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        _logScrollViewer = new ScrollViewer { Content = _logTextBlock };

        Console.SetOut(new TextWriterExtend(text =>
        {
            Dispatcher.Post(() =>
            {
                _logTextBlock.Text += text;
                _logScrollViewer.ScrollToEnd();
            });
        }));

        _logPanel = new Border
        {
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Background = Constants.CONTAINER_BACKGROUND_COLOR,
            Margin = _logMarginThickness,
            Padding = Constants.DEFAULT_PADDING,
            Child = new DockPanel
            {
                LastChildFill = true,
                Children =
                {
                    _logPanelHeader,
                    _logPanelDivider,
                    _logScrollViewer
                },
            }
        };

        #endregion

        #region Main panel

        _pathContainer = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(new GridLength(_pathLabelWidth)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(_browseBtnWidth)),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Auto),
            ],
            ColumnSpacing = _pathGridSpacing,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = Constants.DEFAULT_MARGIN
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
            Width = _browseBtnWidth,
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

        _pathContainer.Children.AddChildren(new TextBlock { Text = "Drive", VerticalAlignment = VerticalAlignment.Center }, (int)PATH_GRID_COL.LABEL, _pathGridRow);
        _pathContainer.Children.AddChildren(_drivePath, (int)PATH_GRID_COL.PATH_INPUT, _pathGridRow);
        _pathContainer.Children.AddChildren(_browseBtn, (int)PATH_GRID_COL.BROWSE_BUTTON, _pathGridRow);

        _mainTabControl = new TabControl { };
        _mainTabControl.LoadModule();

        DockPanel.SetDock(_pathContainer, Dock.Top);

        _mainDock = new DockPanel
        {
            Children =
            {
                _pathContainer,
                _mainTabControl
            },
        };

        _mainPanel = new Border
        {
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Background = Constants.CONTAINER_BACKGROUND_COLOR,
            Child = _mainDock,
            Margin = _mainMarginThickness,
            Padding = Constants.DEFAULT_PADDING
        };
        #endregion

        #region Etcs
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

        _rootGrid.Children.AddChildren(_queuePanel, (int)GRID_COL_SETTING.QUEUE, (int)GRID_ROW_SETTING.QUEUE);
        _rootGrid.Children.AddChildren(_logPanel, (int)GRID_COL_SETTING.LOG, (int)GRID_ROW_SETTING.LOG, (int)GRID_SPAN_COL_SETTING.LOG);
        _rootGrid.Children.AddChildren(_mainPanel, (int)GRID_COL_SETTING.MAIN, (int)GRID_ROW_SETTING.MAIN);
        _rootGrid.Children.AddChildren(_vSplitter, (int)GRID_COL_SETTING.V_SPLITTER, (int)GRID_ROW_SETTING.V_SPLITTER);
        _rootGrid.Children.AddChildren(_hSplitter, (int)GRID_COL_SETTING.H_SPLITTER, (int)GRID_ROW_SETTING.H_SPLITTER, (int)GRID_SPAN_COL_SETTING.H_SPLITTER);
        _rootGrid.Children.AddChildren(_cornerFiller, (int)GRID_COL_SETTING.CORNER_FILTER, (int)GRID_ROW_SETTING.CORNER_FILTER);
        #endregion


        UsbWatcher.InitUsbListener();
        Content = _rootGrid;
    }
}
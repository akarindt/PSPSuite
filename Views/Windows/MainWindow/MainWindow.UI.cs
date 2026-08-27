using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using PSPSuite.Helpers;
using PSPSuite.Views.Components;

namespace PSPSuite.Views.Windows;

public partial class MainWindow
{
    private static readonly Thickness QUEUE_MARGIN_THICKNESS = new(0, 10, 5, 0);
    private static readonly Thickness LOG_MARGIN_THICKNESS = new(5, 0, 5, 5);
    private static readonly Thickness MAIN_MARGIN_THICKNESS = new(5, 10, 0, 0);
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

    public override void BuildUI()
    {

        #region Queue panel
        var queueHeader = new TextBlock
        {
            Text = "Queue list",
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        DockPanel.SetDock(queueHeader, Dock.Top);

        var queuePanelDivider = new DividerControl();
        DockPanel.SetDock(queuePanelDivider, Dock.Top);

        var queueList = new ItemsRepeater
        {
            Layout = new StackLayout
            {

            }
        };

        var queueScrollViewer = new ScrollViewer { Content = queueList };

        var queueSendBtn = new Button
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

        DockPanel.SetDock(queueSendBtn, Dock.Bottom);

        var queuePanel = new Border
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
                    queueHeader,
                    queuePanelDivider,
                    queueSendBtn,
                    queueScrollViewer,
                }
            }
        };
        #endregion

        #region Log panel

        var logPanelHeader = new TextBlock
        {
            Text = "Activity Log",
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        DockPanel.SetDock(logPanelHeader, Dock.Top);

        var logPanelDivider = new DividerControl();
        DockPanel.SetDock(logPanelDivider, Dock.Top);

        var logTextBlock = new TextBlock
        {
            Text = "",
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var logScrollViewer = new ScrollViewer { Content = logTextBlock };

        Console.SetOut(new TextWriterExtend(text =>
        {
            Dispatcher.Post(() =>
            {
                logTextBlock.Text += text;
                logScrollViewer.ScrollToEnd();
            });
        }));

        var logPanel = new Border
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
                    logPanelHeader,
                    logPanelDivider,
                    logScrollViewer
                },
            }
        };

        #endregion

        #region Main panel
        var newTab = new TabItem
        {
            Header = "Dashboard Mới",
            Content = new TextBlock {}
        };

        var mainPanel = new Border
        {
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Background = Constants.CONTAINER_BACKGROUND_COLOR,
            Child = new TabControl
            {
                Items =
                {
                    newTab
                }
            },
            Margin = MAIN_MARGIN_THICKNESS
        };
        #endregion

        #region Etcs
        var vSplitter = new GridSplitter
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
            ResizeDirection = GridResizeDirection.Columns,
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
        };


        var hSplitter = new GridSplitter
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            ResizeDirection = GridResizeDirection.Rows,
            Background = Constants.PRIMARY_BACKGROUND_COLOR
        };

        var cornerFiller = new Panel { Background = Constants.PRIMARY_BACKGROUND_COLOR };

        var rootGrid = new Grid
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

        rootGrid.Children.AddChildren(queuePanel, (int)GRID_COL_SETTING.QUEUE, (int)GRID_ROW_SETTING.QUEUE);
        rootGrid.Children.AddChildren(logPanel, (int)GRID_COL_SETTING.LOG, (int)GRID_ROW_SETTING.LOG, (int)GRID_SPAN_COL_SETTING.LOG);
        rootGrid.Children.AddChildren(mainPanel, (int)GRID_COL_SETTING.MAIN, (int)GRID_ROW_SETTING.MAIN);
        rootGrid.Children.AddChildren(vSplitter, (int)GRID_COL_SETTING.V_SPLITTER, (int)GRID_ROW_SETTING.V_SPLITTER);
        rootGrid.Children.AddChildren(hSplitter, (int)GRID_COL_SETTING.H_SPLITTER, (int)GRID_ROW_SETTING.H_SPLITTER, (int)GRID_SPAN_COL_SETTING.H_SPLITTER);
        rootGrid.Children.AddChildren(cornerFiller, (int)GRID_COL_SETTING.CORNER_FILTER, (int)GRID_ROW_SETTING.CORNER_FILTER);
        #endregion

        Content = rootGrid;
    }
}
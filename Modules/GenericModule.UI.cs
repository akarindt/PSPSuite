using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using PSPSuite.Data;
using PSPSuite.Helpers;
using PSPSuite.Views.Components;
using System.Collections.Generic;

namespace PSPSuite.Modules;

public abstract partial class GenericModule
{
    private enum ButtonGridRowSetting
    {
        BUTTON_ROW = 0,
    }

    private enum ButtonGridColSetting
    {
        SEARCH_BTN = 0,
        ADD_LOCAL_BTN = 1,
    }

    private enum ClearSendBtnGridRowSetting
    {
        BUTTON_ROW = 0,
    }

    private enum ClearSendBtnGridColSetting
    {
        CLEAR_BTN = 0,
        TO_QUEUE_BTN = 1,
    }

    private enum UrlContainerRowSetting
    {
        URL_ROW = 0,
    }

    private enum UrlContainerColSetting
    {
        URL_LABEL = 0,
        URL_TEXTBOX = 1,
        FIRST_TRAILING = 2,
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

    protected static Button CreatePrimaryButton(string content, bool fill = false)
    {
        var button = new Button
        {
            Content = content,
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

        if (fill)
        {
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.Padding = Constants.DEFAULT_PADDING;
        }

        return button;
    }

    protected static TextBox CreateUrlTextBox(string placeholder) => new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Background = Constants.PRIMARY_BACKGROUND_COLOR,
        PlaceholderText = placeholder
    };

    protected static Grid CreateSearchButtonGrid(Button searchBtn, Button addLocalBtn)
    {
        var buttonGrid = new Grid
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

        buttonGrid.Children.AddChildren(searchBtn, (int)ButtonGridColSetting.SEARCH_BTN, (int)ButtonGridRowSetting.BUTTON_ROW);
        buttonGrid.Children.AddChildren(addLocalBtn, (int)ButtonGridColSetting.ADD_LOCAL_BTN, (int)ButtonGridRowSetting.BUTTON_ROW);
        return buttonGrid;
    }

    // "Url" label, then the url text box (fills), then each remaining control in its own auto-sized column
    protected static Grid CreateUrlContainer(TextBox urlTextBox, params Control[] trailingControls)
    {
        var urlContainer = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(Constants.DEFAULT_LABEL_WIDTH),
                new ColumnDefinition(GridLength.Star),
            ],
            RowDefinitions = [
                new RowDefinition(GridLength.Star),
            ],
            ColumnSpacing = Constants.DEFAULT_COLUMN_SPACING,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Width = double.NaN
        };

        var label = new TextBlock { Text = "Url", VerticalAlignment = VerticalAlignment.Center };
        urlContainer.Children.AddChildren(label, (int)UrlContainerColSetting.URL_LABEL, (int)UrlContainerRowSetting.URL_ROW);
        urlContainer.Children.AddChildren(urlTextBox, (int)UrlContainerColSetting.URL_TEXTBOX,(int)UrlContainerRowSetting.URL_ROW);

        for (int i = 0; i < trailingControls.Length; i++)
        {
            urlContainer.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            urlContainer.Children.AddChildren(trailingControls[i], (int)UrlContainerColSetting.FIRST_TRAILING + i,(int)UrlContainerRowSetting.URL_ROW);
        }

        return urlContainer;
    }

    protected static ItemsRepeater CreateMediaRepeater(IEnumerable<Media> items, string templateKey)
    {
        var elementFactory = new RecyclingElementFactory();
        elementFactory.SelectTemplateKey += (sender, args) =>
        {
            args.TemplateKey = templateKey;
        };
        elementFactory.Templates[templateKey] = new FuncDataTemplate<Media>((media, namescope) =>
        {
            return new MediaItemControl();
        });

        return new ItemsRepeater
        {
            ItemsSource = items,
            Layout = new StackLayout { Spacing = Constants.DEFAULT_ITEM_REPEATER_SPACING },
            ItemTemplate = elementFactory
        };
    }

    protected static ScrollViewer CreateMediaScrollViewer(ItemsRepeater itemsRepeater)
    {
        var repeaterBorder = new Border
        {
            Child = itemsRepeater,
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Padding = Constants.DEFAULT_PADDING,
        };

        return new ScrollViewer
        {
            Content = repeaterBorder,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    protected static Border BuildModuleLayout(Grid urlContainer, CheckBox selectAllCheckBox, ScrollViewer scrollViewer, Button clearListBtn, Button toQueueBtn)
    {
        var clearSendBtnGrid = new Grid
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

        clearSendBtnGrid.Children.AddChildren(clearListBtn, (int)ClearSendBtnGridColSetting.CLEAR_BTN, (int)ClearSendBtnGridRowSetting.BUTTON_ROW);
        clearSendBtnGrid.Children.AddChildren(toQueueBtn, (int)ClearSendBtnGridColSetting.TO_QUEUE_BTN, (int)ClearSendBtnGridRowSetting.BUTTON_ROW);

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

        mainGrid.Children.AddChildren(urlContainer, (int)MainGridColSetting.MAIN_COL, (int)MainGridRowSetting.URL_CONTAINER);
        mainGrid.Children.AddChildren(selectAllCheckBox, (int)MainGridColSetting.MAIN_COL, (int)MainGridRowSetting.SELECT_ALL_CHK_BOX);
        mainGrid.Children.AddChildren(scrollViewer, (int)MainGridColSetting.MAIN_COL, (int)MainGridRowSetting.SCROLL_VIEWER);
        mainGrid.Children.AddChildren(clearSendBtnGrid, (int)MainGridColSetting.MAIN_COL, (int)MainGridRowSetting.CLEAR_SEND_BTN_GRID);

        return new Border
        {
            Child = mainGrid,
            Padding = Constants.DEFAULT_PADDING
        };
    }
}

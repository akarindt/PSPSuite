using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Reactive;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using PSPSuite.Helpers;
using System;

namespace PSPSuite.Views.Components;

public enum IconState
{
    ERROR = 0,
    OK = 1,
    INFO = 2,
    CONFIRM = 3,
}

public enum ButtonType
{
    OK = 0,
    OK_CANCEL = 1,
}

public enum MessageBoxResult
{
    OK = 0,
    CANCEL = 1,
}

public class MessageBoxControl : UserControl
{
    private enum MessageBoxFontSize
    {
        TITLE = 16,
        MESSAGE = 14,
        ICON = 20,
    }

    private const double BUTTON_WIDTH = 80.0;
    private const double ICON_BORDER_SIZE = 40.0;
    private const double ICON_CORNER_RADIUS = 20.0;
    private static readonly Thickness BUTTONS_PANEL_MARGIN = new(20, 20, 20, 0);
    private static readonly Thickness TITLE_MARGIN = new(0, 0, 0, 8);
    private static readonly Thickness CANCEL_BUTTON_BORDER = new(1);


    private readonly TextBlock _titleTextBlock;
    private readonly TextBlock _messageTextBlock;
    private readonly SymbolIcon _symbolIcon;
    private readonly Border _iconCircleBorder;
    private readonly StackPanel _buttonsPanel;

    public event Action<MessageBoxResult>? ButtonClicked;

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<MessageBoxControl, string>(nameof(Title), defaultValue: "Notification");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<MessageBoxControl, string>(nameof(Message));

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly StyledProperty<IconState> IconStateProperty =
        AvaloniaProperty.Register<MessageBoxControl, IconState>(nameof(Icon), defaultValue: IconState.INFO);

    public IconState Icon
    {
        get => GetValue(IconStateProperty);
        set => SetValue(IconStateProperty, value);
    }

    public static readonly StyledProperty<ButtonType> ButtonsProperty =
        AvaloniaProperty.Register<MessageBoxControl, ButtonType>(nameof(Buttons), defaultValue: ButtonType.OK);

    public ButtonType Buttons
    {
        get => GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    public MessageBoxControl()
    {
        _symbolIcon = new SymbolIcon
        {
            FontSize = (double)MessageBoxFontSize.ICON,
            Symbol = Symbol.Info,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Constants.PRIMARY_TEXT_COLOR
        };

        this.GetObservable(IconStateProperty).Subscribe(new AnonymousObserver<IconState>(state =>
        {
            _symbolIcon.Symbol = state switch
            {
                IconState.ERROR => Symbol.DismissCircle,
                IconState.CONFIRM => Symbol.QuestionCircle,
                IconState.INFO => Symbol.Info,
                IconState.OK => Symbol.CheckmarkCircle,
                _ => Symbol.Info
            };
        }));

        _iconCircleBorder = new Border
        {
            Width = ICON_BORDER_SIZE,
            Height = ICON_BORDER_SIZE,
            CornerRadius = new CornerRadius(ICON_CORNER_RADIUS),
            Background = Constants.PRIMARY_HOVER_COLOR,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = _symbolIcon
        };

        _titleTextBlock = new TextBlock
        {
            FontSize = (double)MessageBoxFontSize.TITLE,
            FontWeight = FontWeight.Bold,
            Margin = TITLE_MARGIN,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Constants.PRIMARY_TEXT_COLOR
        };
        _titleTextBlock.Bind(TextBlock.TextProperty, this.GetObservable(TitleProperty));

        _messageTextBlock = new TextBlock
        {
            FontSize = (double)MessageBoxFontSize.MESSAGE,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Constants.PRIMARY_TEXT_COLOR
        };
        _messageTextBlock.Bind(TextBlock.TextProperty, this.GetObservable(MessageProperty));

        var textPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _titleTextBlock, _messageTextBlock }
        };

        var contentLayout = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = Constants.DEFAULT_ROW_SPACING,
            Children = { _iconCircleBorder, textPanel }
        };

        _buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = Constants.DEFAULT_ROW_SPACING,
            Margin = BUTTONS_PANEL_MARGIN
        };

        this.GetObservable(ButtonsProperty).Subscribe(new AnonymousObserver<ButtonType>(RebuildButtons));

        var mainLayout = new StackPanel
        {
            Width = Constants.MSGBOX_WIDTH,
            Children = { contentLayout, _buttonsPanel },
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Padding = Constants.DEFAULT_PADDING;
        Content = mainLayout;
    }

    private void RebuildButtons(ButtonType type)
    {
        _buttonsPanel.Children.Clear();

        if (type == ButtonType.OK_CANCEL)
        {
            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = BUTTON_WIDTH,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Background = Brushes.Transparent,
                BorderBrush = Constants.PRIMARY_BUTTON_COLOR,
                BorderThickness = CANCEL_BUTTON_BORDER,
                Cursor = new Cursor(StandardCursorType.Hand),
                Resources =
                {
                    ["ButtonBackgroundPointerOver"] = Constants.PRIMARY_HOVER_COLOR,
                    ["ButtonBackgroundPressed"] = Constants.PRIMARY_HOVER_COLOR,
                    ["ButtonBorderBrushPointerOver"] = Constants.PRIMARY_BUTTON_COLOR,
                    ["ButtonBorderBrushPressed"] = Constants.PRIMARY_BUTTON_COLOR
                },
            };
            cancelButton.Click += (_, _) => ButtonClicked?.Invoke(MessageBoxResult.CANCEL);
            _buttonsPanel.Children.Add(cancelButton);
        }

        var okButton = new Button
        {
            Content = "OK",
            Width = BUTTON_WIDTH,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = Constants.PRIMARY_BUTTON_COLOR,
            Cursor = new Cursor(StandardCursorType.Hand),
            Resources =
            {
                ["ButtonBackgroundPointerOver"] = Constants.PRIMARY_HOVER_COLOR,
                ["ButtonBackgroundPressed"] = Constants.PRIMARY_HOVER_COLOR
            },
        };
        okButton.Click += (_, _) => ButtonClicked?.Invoke(MessageBoxResult.OK);
        _buttonsPanel.Children.Add(okButton);
    }
}
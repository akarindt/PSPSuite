using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PSPSuite.Data;
using PSPSuite.Helpers;

namespace PSPSuite.Views.Components;

public class QueueItemControl : Control
{
    private enum QueueFontSize
    {
        PILL = 9,
        STATUS = 10,
        FILE_NAME = 12
    }

    private const double PILLS_SPACING = 6.0;
    private const double MAIN_STACK_SPACING = 4.0;
    private const double PILL_CORNER_RADIUS = 4.0;
    private static readonly Thickness PILL_PADDING = new(6, 2);

    private readonly Border _rootBorder;
    private readonly TextBlock _fileNameTextBlock;
    private readonly TextBlock _statusTextBlock;
    private readonly TextBlock _fileTypeTextBlock;
    private readonly TextBlock _isLocalTextBlock;

    public static readonly StyledProperty<QueueItem?> QueueItemProperty =
        AvaloniaProperty.Register<QueueItemControl, QueueItem?>(nameof(QueueItem));

    public QueueItem? QueueItem
    {
        get => GetValue(QueueItemProperty);
        set => SetValue(QueueItemProperty, value);
    }

    public QueueItemControl()
    {
        _isLocalTextBlock = CreatePillTextBlock();
        _fileTypeTextBlock = CreatePillTextBlock();

        var pillBorderLocal = WrapInPill(_isLocalTextBlock);
        var pillBorderType = WrapInPill(_fileTypeTextBlock);

        var topPillsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = PILLS_SPACING,
            Children = { pillBorderLocal, pillBorderType }
        };

        _fileNameTextBlock = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = (double)QueueFontSize.FILE_NAME
        };

        _statusTextBlock = new TextBlock
        {
            FontSize = (double)QueueFontSize.STATUS,
            Foreground = Constants.PRIMARY_TEXT_COLOR,
            VerticalAlignment = VerticalAlignment.Center
        };

        var mainStackPanel = new StackPanel
        {
            Spacing = MAIN_STACK_SPACING,
            Children =
            {
                topPillsPanel,
                _fileNameTextBlock,
                _statusTextBlock
            }
        };

        _rootBorder = new Border
        {
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Padding = Constants.DEFAULT_PADDING,
            Child = mainStackPanel
        };

        VisualChildren.Add(_rootBorder);
        LogicalChildren.Add(_rootBorder);
    }

    private static TextBlock CreatePillTextBlock() => new()
    {
        FontSize = (double)QueueFontSize.PILL,
        FontWeight = FontWeight.Bold,
        Foreground = Constants.PRIMARY_TEXT_COLOR,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };

    private static Border WrapInPill(TextBlock textBlock) => new()
    {
        Background = Constants.CONTAINER_BACKGROUND_COLOR,
        CornerRadius = new CornerRadius(PILL_CORNER_RADIUS),
        Padding = PILL_PADDING,
        Child = textBlock
    };

    protected override Size MeasureOverride(Size availableSize)
    {
        _rootBorder.Measure(availableSize);
        return _rootBorder.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _rootBorder.Arrange(new Rect(finalSize));
        return finalSize;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == QueueItemProperty)
        {
            var queue = change.GetNewValue<QueueItem?>();
            if (queue != null)
            {
                string status = queue.Status switch
                {
                    QueueItemStatus.READY => "Ready",
                    QueueItemStatus.COPYING => "Copying...",
                    QueueItemStatus.COMPLETE => "Complete",
                    _ => "Err"
                };

                string type = queue.FileType switch
                {
                    QueueItemType.MUSIC => "Music",
                    QueueItemType.VIDEO => "Video",
                    QueueItemType.PLAYLIST => "Playlist",
                    _ => "Err"
                };

                string local = queue.IsLocal ? "Local" : "YT";

                _fileNameTextBlock.Text = queue.FileName;
                _fileTypeTextBlock.Text = type;
                _statusTextBlock.Text = status;
                _isLocalTextBlock.Text = local;
            }
        }
    }
}
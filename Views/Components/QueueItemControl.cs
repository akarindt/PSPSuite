using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PSPSuite.Data;
using PSPSuite.Helpers;
using Avalonia.Layout;

namespace PSPSuite.Views.Components;

public class QueueItemControl : Control
{
    private readonly Border _rootBorder;
    private TextBlock _fileNameTextBlock;

    public static readonly StyledProperty<QueueItem?> QueueItemProperty = AvaloniaProperty.Register<QueueItemControl, QueueItem?>(nameof(QueueItem));

    public QueueItem? QueueItem
    {
        get => GetValue(QueueItemProperty);
        set => SetValue(QueueItemProperty, value);
    }

    public QueueItemControl()
    {
        _fileNameTextBlock = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };


        _rootBorder = new Border
        {
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Padding = Constants.DEFAULT_PADDING,
            Child = _fileNameTextBlock
        };

        VisualChildren.Add(_rootBorder);
        LogicalChildren.Add(_rootBorder);
    }

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
                _fileNameTextBlock.Text = queue.FileName;
            }
        }
    }
}
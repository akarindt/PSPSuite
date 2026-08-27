using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PSPSuite.Helpers;

namespace PSPSuite.Views.Components;

public class DividerControl : Control
{
    public static new readonly StyledProperty<Thickness> MarginProperty =
        AvaloniaProperty.Register<DividerControl, Thickness>(nameof(Margin), Constants.DEFAULT_DIVIDER_PADDING);

    public new Thickness Margin
    {
        get => GetValue(MarginProperty);
        set => SetValue(MarginProperty, value);
    }

    public static readonly StyledProperty<IBrush> ColorProperty =
        AvaloniaProperty.Register<DividerControl, IBrush>(nameof(Color), Constants.PRIMARY_TEXT_COLOR);

    public IBrush Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<DividerControl, double>(nameof(Thickness), Constants.DEFAULT_THICKNESS);

    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(availableSize.Width, Thickness + Margin.Top + Margin.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        var rect = new Rect(
            Margin.Left,
            Margin.Top,
            Bounds.Width - Margin.Left - Margin.Right,
            Thickness);

        context.FillRectangle(Color, rect);
    }
}
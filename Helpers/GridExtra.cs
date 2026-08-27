using Avalonia.Controls;

namespace PSPSuite.Helpers;

public static class GridExtra
{
    public static void AddChildren(this Avalonia.Collections.AvaloniaList<Control> children, Control control, int? column = null, int? row = null, int? columnSpan = null, int? rowSpan = null)
    {
        if (column is int c) Grid.SetColumn(control, c);
        if (row is int r) Grid.SetRow(control, r);
        if (columnSpan is int cs) Grid.SetColumnSpan(control, cs);
        if (rowSpan is int rs) Grid.SetColumnSpan(control, rs);
        children.Add(control);
    }
}
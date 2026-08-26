using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using PSPSuite.Helper;

namespace PSPSuite;

public partial class MainWindow : GenericWindow
{
    public MainWindow() { }

    public override void BuildUI()
    {
        var dockPanel = new DockPanel { };

        var sidePanel = new Panel
        {
            Background = Brushes.White,
            Width = 300
        };

        var sidePanel2 = new Panel
        {
            Background = Brushes.Red,
            Height = 200
        };

        var sidePanel3 = new Panel
        {
            Background = Brushes.Aqua,
        };

        DockPanel.SetDock(sidePanel, Dock.Right);
        DockPanel.SetDock(sidePanel2, Dock.Bottom);

        dockPanel.Children.Add(sidePanel);
        dockPanel.Children.Add(sidePanel2);
        dockPanel.Children.Add(sidePanel3);

        Content = dockPanel;
    }
}
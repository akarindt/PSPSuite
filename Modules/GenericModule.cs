using Avalonia.Controls;
using PSPSuite.Helpers;

namespace PSPSuite.Modules;

public abstract class GenericModule : UserControl
{
    public abstract void BuildUI();

    protected GenericModule()
    {
        Loaded += (s, e) => BuildUI();
    }
}
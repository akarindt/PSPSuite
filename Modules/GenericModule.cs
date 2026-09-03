using System;
using System.Collections;
using Avalonia.Controls;
using PSPSuite.Helpers;

namespace PSPSuite.Modules;

public abstract class GenericModule : UserControl
{
    public event EventHandler<IList>? SendToQueueRequested;

    public abstract void BuildUI();

    protected GenericModule()
    {
        Loaded += (s, e) => BuildUI();
    }

    protected void RaiseSendToQueue(IList items)
    {
        SendToQueueRequested?.Invoke(this, items);
    }
}
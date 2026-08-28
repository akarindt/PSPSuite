using System;

namespace PSPSuite.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class TabModuleAttribute : Attribute
{
    public string Title {get; }
    public int Order {get;}

    public TabModuleAttribute(string title, int order = 0)
    {
        Title = title;
        Order = order;
    }
}
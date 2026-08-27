using System;

namespace PSPSuite.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class TabModuleAttribute : Attribute
{
    public string Title {get; }
    public int Order {get;}
    public Type ViewType {get;}

    public TabModuleAttribute(string title, Type viewType, int order = 0)
    {
        Title = title;
        ViewType = viewType;
        Order = order;
    }
}
using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using PSPSuite.Attributes;
using PSPSuite.Modules;

namespace PSPSuite.Helpers;

public static class ModuleLoader
{
    public static void LoadModule(this TabControl tabControl)
    {
        var modules = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm =>
                {
                    try { return asm.GetTypes(); }
                    catch { return []; }
                })
                .Where(type => typeof(GenericModule).IsAssignableFrom(type) && !type.IsAbstract)
                .Select(type => new
                {
                    Type = type,
                    Attr = type.GetCustomAttribute<TabModuleAttribute>()
                })
                .Where(x => x.Attr is TabModuleAttribute attr)
                .OrderBy(x => x.Attr!.Order)
                .ToList();


        foreach (var item in modules)
        {
            if (Activator.CreateInstance(item.Type) is GenericModule viewInstance)
            {
                var tabItem = new TabItem
                {
                    Header = item.Attr!.Title,
                    Content = viewInstance
                };

                tabControl.Items.Add(tabItem);
                Console.WriteLine($"[ModuleLoader]::Loaded - {item.Type.Name}");
            }
        }
    }
}
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using PSPSuite.Attributes;
using PSPSuite.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PSPSuite.Helpers;

public static class ModuleLoader
{
    public static void LoadModule(this TabControl tabControl, IEnumerable<GenericModule> modules)
    {
        var sortedModules = modules
            .Select(m => new
            {
                Instance = m,
                Attr = m.GetType().GetCustomAttribute<TabModuleAttribute>()
            })
            .Where(x => x.Attr != null)
            .OrderBy(x => x.Attr!.Order);

        foreach (var item in sortedModules)
        {
            var tabItem = new TabItem
            {
                Header = item.Attr!.Title,
                Content = item.Instance
            };

            tabControl.Items.Add(tabItem);
            Console.WriteLine($"[ModuleLoader]::Loaded - {item.Instance.GetType().Name}");
        }
    }

    public static void LoadModule(this TabControl tabControl, IServiceProvider serviceProvider)
    {
        var modules = serviceProvider.GetServices<GenericModule>();
        tabControl.LoadModule(modules);
    }
}
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using PSPSuite.Data;
using PSPSuite.Dependencies;
using PSPSuite.Views.Windows;

namespace PSPSuite.Helpers;

public static class DependenciesHelper
{
    public static async Task LoadDeps(this GenericWindow window)
    {
        var items = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(type => typeof(DependencyItem).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .Select(type => Activator.CreateInstance(type) as DependencyItem)
            .Where(item => item != null)
            .OrderBy(item => item!.Order)
            .ToList();

        window.Closed += (s, e) =>
        {
            foreach (var depItem in items)
            {
                if(depItem == null) continue;
                
                try
                {
                    depItem?.Cleanup();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DependenciesHelper_LoadDeps]::Cleanup error for {depItem?.GetType().Name}: {ex.Message}");
                }
            }
        };

        foreach (var depItem in items)
        {
            if (depItem == null) continue;

            Console.WriteLine($"[DependenciesHelper_LoadDeps]::Download - {depItem.GetType().Name}");
            await depItem.DownloadItemAsync();

            Console.WriteLine($"[DependenciesHelper_LoadDeps]::Execute - {depItem.GetType().Name}");
            await depItem.ExecuteAsync();
        }
    }
}
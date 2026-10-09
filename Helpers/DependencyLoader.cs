using PSPSuite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSPSuite.Helpers;

public interface IDependencyLoader
{
    Task LoadDepsAsync();
    Task Cleanup();
}

public class DependencyLoader : IDependencyLoader
{
    private readonly IEnumerable<DependencyItem> _items;

    public DependencyLoader(IEnumerable<DependencyItem> items)
    {
        _items = items.OrderBy(x => x.Order).ToList();
    }

    public async Task LoadDepsAsync()
    {
        foreach (var depItem in _items)
        {
            try
            {
                Console.WriteLine($"[DependencyLoader_LoadDepsAsync]::Download - {depItem.GetType().Name}");
                await depItem.DownloadItemAsync();

                Console.WriteLine($"[DependencyLoader_LoadDepsAsync]::Init - {depItem.GetType().Name}");
                await depItem.Init();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DependencyLoader_LoadDepsAsync]::Error on {depItem.GetType().Name}: {ex.Message}");
            }
        }
    }

    public async Task Cleanup()
    {
        await Task.WhenAll(_items.Select(async depItem =>
        {
            await depItem.Cleanup();
            Console.WriteLine($"[DependencyLoader_Cleanup]::Function called! - ID: {depItem.Order}");
        }));
    }
}
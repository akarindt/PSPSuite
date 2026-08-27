#if DEBUG
using System;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(PSPSuite.Helpers.HotReload))]
namespace PSPSuite.Helpers;

public static class HotReload
{
    public static event Action? OnCodeUpdated;
    public static void ClearCache(Type[]? types) { }
    public static void UpdateApplication(Type[]? types)
    {
        OnCodeUpdated?.Invoke();
    }
}
#endif
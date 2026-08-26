using System;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(PSPSuite.Helper.HotReload))]
namespace PSPSuite.Helper;
public static class HotReload
{
    public static event Action? OnCodeUpdated;
    public static void ClearCache(Type[]? types) { }
    public static void UpdateApplication(Type[]? types)
    {
        OnCodeUpdated?.Invoke();
    }
}
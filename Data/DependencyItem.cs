using System.Threading.Tasks;

namespace PSPSuite.Data;

public abstract class DependencyItem
{
    public abstract string DownloadUrl { get; }
    public abstract int Order { get; }
    public abstract Task DownloadItemAsync();
    public abstract Task ExecuteAsync();
    public virtual async Task Cleanup() => await Task.CompletedTask;
}
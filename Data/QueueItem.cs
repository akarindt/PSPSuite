namespace PSPSuite.Data;

public class QueueItem
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsLocal { get; set; }
    public string FolderName { get; set; } = string.Empty;
}
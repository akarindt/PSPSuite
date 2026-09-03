namespace PSPSuite.Data;

public enum QueueItemType
{
    MUSIC = 0,
    VIDEO = 1,
    PLAYLIST = 2
}

public enum QueueItemStatus
{
    READY = 0,
    COPYING = 1,
    COMPLETE = 2
}

public class QueueItem
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public QueueItemType FileType { get; set; }
    public QueueItemStatus Status { get; set; }
    public bool IsLocal { get; set; }
}
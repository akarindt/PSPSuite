using System;
using Avalonia.Media.Imaging;

namespace PSPSuite.Data;

public class Audio
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTimeOffset? DateCreated { get; set; }
    public DateTimeOffset? DateModified { get; set; }
    public ulong? Size { get; set; }
    public TimeSpan? Duration { get; set; }
    public string ContributeArtist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public bool IsLocal { get; set; }
    public bool IsChecked { get; set; }
}
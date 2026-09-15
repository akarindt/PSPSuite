using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PSPSuite.Data;

public partial class Media : ObservableObject
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

    [ObservableProperty]
    private bool _isChecked;
}

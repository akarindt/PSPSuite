using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using PSPSuite.Data;
using PSPSuite.Helpers;

namespace PSPSuite.Views.Components;

public class MediaItemControl : Control
{
    private readonly TextBlock _fileNameText = new();
    private readonly TextBlock _artistAlbumText = new();
    private readonly TextBlock _durationText = new();
    private readonly TextBlock _fileSizeText = new();
    private readonly Border _rootBorder;
    private readonly CheckBox _checkBox = new();

    private enum MediaItemGridColSetting
    {
        CHECK_BOX = 0,
        INFO_SECTION = 1,
        DURATION_SIZE = 2,
    }

    private enum MediaItemFontSize
    {
        SUBTITLE = 12,
        FILE_SIZE = 11,
    }

    private const double ITEM_PADDING_HORIZONTAL = 12.0;
    private const double ITEM_PADDING_VERTICAL = 8.0;
    private const double STACK_SPACING = 8.0;
    private const string FORMAT_HH_MM_SS = @"hh\:mm\:ss";
    private const string DURATION_UNKNOWN_FULL = "00:00:00";
    private const string DURATION_UNKNOWN_SHORT = "00:00";
    private const string UNKNOWN_TITLE = "Unknown Title";
    private const string UNKNOWN_ARTIST = "Unknown Artist";
    private const string UNKNOWN_ALBUM = "Unknown Album";
    private const string ERROR_LABEL = "(Error) Unknown";
    private const string SIZE_UNKNOWN = "Unknown";
    private const string SIZE_ZERO = "0 B";
    private const string LOCAL_TAG = "(Local) ";
    private const string SEPARATOR = " • ";
    private static readonly string[] SIZE_SUFFIX = ["B", "KB", "MB", "GB", "TB"];
    private const double SIZE_DIVISOR = 1024.0;

    public static readonly StyledProperty<Media?> MediaItemProperty =
        AvaloniaProperty.Register<MediaItemControl, Media?>(nameof(MediaItem));

    public Media? MediaItem
    {
        get => GetValue(MediaItemProperty);
        set => SetValue(MediaItemProperty, value);
    }

    public MediaItemControl()
    {
        _fileNameText = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        _artistAlbumText = new TextBlock
        {
            FontSize = (double)MediaItemFontSize.SUBTITLE,
            Foreground = Brushes.Gray,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        var infoStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _fileNameText, _artistAlbumText }
        };

        _durationText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Medium
        };

        _fileSizeText = new TextBlock
        {
            FontSize = (double)MediaItemFontSize.FILE_SIZE,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.Gray,
        };

        var rightStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(STACK_SPACING, 0, 0, 0),
            Children = { _durationText, _fileSizeText }
        };

        var grid = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            ]
        };

        Grid.SetColumn(infoStack, (int)MediaItemGridColSetting.INFO_SECTION);
        Grid.SetColumn(rightStack, (int)MediaItemGridColSetting.DURATION_SIZE);

        grid.Children.Add(infoStack);
        grid.Children.Add(rightStack);

        _checkBox = new CheckBox
        {
            Content = grid,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };

        _checkBox.Bind(CheckBox.IsCheckedProperty, new Binding("IsChecked") { Mode = BindingMode.TwoWay });

        _rootBorder = new Border
        {
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Padding = Constants.DEFAULT_PADDING,
            BorderThickness = new Thickness(1),
            BorderBrush = Constants.PRIMARY_BUTTON_COLOR,
            Child = _checkBox
        };

        VisualChildren.Add(_rootBorder);
        LogicalChildren.Add(_rootBorder);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _rootBorder.Measure(availableSize);
        return _rootBorder.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _rootBorder.Arrange(new Rect(finalSize));
        return finalSize;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MediaItemProperty)
        {
            var media = change.GetNewValue<Media?>();
            if (media != null)
            {
                string isLocal = media.IsLocal ? LOCAL_TAG : "";

                _fileNameText.Text = string.IsNullOrWhiteSpace(media.FileName) ? $"{isLocal}{UNKNOWN_TITLE}" : $"{isLocal}{media.FileName}";
                _artistAlbumText.Text = $"{media.ContributeArtist ?? UNKNOWN_ARTIST}{SEPARATOR}{media.Album ?? UNKNOWN_ALBUM}";
                _fileSizeText.Text = FormatFileSize(media.Size);
                _durationText.Text = media.Duration == null ? DURATION_UNKNOWN_FULL : media.Duration.Value.ToString(FORMAT_HH_MM_SS);
                _checkBox.IsChecked = media.IsChecked;
                return;
            }

            _fileNameText.Text = ERROR_LABEL;
            _artistAlbumText.Text = $"{UNKNOWN_ARTIST}{SEPARATOR}{UNKNOWN_ALBUM}";
            _fileSizeText.Text = SIZE_ZERO;
            _durationText.Text = DURATION_UNKNOWN_SHORT;
            _checkBox.IsChecked = false;
        }
    }

    private static string FormatFileSize(ulong? bytes)
    {
        if (bytes == null) return SIZE_UNKNOWN;

        int i = 0;
        double doubleBytes = bytes.Value;
        while (doubleBytes >= SIZE_DIVISOR && i < SIZE_SUFFIX.Length - 1)
        {
            i++;
            doubleBytes /= 1024;
        }
        return $"{doubleBytes:0.##} {SIZE_SUFFIX[i]}";
    }
}

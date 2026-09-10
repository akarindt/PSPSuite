using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PSPSuite.Data;
using PSPSuite.Helpers;

namespace PSPSuite.Views.Components;

public class AudioItemControl : Control
{
    private readonly TextBlock _fileNameText = new();
    private readonly TextBlock _artistAlbumText = new();
    private readonly TextBlock _durationText = new();
    private readonly TextBlock _fileSizeText = new();
    private readonly Border _rootBorder;
    private readonly CheckBox _checkBox = new();

    private enum AudioItemGridColSetting
    {
        CHECK_BOX = 0,
        INFO_SECTION = 1,
        DURATION_SIZE = 2,
    }

    public static readonly StyledProperty<Audio?> AudioItemProperty =
        AvaloniaProperty.Register<AudioItemControl, Audio?>(nameof(AudioItem));

    public Audio? AudioItem
    {
        get => GetValue(AudioItemProperty);
        set => SetValue(AudioItemProperty, value);
    }

    public AudioItemControl()
    {
        _fileNameText = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        _artistAlbumText = new TextBlock
        {
            FontSize = 12,
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
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.Gray,
        };

        var rightStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
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

        Grid.SetColumn(infoStack, (int)AudioItemGridColSetting.INFO_SECTION);
        Grid.SetColumn(rightStack, (int)AudioItemGridColSetting.DURATION_SIZE);

        grid.Children.Add(infoStack);
        grid.Children.Add(rightStack);

        _checkBox = new CheckBox
        {
            Content = grid,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };

        _checkBox.IsCheckedChanged += (s, e) =>
        {
            if (AudioItem != null)
            {
                AudioItem.IsChecked = _checkBox.IsChecked ?? false;
            }
        };

        _checkBox.Bind(CheckBox.IsCheckedProperty, new Binding("IsChecked") { Mode = BindingMode.TwoWay });

        _rootBorder = new Border
        {
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
            CornerRadius = Constants.DEFAULT_CORNER_RADIUS,
            Padding = Constants.DEFAULT_PADDING,
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

        if (change.Property == AudioItemProperty)
        {
            var audio = change.GetNewValue<Audio?>();
            if (audio != null)
            {
                string isLocal = audio.IsLocal ? "(Local) " : "";

                _fileNameText.Text = string.IsNullOrWhiteSpace(audio.FileName) ? $"{isLocal}Unknown Title" : $"{isLocal}{audio.FileName}";
                _artistAlbumText.Text = $"{audio.ContributeArtist ?? "Unknown Artist"} • {audio.Album ?? "Unknown Album"}";
                _fileSizeText.Text = FormatFileSize(audio.Size);
                _durationText.Text = audio.Duration == null ? "00:00:00" : audio.Duration.Value.ToString(@"hh\:mm\:ss");
                _checkBox.IsChecked = audio.IsChecked;
                return;
            }

            _fileNameText.Text = "(Error) Unknown";
            _artistAlbumText.Text = "Unknown Artist • Unknown Album";
            _fileSizeText.Text = "0 B";
            _durationText.Text = "00:00";
            _checkBox.IsChecked = false;
        }
    }

    private static string FormatFileSize(ulong? bytes)
    {
        if (bytes == null) return "Unknown";

        string[] suffix = ["B", "KB", "MB", "GB", "TB"];
        int i = 0;
        double doubleBytes = bytes.Value;
        while (doubleBytes >= 1024 && i < suffix.Length - 1)
        {
            i++;
            doubleBytes /= 1024;
        }
        return $"{doubleBytes:0.##} {suffix[i]}";
    }
}
using Avalonia.Controls;
using Avalonia.Layout;
using PSPSuite.Attributes;
using PSPSuite.Data;
using PSPSuite.Helpers;
using System.Collections.ObjectModel;

namespace PSPSuite.Modules;

[TabModule("Video", (int)Constants.ModuleOrder.VIDEO)]
public partial class Video : GenericModule
{
    private Grid _urlContainer = new();
    private TextBox _urlTextBox = new();
    private ComboBox _typeComboBox = new();
    private Button _searchBtn = new();
    private Button _addLocalBtn = new();
    private Grid _buttonGrid = new();
    private readonly ObservableCollection<Media> _videoList = new();
    private ItemsRepeater _itemsRepeater = new();
    private ScrollViewer _scrollViewer = new();
    private Button _toQueueBtn = new();
    private Button _clearListBtn = new();
    private CheckBox _selectAllCheckBox = new();

    public override void BuildUI()
    {
        _urlTextBox = CreateUrlTextBox("Paste Youtube URL...");
        _typeComboBox = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = Constants.PRIMARY_BACKGROUND_COLOR,
            ItemsSource = new[] { "Single Video", "Video Playlist" },
        };
        _searchBtn = CreatePrimaryButton("Add from YT");
        _addLocalBtn = CreatePrimaryButton("Add local video");
        _buttonGrid = CreateSearchButtonGrid(_searchBtn, _addLocalBtn);
        _urlContainer = CreateUrlContainer(_urlTextBox, _typeComboBox, _buttonGrid);

        _clearListBtn = CreatePrimaryButton("Clear", fill: true);
        _toQueueBtn = CreatePrimaryButton("Send to queue", fill: true);
        _itemsRepeater = CreateMediaRepeater(_videoList, "VideoItemKey");
        _scrollViewer = CreateMediaScrollViewer(_itemsRepeater);
        _selectAllCheckBox = new CheckBox { Content = "Select all" };

        Content = BuildModuleLayout(_urlContainer, _selectAllCheckBox, _scrollViewer, _clearListBtn, _toQueueBtn);
    }
}

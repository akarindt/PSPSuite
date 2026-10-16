using Avalonia.Controls;
using PSPSuite.Attributes;
using PSPSuite.Data;
using PSPSuite.Helpers;
using System.Collections.ObjectModel;

namespace PSPSuite.Modules;

[TabModule("Playlist", (int)Constants.ModuleOrder.PLAYLIST)]
public partial class Playlist : GenericModule
{
    private Grid _urlContainer = new();
    private TextBox _urlTextBox = new();
    private Button _searchBtn = new();
    private Button _clearListBtn = new();
    private Button _toQueueBtn = new();
    private ItemsRepeater _itemsRepeater = new();
    private ScrollViewer _scrollViewer = new();
    private CheckBox _selectAllCheckBox = new();
    private readonly ObservableCollection<Media> _audioList = new();

    public override void BuildUI()
    {
        _urlTextBox = CreateUrlTextBox("Paste Youtube playlist URL...");
        _searchBtn = CreatePrimaryButton("Add from YT");
        _urlContainer = CreateUrlContainer(_urlTextBox, _searchBtn);

        _clearListBtn = CreatePrimaryButton("Clear", fill: true);
        _toQueueBtn = CreatePrimaryButton("Send to queue", fill: true);
        _itemsRepeater = CreateMediaRepeater(_audioList, "AudioItemKey");
        _scrollViewer = CreateMediaScrollViewer(_itemsRepeater);
        _selectAllCheckBox = new CheckBox { Content = "Select all" };

        Content = BuildModuleLayout(_urlContainer, _selectAllCheckBox, _scrollViewer, _clearListBtn, _toQueueBtn);
    }
}

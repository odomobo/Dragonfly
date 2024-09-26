using Avalonia.Controls;
using Avalonia.Input;
using Dragonfly.Engine;
using Dragonfly.ToolsGui.Views;
using System;
using System.ComponentModel;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Input;
using System.Reflection;
using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using System.IO;
using Avalonia;

namespace Dragonfly.ToolsGui.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ChessBoardViewModel ChessBoardViewModel { get; set; }
    public string Title { get; set; }

    public MainViewModel() {
        Title = $"Dragonfly Tools GUI - {VersionInfo.VersionWithCodename}";

        ChessBoardViewModel = new ChessBoardViewModel();
        //OpenMenuItemCommand = new AsyncRelayCommand(OpenMenuItemFn);
        TestingMenuItemCommand = new RelayCommand(TestingMenuItemFn);
    }

    //public AsyncRelayCommand OpenMenuItemCommand { get; set; }
    //private async Task OpenMenuItemFn()
    //{
    //    // Get top level from the current control. Alternatively, you can use Window reference instead.
    //    var topLevel = TopLevel.GetTopLevel(_control);

    //    // Start async operation to open the dialog.
    //    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
    //    {
    //        Title = "Open Text File",
    //        AllowMultiple = false
    //    });

    //    if (files.Count >= 1)
    //    {
    //        // do something
    //    }
    //    else
    //    {
    //        // do something else
    //    }
    //}

    public RelayCommand TestingMenuItemCommand { get; set; }
    private void TestingMenuItemFn()
    {
        var window = new TestingWindow();
        window.DataContext = new TestingViewModel();
        window.Show();
    }
}

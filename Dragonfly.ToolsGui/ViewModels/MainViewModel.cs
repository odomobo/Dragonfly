using Avalonia.Controls;
using Avalonia.Input;
using Dragonfly.Engine;
using Dragonfly.ToolsGui.Views;
using System;
using System.ComponentModel;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Input;

namespace Dragonfly.ToolsGui.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ChessBoardViewModel ChessBoardViewModel { get; set; }

    public MainViewModel() {
        ChessBoardViewModel = new ChessBoardViewModel();
        TestingMenuItem = new RelayCommand(TestingMenuItemFn);
    }

    public RelayCommand TestingMenuItem { get; set; }

    private void TestingMenuItemFn()
    {
        var window = new TestingWindow();
        window.DataContext = new TestingViewModel();
        window.Show();
    }
}

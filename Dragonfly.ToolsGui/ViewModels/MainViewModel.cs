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
    }
}

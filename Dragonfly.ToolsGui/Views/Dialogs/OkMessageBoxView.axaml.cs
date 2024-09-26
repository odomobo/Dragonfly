using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using DialogHostAvalonia;

namespace Dragonfly.ToolsGui.Views.Dialogs;

public partial class OkMessageBoxView : UserControl
{
    public OkMessageBoxView(string message)
    {
        InitializeComponent();
        Message.Text = message;
    }
}
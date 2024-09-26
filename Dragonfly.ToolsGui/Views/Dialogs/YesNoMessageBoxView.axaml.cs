using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using DialogHostAvalonia;

namespace Dragonfly.ToolsGui.Views.Dialogs;

public partial class YesNoMessageBoxView : UserControl
{
    public YesNoMessageBoxView(string message, string yes = "Yes", string no = "No")
    {
        InitializeComponent();
        Message.Text = message;
        Yes.Text = yes;
        No.Text = no;
    }

    private async void Yes_Clicked(object sender, RoutedEventArgs args)
    {
        var dialogHost = this.FindAncestorOfType<DialogHost>();
        dialogHost.CloseDialogCommand.Execute(true);
    }

    private async void No_Clicked(object sender, RoutedEventArgs args)
    {
        var dialogHost = this.FindAncestorOfType<DialogHost>();
        dialogHost.CloseDialogCommand.Execute(false);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using DialogHostAvalonia;

namespace Dragonfly.ToolsGui.Views.Dialogs;

public partial class InputBoxDialog : UserControl
{
    public InputBoxDialog(string message, string inputDefault = "", string ok = "Ok", string cancel = "Cancel")
    {
        InitializeComponent();
        Message.Text = message;
        Input.Text = inputDefault;
        Ok.Text = ok;
        Cancel.Text = cancel;
    }

    private async void Ok_Clicked(object sender, RoutedEventArgs args)
    {
        var dialogHost = this.FindAncestorOfType<DialogHost>();
        dialogHost.CloseDialogCommand.Execute(Input.Text);
    }

    private async void Cancel_Clicked(object sender, RoutedEventArgs args)
    {
        var dialogHost = this.FindAncestorOfType<DialogHost>();
        dialogHost.CloseDialogCommand.Execute(null);
    }
}
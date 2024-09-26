using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using DialogHostAvalonia;

namespace Dragonfly.ToolsGui.Views.Dialogs;

public partial class MessageBoxDialog : UserControl
{
    public MessageBoxDialog(string message, bool cancelIsVisible = false, string ok = "Ok", string cancel = "Cancel")
    {
        InitializeComponent();
        Message.Text = message;
        Ok.Text = ok;
        Cancel.Text = cancel;

        CancelButton.IsVisible = cancelIsVisible;
    }

    private async void Ok_Clicked(object sender, RoutedEventArgs args)
    {
        var dialogHost = this.FindAncestorOfType<DialogHost>();
        dialogHost.CloseDialogCommand.Execute(true);
    }

    private async void Cancel_Clicked(object sender, RoutedEventArgs args)
    {
        var dialogHost = this.FindAncestorOfType<DialogHost>();
        dialogHost.CloseDialogCommand.Execute(false);
    }
}
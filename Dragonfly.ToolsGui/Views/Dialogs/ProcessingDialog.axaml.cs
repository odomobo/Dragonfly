using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DialogHostAvalonia;
using System.Threading.Channels;

namespace Dragonfly.ToolsGui.Views.Dialogs;

public partial class ProcessingDialog : UserControl
{
    public ProcessingDialog(string message = "Processing...", bool showProgressBar = false)
    {
        InitializeComponent();
        Message.Text = message;
        Progress.IsVisible = showProgressBar;
    }

    private async void Close_Clicked(object sender, RoutedEventArgs args)
    {
        var dialogHost = this.FindAncestorOfType<DialogHost>();
        dialogHost.CloseDialogCommand.Execute(true);
    }

    public void UpdateMessage(string message)
    {
        Dispatcher.UIThread.Post(() => Message.Text = message);
    }

    public void UpdateProgress(int value, int maximum)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Progress.Value = value;
            Progress.Maximum = maximum;
        });
    }

    private bool _hidden = false;
    public void Hide()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_hidden)
                return;

            var dialogHost = this.FindAncestorOfType<DialogHost>();
            dialogHost.CloseDialogCommand.Execute(null);
            _hidden = true;
        });
    }

    public void Finished(string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            FinishedMessage.Text = message;
            FinishedMessage.IsVisible = true;
            FinishedButton.IsVisible = true;
        });
    }
}
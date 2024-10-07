using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DialogHostAvalonia;
using Dragonfly.Tools;
using System.Threading.Channels;

namespace Dragonfly.ToolsGui.Views.Dialogs;

public partial class ProcessingDialog : UserControl, IProgressNotifier
{
    public static ProcessingDialog AddProcessingDialog(DialogHost dh, string message = "Processing...", bool showProgressBar = false)
    {
        var ret = new ProcessingDialog(message, showProgressBar);
        DialogHost.Show(ret, dh);
        return ret;
    }

    public ProcessingDialog(string message = "Processing...", bool showProgressBar = false)
    {
        InitializeComponent();
        Message.Text = message;
        Progress.IsVisible = showProgressBar;
    }

    private async void Close_Clicked(object sender, RoutedEventArgs args)
    {
        // always try to stop flashing, in case we were flashing
        var window = this.FindAncestorOfType<Window>();
        var platformHandle = window.TryGetPlatformHandle();
        if (platformHandle != null)
        {
            WindowsInterop.StopFlashingWindow(platformHandle.Handle);
        }

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
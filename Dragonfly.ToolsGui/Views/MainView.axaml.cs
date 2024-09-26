using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using Dragonfly.ToolsGui.ViewModels;
using Dragonfly.ToolsGui.Views.Dialogs;

namespace Dragonfly.ToolsGui.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private async void OpenMenuItem_Clicked(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            var messageBox = new YesNoMessageBoxView($"Selected a file: {files[0].Name}");
            var result = await DialogHost.Show(messageBox);

            var otherMessageBox = new OkMessageBoxView($"Received result from dialog: {result}");
            result = await DialogHost.Show(otherMessageBox);
        }
        else
        {
            var messageBox = new OkMessageBoxView("No file was selected.");
            var result = await DialogHost.Show(messageBox);
        }
    }
}

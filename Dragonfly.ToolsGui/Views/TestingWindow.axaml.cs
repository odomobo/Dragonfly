using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using Dragonfly.ToolsGui.ViewModels;
using Dragonfly.ToolsGui.Views.Dialogs;

namespace Dragonfly.ToolsGui.Views;

public partial class TestingWindow : Window
{
    public TestingWindow()
    {
        InitializeComponent();

        DropDestinationRect.AddHandler(DragDrop.DropEvent, DropEventHandler);
    }

    private async void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var dragData = new DataObject();
        dragData.Set("ChessMoveSourceSquare", "e4");
        var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
    }

    private void DropEventHandler(object? sender, DragEventArgs e)
    {
        var vm = (TestingViewModel)this.DataContext;
        vm.Drop(sender, e);
    }

    private async void Open_Clicked(object sender, RoutedEventArgs args)
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
            var messageBox = new MessageBoxDialog($"Selected a file: {files[0].Name}", true);
            var result1 = (bool)await DialogHost.Show(messageBox, DH);

            if (result1 == true)
            {
                var inputBox = new InputBoxDialog($"You liked that? Enter your favorite food:");
                var result2 = (string?)await DialogHost.Show(inputBox, DH);

                if (result2 != null)
                {
                    messageBox = new MessageBoxDialog($"Your favorite food is {result2}? I mean, I guess...");
                    await DialogHost.Show(messageBox, DH);
                }
            }
        }
        else
        {
            var messageBox = new MessageBoxDialog("No file was selected.");
            var result = await DialogHost.Show(messageBox, DH);
        }
    }
}
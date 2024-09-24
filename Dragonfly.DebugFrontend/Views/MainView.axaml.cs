using Avalonia.Controls;
using Avalonia.Input;
using Dragonfly.DebugFrontend.ViewModels;

namespace Dragonfly.DebugFrontend.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        // set in xaml???
        //DragDrop.SetAllowDrop(DropDestinationRect, true);
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
        var vm = (MainViewModel)this.DataContext;
        vm.Drop(sender, e);
    }
}

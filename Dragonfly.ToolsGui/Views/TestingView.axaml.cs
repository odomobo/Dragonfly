using Avalonia.Controls;
using Avalonia.Input;
using Dragonfly.ToolsGui.ViewModels;

namespace Dragonfly.ToolsGui.Views;

public partial class TestingView : UserControl
{
    public TestingView()
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
        var vm = (TestingViewModel)this.DataContext;
        vm.Drop(sender, e);
    }
}
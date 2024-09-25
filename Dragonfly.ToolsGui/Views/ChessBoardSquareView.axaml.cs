using Avalonia.Controls;
using Avalonia.Input;
using Dragonfly.ToolsGui.ViewModels;
using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.ToolsGui.Views;

public partial class ChessBoardSquareView : UserControl
{
    public ChessBoardSquareView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, DropEventHandler);
    }

    private ChessBoardSquareViewModel Vm => (ChessBoardSquareViewModel)this.DataContext;

    private async void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var square = Position.IxFromFileRank(Vm.File, Vm.Rank);

        var dragData = new DataObject();
        dragData.Set("ChessMoveSourceSquare", square);
        var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
    }

    private void DropEventHandler(object? sender, DragEventArgs e)
    {
        Vm.Drop(sender, e);
    }
}
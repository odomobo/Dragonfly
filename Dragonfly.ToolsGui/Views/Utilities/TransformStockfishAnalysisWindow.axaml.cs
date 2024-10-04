using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Dragonfly.Tools;
using Dragonfly.ToolsGui.Views.Dialogs;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Dragonfly.ToolsGui.Views.Utilities;

public partial class TransformStockfishAnalysisWindow : Window
{
    private TransformStockfishAnalysisViewModel _vm => (TransformStockfishAnalysisViewModel)DataContext;

    public TransformStockfishAnalysisWindow()
    {
        InitializeComponent();
    }

    private async void StockfishAnalysisFile_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var json = new FilePickerFileType("JSON")
        {
            Patterns = new[] { "*.json" },
        };

        var suggestedPathName = Path.GetDirectoryName(_vm.StockfishAnalysisFile);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(_vm.StockfishAnalysisFile);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open JSON File",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { json, FilePickerFileTypes.All },
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = suggestedPath,
        });

        if (files.Count == 1)
        {
            _vm.StockfishAnalysisFile = files[0].Path.AbsolutePath;
        }
    }

    private async void IntermediaryAnalysisFile_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var json = new FilePickerFileType("JSON")
        {
            Patterns = new[] { "*.json" },
        };

        var suggestedPathName = Path.GetDirectoryName(_vm.IntermediaryAnalysisFile);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(_vm.IntermediaryAnalysisFile);

        // Start async operation to open the dialog.
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Open JSON Output File",
            FileTypeChoices = new List<FilePickerFileType> { json, FilePickerFileTypes.All },
            DefaultExtension = "json",
            ShowOverwritePrompt = true,
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = suggestedPath,
        });

        if (file != null)
        {
            _vm.IntermediaryAnalysisFile = file.Path.AbsolutePath;
        }
    }

    private async void ButtonProcess_Click(object? sender, RoutedEventArgs e)
    {
        var message = ProcessingDialog.AddProcessingDialog(DH, $"Transforming stockfish analysis into intermediary analysis output file.", true);

        PositionAnalysisNodeExtraction.Run(_vm.StockfishAnalysisFile, _vm.IntermediaryAnalysisFile, message);
    }
}
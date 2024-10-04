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

public partial class CaptureStockfishAnalysisWindow : Window
{
    private CaptureStockfishAnalysisViewModel _vm => (CaptureStockfishAnalysisViewModel)DataContext;

    public CaptureStockfishAnalysisWindow()
    {
        InitializeComponent();
    }

    private async void ButtonFenFile_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var fen = new FilePickerFileType("FEN")
        {
            Patterns = new[] { "*.fen" },
        };

        var suggestedPathName = Path.GetDirectoryName(_vm.FenFile);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(_vm.FenFile);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open FEN File",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { fen, FilePickerFileTypes.All },
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = suggestedPath,
        });

        if (files.Count == 1)
        {
            _vm.FenFile = files[0].Path.AbsolutePath;
        }
    }

    private async void ButtonStockfishPath_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var exe = new FilePickerFileType("EXE")
        {
            Patterns = new[] { "*.exe" },
        };

        var suggestedPathName = Path.GetDirectoryName(_vm.StockfishPath);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(_vm.StockfishPath);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Stockfish Executable",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { exe, FilePickerFileTypes.All },
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = suggestedPath,
        });

        if (files.Count == 1)
        {
            _vm.StockfishPath = files[0].Path.AbsolutePath;
        }
    }

    private async void ButtonJsonOutputFile_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var json = new FilePickerFileType("JSON")
        {
            Patterns = new[] { "*.json" },
        };

        var suggestedPathName = Path.GetDirectoryName(_vm.JsonOutputFile);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(_vm.JsonOutputFile);

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
            _vm.JsonOutputFile = file.Path.AbsolutePath;
        }
    }

    private async void ButtonProcess_Click(object? sender, RoutedEventArgs e)
    {
        var message = ProcessingDialog.AddProcessingDialog(DH, $"Processing stockfish analysis, using {_vm.ThreadCount} thread(s), at {_vm.NodeCount} nodes per thread.", true);

        var analysis = new CaptureStockfishAnalysis();
        analysis.Run(_vm.FenFile, _vm.JsonOutputFile, _vm.StockfishPath, _vm.ThreadCount, _vm.NodeCount, message);
    }
}
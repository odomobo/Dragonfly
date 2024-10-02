using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DialogHostAvalonia;
using Dragonfly.Tools;
using Dragonfly.ToolsGui.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace Dragonfly.ToolsGui.Views.Utilities;

public partial class PgnToFenWindow : Window
{
    private PgnToFenViewModel _vm => (PgnToFenViewModel)DataContext;
    public PgnToFenWindow()
    {
        InitializeComponent();
    }

    private async void Closing(object sender, WindowClosingEventArgs e)
    {
        PgnToFenSettings.Default.Save();
    }

    private async void ButtonPgnFile_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var pgn = new FilePickerFileType("PGN")
        {
            Patterns = new[] {"*.pgn"},
        };

        var suggestedPathName = Path.GetDirectoryName(_vm.PgnFile);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(_vm.PgnFile);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open PGN File",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { pgn, FilePickerFileTypes.All },
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = suggestedPath,
        });

        if (files.Count == 1)
        {
            _vm.PgnFile = files[0].Path.AbsolutePath;
        }
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
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Open FEN File",
            FileTypeChoices = new List<FilePickerFileType> { fen, FilePickerFileTypes.All },
            DefaultExtension = "fen",
            ShowOverwritePrompt = true,
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = suggestedPath,
        });

        if (file != null)
        {
            _vm.FenFile = file.Path.AbsolutePath;
        }
    }

    private async void ButtonProcess_Click(object? sender, RoutedEventArgs e)
    {
        //var message = new ProcessingDialog($"Processing reading file {_vm.PgnFile} writing to file {_vm.FenFile}");
        //await DialogHost.Show(message, DH);
        var message = ProcessingDialog.AddProcessingDialog(DH, $"Processing reading file {_vm.PgnFile} writing to file {_vm.FenFile}");

        var pgnFile = _vm.PgnFile;
        var fenFile = _vm.FenFile;
        var t = new Thread(() => ProcessPgnFile(pgnFile, fenFile, message));
        t.Start();
    }

    private void ProcessPgnFile(string pgnFile, string fenFile, IProgressNotifier progress)
    {
        try
        {
            PgnToFen.TransformToFenWithoutDuplicatePositions(pgnFile, fenFile);

            progress.Finished("Successful");
        }
        catch (Exception e)
        {
            progress.Finished(e.ToString());
        }
    }
}
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

        var suggestedPathName = Path.GetDirectoryName(PgnFile.Text);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(PgnFile.Text);

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
            PgnFile.Text = files[0].Path.AbsolutePath;
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

        var suggestedPathName = Path.GetDirectoryName(FenFile.Text);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(FenFile.Text);

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
            FenFile.Text = file.Path.AbsolutePath;
        }
    }

    private async void ButtonProcess_Click(object? sender, RoutedEventArgs e)
    {
        var message = new ProcessingDialog($"Processing reading file {PgnFile.Text} writing to file {FenFile.Text}");
        var pgnFile = PgnFile.Text;
        var fenFile = FenFile.Text;
        var t = new Thread(() => ProcessPgnFile(pgnFile, fenFile, message));
        t.Start();

        await DialogHost.Show(message, DH);
    }

    private void ProcessPgnFile(string pgnFile, string fenFile, ProcessingDialog dialog)
    {
        try
        {
            PgnToFen.TransformToFenWithoutDuplicatePositions(pgnFile, fenFile);

            dialog.Finished("Successful");
        }
        catch (Exception e)
        {
            dialog.Finished(e.ToString());
        }
    }
}
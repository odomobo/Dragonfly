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

public partial class EvaluateSuiteWindow : Window
{
    private EvaluateSuiteViewModel _vm => (EvaluateSuiteViewModel)DataContext;

    public EvaluateSuiteWindow()
    {
        InitializeComponent();
    }

    private async void InputFile_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var json = new FilePickerFileType("JSON")
        {
            Patterns = new[] { "*.json" },
        };

        var suggestedPathName = Path.GetDirectoryName(_vm.InputFile);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(_vm.InputFile);

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
            _vm.InputFile = files[0].Path.AbsolutePath;
        }
    }

    private async void OutputFile_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var json = new FilePickerFileType("JSON")
        {
            Patterns = new[] { "*.json" },
        };

        var suggestedPathName = Path.GetDirectoryName(_vm.OutputFile);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(_vm.OutputFile);

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
            _vm.OutputFile = file.Path.AbsolutePath;
        }
    }

    private async void ButtonProcess_Click(object? sender, RoutedEventArgs e)
    {
        var progress = ProcessingDialog.AddProcessingDialog(DH, $"Running engine evaluation against position file: {_vm.InputFile}", true);

        var platformHandle = this.TryGetPlatformHandle();

        var evaluationSuite = new EvaluationSuite(true); // TODO: parameterize with inputs
        evaluationSuite.Run(_vm.InputFile, _vm.OutputFile, _vm.ThreadCount, _vm.NodeCount, progress, platformHandle?.Handle);
    }
}
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

    private async void InputFolder_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var suggestedPathName = Path.GetDirectoryName(_vm.InputFolder);
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        var suggestedFileName = Path.GetFileName(_vm.InputFolder);

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Analysis Folder",
            AllowMultiple = false,
            //SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = suggestedPath,
        });

        if (folders.Count == 1)
        {
            _vm.InputFolder = folders[0].Path.AbsolutePath;
        }
    }

    private async void ButtonProcess_Click(object? sender, RoutedEventArgs e)
    {
        var progress = ProcessingDialog.AddProcessingDialog(DH, $"Running engine evaluation against analysis folder: {_vm.InputFolder}", true);

        var platformHandle = this.TryGetPlatformHandle();

        var evaluationSuite = new EvaluationSuite(true); // TODO: parameterize with inputs
        evaluationSuite.Run(_vm.InputFolder, _vm.ThreadCount, _vm.NodeCount, progress, platformHandle?.Handle);
    }
}
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

public partial class OpenEvaluationWindow : Window
{
    private OpenEvaluationViewModel _vm => (OpenEvaluationViewModel)DataContext;

    public OpenEvaluationWindow()
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

    private async void ButtonGoldenEvaluationFile_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var evJson = new FilePickerFileType("Evaluation")
        {
            Patterns = new[] { "*.ev.json" },
        };

        var suggestedPathName = _vm.InputFolder;
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        //var suggestedFileName = Path.GetFileName(_vm.FenFile);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Golden Evaluation File",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { evJson, FilePickerFileTypes.All },
            //SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = suggestedPath,
        });

        if (files.Count == 1)
        {
            _vm.GoldenEvaluationFile = files[0].Path.AbsolutePath;
        }
    }

    private async void ButtonNewEvaluationFile_Click(object? sender, RoutedEventArgs e)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        var evJson = new FilePickerFileType("Evaluation")
        {
            Patterns = new[] { "*.ev.json" },
        };

        var suggestedPathName = _vm.InputFolder;
        var suggestedPath = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPathName);
        //var suggestedFileName = Path.GetFileName(_vm.FenFile);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open New Evaluation File",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { evJson, FilePickerFileTypes.All },
            //SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = suggestedPath,
        });

        if (files.Count == 1)
        {
            _vm.NewEvaluationFile = files[0].Path.AbsolutePath;
        }
    }

    private async void ButtonOpenEvaluation_Click(object? sender, RoutedEventArgs e)
    {
        // open window
        var window = new EvaluationWindow();
        window.DataContext = new EvaluationViewModel(_vm.InputFolder, _vm.GoldenEvaluationFile, _vm.NewEvaluationFile);
        window.Show();

        // close self
        Close();
    }
}
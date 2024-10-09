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

public partial class EvaluationWindow : Window
{
    private EvaluationViewModel _vm => (EvaluationViewModel)DataContext;

    public EvaluationWindow()
    {
        InitializeComponent();
    }
}
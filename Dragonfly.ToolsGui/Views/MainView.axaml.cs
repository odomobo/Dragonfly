using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using DialogHostAvalonia;
using Dragonfly.Tools;
using Dragonfly.ToolsGui.ViewModels;
using Dragonfly.ToolsGui.Views.Dialogs;
using Dragonfly.ToolsGui.Views.Utilities;
using System;

namespace Dragonfly.ToolsGui.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private async void PgnToFen_Clicked(object sender, RoutedEventArgs args)
    {
        var window = new PgnToFenWindow();
        window.DataContext = new PgnToFenViewModel();
        window.Show();
    }

    private async void CaptureStockfishAnalysis_Clicked(object sender, RoutedEventArgs args)
    {
        var window = new CaptureStockfishAnalysisWindow();
        window.DataContext = new CaptureStockfishAnalysisViewModel();
        window.Show();
    }

    private async void TransformStockfishAnalysis_Clicked(object sender, RoutedEventArgs args)
    {
        var window = new TransformStockfishAnalysisWindow();
        window.DataContext = new TransformStockfishAnalysisViewModel();
        window.Show();
    }

    private async void EvaluateSuite_Clicked(object sender, RoutedEventArgs args)
    {
        var window = new EvaluateSuiteWindow();
        window.DataContext = new EvaluateSuiteViewModel();
        window.Show();
    }

    private async void Testing_Clicked(object sender, RoutedEventArgs args)
    {
        var window = new TestingWindow();
        window.DataContext = new TestingViewModel();
        window.Show();
    }
}

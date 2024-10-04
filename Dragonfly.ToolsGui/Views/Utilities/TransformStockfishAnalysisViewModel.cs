using Dragonfly.ToolsGui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.ToolsGui.Views.Utilities
{
    internal class TransformStockfishAnalysisViewModel : ViewModelBase
    {
        private TransformStockfishAnalysisSettings _settings => TransformStockfishAnalysisSettings.Default;

        public string StockfishAnalysisFile
        {
            get => _settings.StockfishAnalysisFile;
            set
            {
                if (_settings.StockfishAnalysisFile == value)
                    return;
                _settings.StockfishAnalysisFile = value;
                _settings.Save();
                OnPropertyChanged(nameof(StockfishAnalysisFile));
            }
        }

        public string IntermediaryAnalysisFile
        {
            get => _settings.IntermediaryAnalysisFile;
            set
            {
                if (_settings.IntermediaryAnalysisFile == value)
                    return;
                _settings.IntermediaryAnalysisFile = value;
                _settings.Save();
                OnPropertyChanged(nameof(IntermediaryAnalysisFile));
            }
        }
    }
}

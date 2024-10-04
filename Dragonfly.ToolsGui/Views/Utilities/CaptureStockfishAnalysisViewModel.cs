using Dragonfly.ToolsGui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.ToolsGui.Views.Utilities
{
    internal class CaptureStockfishAnalysisViewModel : ViewModelBase
    {
        private CaptureStockfishAnalysisSettings _settings => CaptureStockfishAnalysisSettings.Default;
        public string FenFile
        {
            get => _settings.FenFile;
            set
            {
                if (_settings.FenFile == value)
                    return;
                _settings.FenFile = value;
                _settings.Save();
                OnPropertyChanged(nameof(FenFile));
            }
        }

        public string StockfishPath
        {
            get => _settings.StockfishPath;
            set
            {
                if (_settings.StockfishPath == value)
                    return;
                _settings.StockfishPath = value;
                _settings.Save();
                OnPropertyChanged(nameof(StockfishPath));
            }
        }

        public string JsonOutputFile
        {
            get => _settings.JsonOutputFile;
            set
            {
                if (_settings.JsonOutputFile == value)
                    return;
                _settings.JsonOutputFile = value;
                _settings.Save();
                OnPropertyChanged(nameof(JsonOutputFile));
            }
        }

        public int ThreadCount
        {
            get => _settings.ThreadCount;
            set
            {
                if (_settings.ThreadCount == value)
                    return;
                _settings.ThreadCount = value;
                _settings.Save();
                OnPropertyChanged(nameof(ThreadCount));
            }
        }

        public int NodeCount
        {
            get => _settings.NodeCount;
            set
            {
                if (_settings.NodeCount == value)
                    return;
                _settings.NodeCount = value;
                _settings.Save();
                OnPropertyChanged(nameof(NodeCount));
            }
        }
    }
}

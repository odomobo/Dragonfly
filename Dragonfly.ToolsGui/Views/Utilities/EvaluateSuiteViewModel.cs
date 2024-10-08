using Dragonfly.ToolsGui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.ToolsGui.Views.Utilities
{
    internal class EvaluateSuiteViewModel : ViewModelBase
    {
        private EvaluateSuiteSettings _settings => EvaluateSuiteSettings.Default;

        public string InputFolder
        {
            get => _settings.InputFolder;
            set
            {
                if (_settings.InputFolder == value)
                    return;
                _settings.InputFolder = value;
                _settings.Save();
                OnPropertyChanged(nameof(InputFolder));
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

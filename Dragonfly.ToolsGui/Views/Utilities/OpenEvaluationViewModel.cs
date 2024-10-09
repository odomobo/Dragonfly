using Dragonfly.ToolsGui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.ToolsGui.Views.Utilities
{
    internal class OpenEvaluationViewModel : ViewModelBase
    {
        private OpenEvaluationSettings _settings => OpenEvaluationSettings.Default;

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

        public string GoldenEvaluationFile
        {
            get => _settings.GoldenEvaluation;
            set
            {
                if (_settings.GoldenEvaluation == value)
                    return;
                _settings.GoldenEvaluation = value;
                _settings.Save();
                OnPropertyChanged(nameof(GoldenEvaluationFile));
            }
        }

        public string NewEvaluationFile
        {
            get => _settings.NewEvaluation;
            set
            {
                if (_settings.NewEvaluation == value)
                    return;
                _settings.NewEvaluation = value;
                _settings.Save();
                OnPropertyChanged(nameof(NewEvaluationFile));
            }
        }
    }
}

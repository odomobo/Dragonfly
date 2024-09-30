using Dragonfly.Engine.CoreTypes;
using Dragonfly.ToolsGui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.ToolsGui.Views.Utilities
{
    internal class PgnToFenViewModel : ViewModelBase
    {
        private readonly PgnToFenSettings _settings = PgnToFenSettings.Default;

        public string FenFile
        {
            get => _settings.FenFile;
            set
            {
                _settings.FenFile = value;
                OnPropertyChanged(nameof(FenFile));
            }
        }

        public string PgnFile
        {
            get => _settings.PgnFile;
            set
            {
                _settings.PgnFile = value;
                OnPropertyChanged(nameof(PgnFile));
            }
        }

    }
}

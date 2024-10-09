using Dragonfly.Tools;
using Dragonfly.ToolsGui.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.ToolsGui.Views.Utilities
{
    internal record EvaluationEntry(int Index, string Fen, double? GoldenLoss, double? NewLoss, double? RelativeLoss);

    internal class EvaluationViewModel : ViewModelBase
    {
        public string Foo => "Bar";

        //private EvaluationSettings _settings => EvaluationSettings.Default;

        public ObservableCollection<EvaluationEntry> EvaluationEntries
        {
            get => _evaluationEntries;
            set
            {
                _evaluationEntries = value;
                OnPropertyChanged(nameof(EvaluationEntries));
            }
        }
        private ObservableCollection<EvaluationEntry> _evaluationEntries;

        public EvaluationViewModel(string evaluationFolder, string goldenEvaluationFile, string newEvaluationFile)
        {
            var positionAnalysisNodes = FileHandling.GetPositionAnalysisNodesFromEvaluationFolder(evaluationFolder);
            var goldenEvaluation = FileHandling.GetEvaluationNodesFromFile(goldenEvaluationFile);
            var newEvaluation = FileHandling.GetEvaluationNodesFromFile(newEvaluationFile);

            var goldenEvaluationDictionary = goldenEvaluation.ToDictionary(e => e.Hash);
            var newEvaluationDictionary = newEvaluation.ToDictionary(e => e.Hash);

            var evaluationEntries = new ObservableCollection<EvaluationEntry>();

            int i = 0;
            foreach (var positionAnalysisNode in positionAnalysisNodes)
            {
                goldenEvaluationDictionary.TryGetValue(positionAnalysisNode.Hash, out var goldenEvaluationEntry);
                newEvaluationDictionary.TryGetValue(positionAnalysisNode.Hash, out var newEvaluationEntry);

                var goldenEvaluationLoss = GetLoss(positionAnalysisNode, goldenEvaluationEntry);
                var newEvaluationLoss = GetLoss(positionAnalysisNode, newEvaluationEntry);
                double? relativeLoss = null;
                if (goldenEvaluationLoss != null && newEvaluationLoss != null)
                {
                    relativeLoss = newEvaluationLoss - goldenEvaluationLoss;
                }

                evaluationEntries.Add(new EvaluationEntry(i++, positionAnalysisNode.Fen, goldenEvaluationLoss, newEvaluationLoss, relativeLoss));
            }
            EvaluationEntries = evaluationEntries;
        }

        public double? GetLoss(PositionAnalysisNode positionAnalysisNode, EvaluationNode? entry)
        {
            if (entry == null)
                return null;

            var bestMoveScore = positionAnalysisNode.MoveScores.Max(ms => ms.Value);
            var entryMoveScore = positionAnalysisNode.MoveScores[entry.Move];
            return GetScaledLoss(bestMoveScore, entryMoveScore);
        }

        public double GetScaledLoss(int bestMoveScore, int entryMoveScore)
        {
            return ScaleScore(bestMoveScore) - ScaleScore(entryMoveScore);
        }

        public double ScaleScore(int score)
        {
            // Using formula:
            //   f(x) = 1 / [1 + e ^ -(x / 275)]
            return 1d / (1d + Math.Pow(Math.E, -(score / 275d)));
        }
    }
}

using Dragonfly.Engine;
using Dragonfly.Engine.Evaluation;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveGeneration;
using Dragonfly.Engine.MoveOrdering;
using Dragonfly.Engine.Searching;
using Dragonfly.Engine.TimeStrategies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dragonfly.Tools
{
    public class EvaluationNode
    {
        public string Fen { get; set; }
        public ulong Hash { get; set; }
        public string Move { get; set; }
        public int NodeCount { get; set; }
    }

    public class EvaluationSuite(bool _writeIndented = false)
    {
        private int _completedCount = 0;
        private int _totalCount;
        public void Run(string analysisFolder, int threadCount, int nodeCount, IProgressNotifier progress, nint? windowHandle)
        {
            Task.Run(() =>
            {
                try
                {
                    WindowsInterop.KeepAwake();
                    InnerRun(analysisFolder, threadCount, nodeCount, progress);
                }
                catch (Exception ex)
                {
                    progress.Finished(ex.ToString());
                }
                finally
                {
                    WindowsInterop.AllowSleep();
                    if (windowHandle != null) 
                    {
                        WindowsInterop.FlashWindow(windowHandle.Value);
                    }
                }
            });
        }

        private void InnerRun(string analysisFolder, int threadCount, int nodeCount, IProgressNotifier progress)
        {
            var sw = Stopwatch.StartNew();

            var analysisFile = Path.Combine(analysisFolder, "_evaluation_positions.ep.json"); // TODO: make a constant somewhere

            // Open analysis file, slurp json
            List<PositionAnalysisNode> positionAnalyses;
            using (var filestream = new FileStream(analysisFile, FileMode.Open))
            {
                positionAnalyses = JsonSerializer.Deserialize<List<PositionAnalysisNode>>(filestream);
            }

            _totalCount = positionAnalyses.Count;
            progress.UpdateProgress(0, _totalCount);

            // Parallel map, position from analysis json to evaluation node output
            var options = new ParallelOptions { MaxDegreeOfParallelism = threadCount };
            var evaluations = MyParallel.Map(positionAnalyses, options, analysis => EvaluatePosition(analysis, nodeCount, progress));

            // TODO: store actual score in here instead of xx.xxx
            var outputFileName = $"xx.xxx_{VersionInfo.Version}-({VersionInfo.Codename})_{nodeCount.ToShortString()}_{DateTime.Now:yyyy-MM-dd_hhmm}.ev.json";
            var outputFile = Path.Combine(analysisFolder, outputFileName);

            using (var filestream = File.Create(outputFile))
            {
                JsonSerializer.Serialize(filestream, evaluations, new JsonSerializerOptions { WriteIndented = _writeIndented });
            }

            progress.Finished($"Wrote file: {Path.GetFileName(outputFile)}\nCompleted in {sw.Elapsed}");
        }

        private EvaluationNode EvaluatePosition(PositionAnalysisNode analysis, int nodeCount, IProgressNotifier progress)
        {
            // set up engine
            var searchWorkerThread = new SearchWorkerThread();
            var strategy = new NodeCountStrategy(nodeCount);
            var moveGen = new MoveGenerator();
            var evaluator = new Evaluator();
            var promotionMvvLvaMoveOrderer = new CompositeMoveOrderer(new IMoveOrderer[] { new PromotionsOrderer(), new MvvLvaOrderer() });
            var qSearch = new SimpleQSearch(evaluator, moveGen, promotionMvvLvaMoveOrderer, CompositeMoveOrderer.NullMoveOrderer);
            //var search = new SimpleAlphaBetaSearch(moveGen, evaluator, qSearch);
            var search = new IidAlphaBetaSearch(moveGen, new StandardGameTerminator(), evaluator, qSearch, promotionMvvLvaMoveOrderer);
            var bestMoveProtocol = new GetBestMoveProtocol();

            // calculate position
            var position = BoardParsing.PositionFromFen(analysis.Fen);
            searchWorkerThread.StartSearch(search, position, strategy, bestMoveProtocol);
            searchWorkerThread.Join();
            searchWorkerThread.Exit();

            Interlocked.Increment(ref _completedCount);
            progress.UpdateProgress(_completedCount, _totalCount);

            return new EvaluationNode
            {
                Fen = analysis.Fen,
                Hash = analysis.Hash,
                Move = bestMoveProtocol.BestMove.Value.ToString(),
                NodeCount = nodeCount,
            };
        }
    }
}

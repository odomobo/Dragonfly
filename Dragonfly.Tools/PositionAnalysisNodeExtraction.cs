using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public class PositionAnalysisNode
    {
        public string Fen { get; set; }
        public ulong Hash { get; set; }
        public Dictionary<string, Score> MoveScores { get; set; }
    }

    static public class PositionAnalysisNodeExtraction
    {
        public static void Run(string stockfishJsonFile, string positionAnalysisJsonFile, IProgressNotifier progress)
        {
            Task.Run(() =>
            {
                try
                {
                    WindowsInterop.KeepAwake();
                    InnerRun(stockfishJsonFile, positionAnalysisJsonFile, progress);
                    progress.Finished("Completed!");
                }
                catch (Exception ex)
                {
                    progress.Finished(ex.ToString());
                }
                finally
                {
                    WindowsInterop.AllowSleep();
                }
            });
        }

        private static void InnerRun(string stockfishJsonFile, string positionAnalysisJsonFile, IProgressNotifier progress)
        {
            List<StockfishRawAnalysis> stockfishAnalyses;
            using (var filestream = new FileStream(stockfishJsonFile, FileMode.Open))
            {
                stockfishAnalyses = JsonSerializer.Deserialize<List<StockfishRawAnalysis>>(filestream);
            }

            var positionAnalyses = new List<PositionAnalysisNode>();

            int i = 0;
            foreach (var stockfishAnalysis in stockfishAnalyses)
            {
                progress.UpdateProgress(i++, stockfishAnalyses.Count);

                positionAnalyses.Add(ExtractNodeFromStockfishRawAnalysis(stockfishAnalysis));
            }

            progress.UpdateProgress(i, stockfishAnalyses.Count);

            using (var filestream = File.Create(positionAnalysisJsonFile))
            {
                JsonSerializer.Serialize(filestream, positionAnalyses);
            }
        }

        public static PositionAnalysisNode ExtractNodeFromStockfishRawAnalysis(StockfishRawAnalysis stockfishRawAnalysis)
        {
            var fen = stockfishRawAnalysis.Fen;
            var position = BoardParsing.PositionFromFen(fen);
            var hash = position.ZobristHash;

            var lines = stockfishRawAnalysis.EngineOutput.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            var depthScoredMoves = new Dictionary<string, (int depth, Score score)>();

            foreach (var line in lines)
            { 
                if (!IsSearchInfo(line)) continue;

                var depth = GetDepth(line);
                var score = ExtractRelativeScore(line, position.GamePly);
                var move = GetMove(line);

                // if there isn't already a move, or if the recorded move is shallower than the current move, then store the current move
                if (!depthScoredMoves.ContainsKey(move) || depth >= depthScoredMoves[move].depth)
                {
                    depthScoredMoves[move] = (depth, score);
                }
            }

            var scoredMoves = depthScoredMoves.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.score);

            return new PositionAnalysisNode
            {
                Fen = fen,
                Hash = hash,
                MoveScores = scoredMoves,
            };
        }

        private static readonly Regex ScoreRegex = new Regex(@" score (\S+) (\S+)");
        public static Score ExtractRelativeScore(string line, int gamePly)
        {
            var match = ScoreRegex.Match(line);
            if (!match.Success)
                throw new Exception($"Could not parse score out of info line: {line}");

            var type = match.Groups[1].Value;
            var scoreStr = match.Groups[2].Value;
            var score = int.Parse(scoreStr);

            switch (type)
            {
                case "cp":
                    return new Score(score);
                case "mate":
                    if (score > 0)
                        return Score.GetMateScore(Color.White, gamePly + score);
                    else
                        return Score.GetMateScore(Color.Black, gamePly - score);
                default:
                    throw new Exception($"Unknown score type: {type}");
            }
        }

        private static readonly Regex IsSearchInfoRegex = new Regex(@"^info .* score .* pv ");
        public static bool IsSearchInfo(string line)
        {
            return IsSearchInfoRegex.IsMatch(line);
        }

        private static readonly Regex DepthRegex = new Regex(@" depth (\S+)");
        public static int GetDepth(string line)
        {
            var match = DepthRegex.Match(line);
            if (!match.Success)
                throw new Exception($"Could not parse depth out of info line: {line}");

            return int.Parse(match.Groups[1].Value);
        }

        private static readonly Regex MoveRegex = new Regex(@" pv (\S+)");
        public static string GetMove(string line)
        {
            var match = MoveRegex.Match(line);
            if (!match.Success)
                throw new Exception($"Could not parse pv move out of info line: {line}");

            return match.Groups[1].Value;
        }
    }
}

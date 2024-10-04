using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public class StockfishRawAnalysis
    {
        public string Fen { get; set; }
        public string EngineOutput { get; set; }
    }

    public class CaptureStockfishAnalysis
    {
        private int _completedCount = 0;

        public void Run(string fenFile, string jsonOutputFile, string stockfishFile, int threadCount, int nodeCount, IProgressNotifier progress)
        {
            Task.Run(() =>
            {
                _completedCount = 0;

                try
                {
                    MachineSleep.KeepAwake();
                    InnerRun(fenFile, jsonOutputFile, stockfishFile, threadCount, nodeCount, progress);
                    progress.Finished("Completed!");
                }
                catch (Exception ex)
                {
                    progress.Finished(ex.ToString());
                }
                finally
                {
                    MachineSleep.AllowSleep();
                }
            });
        }

        private void InnerRun(string fenFile, string jsonOutputFile, string stockfishFile, int threadCount, int nodeCount, IProgressNotifier progress)
        {
            var analyses = new List<StockfishRawAnalysis>();

            // first, read FEN file
            using (var reader = new StreamReader(fenFile))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    analyses.Add(new StockfishRawAnalysis { Fen = line });
                }
            }

            var options = new ParallelOptions { MaxDegreeOfParallelism = threadCount };
            var task = Task.Run(() => Parallel.ForEach(analyses, options, analysis => AnalyzeFen(analysis, stockfishFile, nodeCount)));

            while (true)
            {
                progress.UpdateProgress(_completedCount, analyses.Count);
                if (task.Wait(TimeSpan.FromSeconds(1)))
                {
                    progress.UpdateProgress(_completedCount, analyses.Count);

                    var result = task.Result;
                    if (result.IsCompleted)
                    {
                        break;
                    }
                    else
                    {
                        progress.Finished("Process was stopped early for some reason...");
                        return;
                    }
                }
            }

            using (var filestream = File.Create(jsonOutputFile))
            {
                JsonSerializer.Serialize(filestream, analyses, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        private void AnalyzeFen(StockfishRawAnalysis analysis, string stockfishFile, int nodeCount)
        {
            var stockfish = new ProcessWrapper(stockfishFile);

            stockfish.WriteLine("uci");
            stockfish.ReadUntil("uciok");

            stockfish.WriteLine("setoption name MultiPV value 256");
            stockfish.WriteLine("isready");
            stockfish.ReadUntil("readyok");

            stockfish.WriteLine($"position fen {analysis.Fen}");
            stockfish.WriteLine($"go nodes {nodeCount}");
            analysis.EngineOutput = stockfish.ReadUntil(new Regex("^bestmove "));

            stockfish.WriteLine("quit");

            Interlocked.Increment(ref _completedCount);
        }
    }
}

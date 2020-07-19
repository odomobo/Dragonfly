using System;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.MoveGeneration;

namespace Dragonfly.Engine.Benchmark
{
    public class SpeedColumn : IColumn
    {
        public SpeedColumn()
        {
            Id = nameof(TagColumn) + "." + ColumnName;
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var methodName = benchmarkCase.Descriptor.WorkloadMethod.Name;
            var nodesStr = methodName.Split('_')[1];
            var nodes = int.Parse(nodesStr);

            // Mean is represented in nanoseconds
            var seconds = summary[benchmarkCase].ResultStatistics.Mean / 1_000_000_000;
            var knodes = nodes / 1000d;
            return (knodes / seconds).ToString("N2");
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) =>
            GetValue(summary, benchmarkCase);

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public bool IsAvailable(Summary summary) => true;

        public string Id { get; }
        public string ColumnName => "Mean KNps";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 0;
        public bool IsNumeric => false; // TODO:????
        public UnitType UnitType => UnitType.Dimensionless; // TODO:????
        public string Legend => $"Mean kilonodes per second";
    }

    
    public class PerftBenchConfig : ManualConfig
    {
        public PerftBenchConfig()
        {
            AddColumn(new SpeedColumn()); // TODO: how to get this dynamically?
        }
    }

    //[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.CoreRt31)]
    [MaxRelativeError(0.005)]
    [MaxIterationCount(200)]
    [Config(typeof(PerftBenchConfig))]
    public class PerftBench
    {
        private const string OpeningFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const string MidgameFen = "r1b2rk1/4nppp/p3p3/2qpP3/8/2N2N2/PP3PPP/2RQ1RK1 b - - 3 14";
        private const string EndgameFen = "5n2/R7/4pk2/8/5PK1/8/8/8 b - - 0 1";

        private Perft _perft;
        private Position _openingPosition;
        private Position _midgamePosition;
        private Position _endgamePosition;

        [GlobalSetup]
        public void Setup()
        {
            var moveGen = new MoveGenerator();
            _perft = new Perft(moveGen);

            _openingPosition = BoardParsing.PositionFromFen(OpeningFen);
            _midgamePosition = BoardParsing.PositionFromFen(MidgameFen);
            _endgamePosition = BoardParsing.PositionFromFen(EndgameFen);
        }

        [Benchmark]
        public void OpeningD5_4865609()
        {
            int nodes = _perft.GoPerft(_openingPosition, 5);
            if (nodes != 4865609)
                throw new Exception($"Result {nodes} needs to match method name {nameof(OpeningD5_4865609)}");
        }

        [Benchmark]
        public void MidgameD4_1162726()
        {
            int nodes = _perft.GoPerft(_midgamePosition, 4);
            if (nodes != 1162726)
                throw new Exception($"Result {nodes} needs to match method name {nameof(MidgameD4_1162726)}");
        }

        [Benchmark]
        public void EndgameD6_2024953()
        {
            int nodes = _perft.GoPerft(_endgamePosition, 6);
            if (nodes != 2024953)
                throw new Exception($"Result {nodes} needs to match method name {nameof(EndgameD6_2024953)}");
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PerftBench>();
        }
    }
}

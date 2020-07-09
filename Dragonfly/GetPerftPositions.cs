using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Evaluation;

namespace Dragonfly
{
    // TODO: clean up or remove this class; it's very rough and dirty
    static class GetPerftPositions
    {
        public static void DumpEvalsForEachPosition()
        {
            var evaluator = new Evaluator();

            foreach (var (fen, _) in GetPerftCasesEnumerator())
            {
                var position = BoardParsing.PositionFromFen(fen);

                Console.WriteLine($"{fen} {evaluator.Evaluate(position)}");
            }
        }

        public static IEnumerable<(string fen, string depths)> GetPerftCasesEnumerator()
        {
            foreach (var line in GetResourceEnumerator("Dragonfly.perfts.epd"))
            {
                var splitLine = line.Split(';', 2);

                yield return (splitLine[0], splitLine[1]);
            }
        }

        private static IEnumerable<string> GetResourceEnumerator(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream!))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        break;

                    yield return line;
                }
            }
        }
    }
}

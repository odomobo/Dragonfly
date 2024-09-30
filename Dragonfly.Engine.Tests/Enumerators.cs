using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Dragonfly.Engine.Tests
{
    internal class Enumerators
    {
        public static IEnumerable<string> GetPerftCasesEnumerator()
        {
            foreach (var line in GetResourceEnumerator("Dragonfly.Engine.Tests.perfts.epd"))
            {
                if (string.IsNullOrWhiteSpace(line))
                        continue;

                yield return line;
            }
        }

        public static IEnumerable<string> GetPgnsEnumerator()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream("Dragonfly.Engine.Tests.testPgns.pgn");
            using StreamReader reader = new StreamReader(stream);
            // this is silly, but let's do it. Basically, this lets the stream and streamreader stay open until we've read all the PGNs
            foreach (var pgn in Pgn.SplitPgnStreamIntoPgns(reader))
            {
                yield return pgn;
            }
        }
        private static IEnumerable<string> GetResourceEnumerator(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        yield break;

                    yield return line;
                }
            }
        }
    }
}

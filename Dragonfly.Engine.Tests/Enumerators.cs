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


        private enum PgnParserState
        {
            HeaderReady,
            HeaderProcessing,
            MovesReady,
            MovesProcessing,
        }
        public static IEnumerable<string> GetPgnsEnumerator()
        {
            var sb = new StringBuilder();
            var state = PgnParserState.HeaderReady;
            foreach (var line in GetResourceEnumerator("Dragonfly.Engine.Tests.testPgns.pgn"))
            {
                // first, handle state transitions
                if (string.IsNullOrWhiteSpace(line)) 
                {
                    switch (state)
                    {
                        // header ready stays in state
                        case PgnParserState.HeaderReady:
                            break;
                        // header processing transitions to next state
                        case PgnParserState.HeaderProcessing:
                            state = PgnParserState.MovesReady;
                            break;
                        // moves ready stays in state
                        case PgnParserState.MovesReady:
                            break;
                        // moves processing transitions to next state, including flushing pgn
                        case PgnParserState.MovesProcessing:
                            yield return sb.ToString();
                            sb.Clear();
                            state = PgnParserState.HeaderReady;
                            break;
                    }
                }
                else if (state is PgnParserState.HeaderReady or PgnParserState.HeaderProcessing) 
                {
                    state = PgnParserState.HeaderProcessing;
                    sb.AppendLine(line);
                }
                else if (state is PgnParserState.MovesReady or PgnParserState.MovesProcessing)
                {
                    bool isFirstLine = state == PgnParserState.MovesReady;
                    state = PgnParserState.MovesProcessing;
                    
                    if (isFirstLine)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(line);
                }
                else
                {
                    throw new Exception("Should not be able to reach here");
                }
            }

            // flush
            if (state == PgnParserState.MovesProcessing)
            {
                yield return sb.ToString();
                // not really necessary
                sb.Clear();
                state = PgnParserState.HeaderReady;
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
                    if (line == null)
                        yield break;

                    yield return line;
                }
            }
        }
    }
}

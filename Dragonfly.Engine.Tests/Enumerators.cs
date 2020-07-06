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
            return GetResourceEnumerator("Dragonfly.Engine.Tests.perfts.epd");
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

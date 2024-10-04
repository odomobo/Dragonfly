using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public class ProcessWrapper
    {
        private readonly Process _process;

        public TimeSpan Timeout = TimeSpan.FromMinutes(1);

        public ProcessWrapper(string processFile, string arguments = "", TimeSpan? timeout = null)
        {
            if (timeout != null)
                Timeout = timeout.Value;

            var process = Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,

                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,

                FileName = processFile,
                Arguments = arguments,
            });

            if (process == null)
            {
                throw new Exception($"Process could not be started; processFile: \"{processFile}\"; arguments: \"{arguments}\"");
            }

            process.StandardInput.AutoFlush = true;

            _process = process;
        }

        public string ReadUntil(string expectedLine)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var line = TimedReadLine(_process.StandardOutput.ReadLine);
                sb.AppendLine(line);

                if (line == expectedLine)
                {
                    return sb.ToString();
                }
            }
        }

        public string ReadUntil(Regex regex)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var line = TimedReadLine(_process.StandardOutput.ReadLine);
                sb.AppendLine(line);

                if (regex.IsMatch(line))
                {
                    return sb.ToString();
                }
            }
        }

        private string? TimedReadLine(Func<string?> readLineFunc)
        {
            var task = Task.Run(readLineFunc);
            task.Wait(Timeout);
            if (task.IsCompleted)
            {
                return task.Result;
            }
            else
            {
                _process.Kill();
                try
                {
                    task.Wait();
                }
                catch (Exception) { }

                throw new TimeoutException($"Timed out when attempting to read from process; timeout set to {Timeout}");
            }
        }

        public void Write(string data)
        {
            _process.StandardInput.Write(data);
        }

        public void WriteLine(string line)
        {
            _process.StandardInput.WriteLine(line);
        }
    }
}

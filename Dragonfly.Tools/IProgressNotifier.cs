using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public interface IProgressNotifier
    {
        void UpdateMessage(string message);
        void UpdateProgress(int current, int max);
        void Finished(string finishedMessage);
    }

    public class DummyProgressNotifier : IProgressNotifier
    {
        public void UpdateMessage(string message)
        {
        }

        public void UpdateProgress(int current, int max)
        {
        }

        public void Finished(string finishedMessage)
        {
        }
    }
}

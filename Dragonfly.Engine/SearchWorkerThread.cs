using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine
{
    public sealed class SearchWorkerThread
    {
        private enum State
        {
            Waiting,
            Searching,
            Exiting,
        }

        private readonly Thread _workThread;

        /// <summary>
        /// When worker state is Searching, no thread is allowed to modify the search parameters (_search, _position, etc.)
        /// This is only allowed when worker state is Waiting.
        ///
        /// The work thread listens for a signal that worker state is now Searching, and then it will begin the search.
        ///
        /// In order to interrupt a search, _timeStrategy must be sent the ForceStop signal. Once the search has concluded, then the worker
        /// thread will set worker state to Waiting.
        ///
        /// When the worker thread sees that worker state is exiting, then it'll exit and stop the thread.
        /// </summary>
        private State _workerState;

        private ISearch _search;
        private Position _position;
        private ITimeStrategy _timeStrategy;
        private Statistics.PrintInfoDelegate _printInfoDelegate;
        private Statistics.PrintBestMoveDelegate _printBestMoveDelegate;

        public SearchWorkerThread()
        {
            _workThread = new Thread(Loop);
            lock (_workThread)
            {
                _workerState = State.Waiting;
            }
            _workThread.Start();
        }

        /// <summary>
        /// If the thread is already searching, this will block until it naturally stops searching.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="position"></param>
        /// <param name="timeStrategy"></param>
        /// <param name="printInfoDelegate"></param>
        /// <param name="printBestMoveDelegate"></param>
        public void StartSearch(
            ISearch search,
            Position position,
            ITimeStrategy timeStrategy,
            Statistics.PrintInfoDelegate printInfoDelegate,
            Statistics.PrintBestMoveDelegate printBestMoveDelegate
        )
        {
            lock (_workThread)
            {
                while (true)
                {
                    if (_workerState == State.Waiting)
                        break;

                    Monitor.Wait(_workThread);
                }

                // worker state is now Waiting

                _search = search;
                _position = position;
                _timeStrategy = timeStrategy;
                _printInfoDelegate = printInfoDelegate;
                _printBestMoveDelegate = printBestMoveDelegate;

                _workerState = State.Searching;
                Monitor.PulseAll(_workThread);
            }
        }

        public void StopSearch()
        {
            lock (_workThread)
            {
                while (true)
                {
                    if (_workerState != State.Searching)
                        return;

                    _timeStrategy.ForceStop();
                    Monitor.Wait(_workThread);
                }
            }
        }

        public void Exit()
        {
            lock (_workThread)
            {
                // this should never loop more than once, because after StopSearch(), the state should be Waiting
                while (_workerState == State.Searching)
                    StopSearch();

                _workerState = State.Exiting;
                Monitor.PulseAll(_workThread);
            }
        }

        private void Loop()
        {
            while (true)
            {
                lock (_workThread)
                {
                    while (true)
                    {
                        if (_workerState != State.Waiting)
                            break;
                        
                        Monitor.Wait(_workThread);
                    }

                    if (_workerState == State.Exiting)
                        return;

                    // worker state is Searching
                }

                var (bestMove, statistics) = _search.Search(_position, _timeStrategy, _printInfoDelegate);
                _printInfoDelegate(statistics);
                _printBestMoveDelegate(bestMove);

                lock (_workThread)
                {
                    _workerState = State.Waiting;
                    Monitor.PulseAll(_workThread);
                }
            }

        }
    }
}

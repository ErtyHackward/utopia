using System;
using System.Diagnostics;
using NLog;

namespace Utopia.Shared
{
    /// <summary>
    /// Allows to measure performance time of a section
    /// </summary>
    public class PerfLimit : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly string _message;
        private readonly int _limitMs;
        private readonly Stopwatch _sw;

        public PerfLimit(string message, int limitMs = 100)
        {
            _message = message;
            _limitMs = limitMs;
            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _sw.Stop();
            if (_sw.ElapsedMilliseconds > _limitMs)
            {
                logger.Warn("{0} : {1}/{2} ms", _message, _sw.ElapsedMilliseconds, _limitMs);
            }
        }
    }

    public class LazyPerfLimit : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Func<string> _message;
        private readonly int _limitMs;
        private readonly Stopwatch _sw;

        public LazyPerfLimit(Func<string> createMessage, int limitMs = 100)
        {
            _message = createMessage;
            _limitMs = limitMs;
            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _sw.Stop();
            if (_sw.ElapsedMilliseconds > _limitMs)
            {
                logger.Warn("{0} : {1}/{2} ms", _message(), _sw.ElapsedMilliseconds, _limitMs);
            }
        }
    }
}
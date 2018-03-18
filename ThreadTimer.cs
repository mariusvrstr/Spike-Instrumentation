
namespace Spike.Instrumentation
{
    using System;
    using System.Threading;
    using Logging;

    public class ThreadTimer
    {
        public Timer Timer { get; private set; }
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);
        private readonly object _threadLock = new object();
        public event Action<Timer, object> TimerEvent;
        private readonly Logging.Logging _logger = LogFactory.Create("Spike.Instrumentation.ThreadTimer");
        private readonly string _counterName;

        private string CounterInfo => string.IsNullOrEmpty(_counterName) ? string.Empty : $" Counter Name [{_counterName}]";
        private string ThreadInfo => $"Thread ID [{OrigiatingThread.ManagedThreadId}] State [{OrigiatingThread.ThreadState}]";
        public Thread OrigiatingThread { get; private set; }

        public ThreadTimer(string counterName = null)
        {
            OrigiatingThread = Thread.CurrentThread;

            this._counterName = counterName ?? string.Empty;
        }

        public void Start(TimeSpan timerInterval, bool triggerAtStart = false,
            object state = null)
        {
            _logger.Info($"Start thread safe timer {CounterInfo}");

            Stop();
            Timer = new Timer(Timer_Elapsed, state,
                System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            Timer.Change(triggerAtStart ? TimeSpan.FromTicks(0) : timerInterval, timerInterval);
        }

        public void Stop(TimeSpan? timeout = null)
        {
            timeout = timeout ?? Timeout;
            _logger.Info($"Stop thread safe timer {CounterInfo}");
            lock (_threadLock)
            {
                if (Timer == null) return;

                using (var waitHandle = new ManualResetEvent(false))
                {
                    if (Timer.Dispose(waitHandle) && !waitHandle.WaitOne(timeout.Value))
                    {
                        // Timer has not been disposed by someone else
                        _logger.Info("Timeout waiting for timer to stop");
                        throw new TimeoutException("Timeout waiting for timer to stop");
                    }
                    Timer = null;
                }
            }
        }

        public void Timer_Elapsed(object state)
        {
            if (Monitor.TryEnter(_threadLock))
            {
                var timerEvent = TimerEvent;

                try
                {
                    if (Timer == null)
                    {
                        _logger.Info($"Thread Timer is null. Stop ticking.{CounterInfo}");
                        return;
                    }

                    var isRunning = (OrigiatingThread.ThreadState == ThreadState.Background
                        || OrigiatingThread.ThreadState == ThreadState.Running
                        || OrigiatingThread.ThreadState == ThreadState.WaitSleepJoin);

                    if (!isRunning || timerEvent == null)
                    {
                        if (!isRunning)
                        {
                            _logger.Info($"Application Thread is not active. Stop ticking.{CounterInfo} {ThreadInfo}");
                        }
                        else if (timerEvent == null)
                        {
                            _logger.Info($"Timer event is null. Stop ticking.{CounterInfo}");
                        }

                        Timer.Dispose();
                        Timer = null;
                        return;
                    }

                    timerEvent(Timer, state);
                }
                finally
                {
                    Monitor.Exit(_threadLock);
                }
            }
        }
    }
}

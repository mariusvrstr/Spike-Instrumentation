
namespace Spike.Instrumentation.Monitoring.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class HeartbeatMonitor : MonitorBase
    {
        private const int HeartBeatIntervalInSeconds = 1;
        public const string HeartbeatName = "Heartbeat";

        /// <summary>
        /// The timer helper
        /// </summary>
        private ThreadTimer _timerHelper;

        /// <summary>
        /// The stop watch
        /// </summary>
        private Stopwatch _stopWatch;

        public HeartbeatMonitor(string categoryName) : base(categoryName)
        {
        }

        private void OnHeartbeat(object state)
        {
            var instance = (HeartbeatMonitor)state;
            instance._stopWatch.Stop();

            var numberOfTicks = instance._stopWatch.ElapsedTicks;
            instance.Counters.Single(c => c.CounterName == instance.HeartBeatData.CounterName).IncrementBy(numberOfTicks);

            instance._stopWatch = Stopwatch.StartNew();
        }


        private CounterCreationData _heartBeatData;

        private CounterCreationData HeartBeatData
        {
            get
            {
                if (_heartBeatData != null)
                {
                    return _heartBeatData;
                }

                return _heartBeatData = new CounterCreationData
                {
                    CounterName = HeartbeatName,
                    CounterType = PerformanceCounterType.CounterTimer
                };
            }
        }

        public void StartTimer(int interval)
        {
            _stopWatch = Stopwatch.StartNew();
            _timerHelper.Start(TimeSpan.FromSeconds(interval), true);
        }

        public override void IntializeMonitor()
        {
            _timerHelper = new ThreadTimer($"{CategoryName}:{MonitorName ?? string.Empty}:{HeartBeatData.CounterName}");
            _timerHelper.TimerEvent += (timer, state) => OnHeartbeat(this);

            StartTimer(HeartBeatIntervalInSeconds);
        }

        protected override List<CounterCreationData> CounterDataToRegister()
        {
            return new List<CounterCreationData>
            {
                HeartBeatData
            };
        }
    }
}

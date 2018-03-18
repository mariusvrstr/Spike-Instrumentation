
namespace Spike.Instrumentation.Monitoring.Monitors
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class AverageMonitor : MonitorBase
    {
        private const int AverageIntervalInMinutes = 1;

        public AverageMonitor(string categoryName, string monitorName, IntervalType averagePeriod) : base(categoryName, monitorName)
        {
            _averagePeriod = averagePeriod;
        }

        private readonly IntervalType _averagePeriod;
        private CounterCreationData _averageCounterData;
        private int _counter;
        private LoopQueue _queue;
        private ThreadTimer _threadTimer;

        private CounterCreationData AverageCounterData
        {
            get
            {
                if (_averageCounterData != null)
                {
                    return _averageCounterData;
                }

                var counterName = $"{MonitorName}";

                return _averageCounterData = new CounterCreationData
                {
                    CounterName = counterName,
                    CounterType = PerformanceCounterType.NumberOfItems64
                };
            }
        }

        private void OnAverageTic(object state)
        {
            var instance = (AverageMonitor)state;

            var average = instance._queue.IntervalTic(ref _counter);

            instance.Counters.Single(c => c.CounterName == AverageCounterData.CounterName).RawValue = average;
        }

        public void StartTimer(int intervalInMinutes)
        {
            _threadTimer.Start(TimeSpan.FromMinutes(intervalInMinutes), true);
        }

        public override void IntializeMonitor()
        {
            _queue = new LoopQueue(_averagePeriod, AverageIntervalInMinutes);

            _threadTimer = new ThreadTimer();
            _threadTimer.TimerEvent += (timer, state) => OnAverageTic(this);

            StartTimer(AverageIntervalInMinutes);
        }

        protected override List<CounterCreationData> CounterDataToRegister()
        {
            return new List<CounterCreationData>
            {
                AverageCounterData
            };
        }
    }
}

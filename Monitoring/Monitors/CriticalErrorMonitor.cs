
namespace Spike.Instrumentation.Monitoring.Monitors
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System;
    using System.Linq;
    using Logging;

    public class CriticalErrorMonitor : MonitorBase
    {
        public CriticalErrorMonitor(string categoryName, string monitorName) :
            base(categoryName, monitorName.ToLower().Contains("critical") ? monitorName : $"Critical{categoryName}")
        {
        }

        private CounterCreationData _criticalCounterData;

        private readonly Logging _logger = LogFactory.Create("Spike.Monitoring.Monitors.CriticalErrorMonitor");

        private CounterCreationData CriticalCounterData
        {
            get
            {
                if (_criticalCounterData != null)
                {
                    return _criticalCounterData;
                }

                return _criticalCounterData = new CounterCreationData
                {
                    CounterName = MonitorName,
                    CounterType = PerformanceCounterType.NumberOfItems64
                };
            }
        }

        public void Set(long value)
        {
            try
            {
                Counters.Single(c => c.CounterName == CriticalCounterData.CounterName).RawValue = value;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not set counter [{CriticalCounterData.CounterName}]. Error [{ex.Message}]");
            }
        }

        public void Increment(long value = 1)
        {
            try
            {
                Counters.Single(c => c.CounterName == CriticalCounterData.CounterName).IncrementBy(value);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not increment counter [{CriticalCounterData.CounterName}]. Error [{ex.Message}]");
            }
        }

        public void Decrement(long value = 1)
        {
            try
            {
                var decriment = -1 * value;
                Counters.Single(c => c.CounterName == CriticalCounterData.CounterName).IncrementBy(decriment);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not decrement counter [{CriticalCounterData.CounterName}]. Error [{ex.Message}]");
            }
        }

        public void Reset()
        {
            try
            {
                var counter = Counters.Single(c => c.CounterName == CriticalCounterData.CounterName);
                counter.RawValue = 0;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not decrement counter [{CriticalCounterData.CounterName}]. Error [{ex.Message}]");
            }
        }

        protected override List<CounterCreationData> CounterDataToRegister()
        {
            return new List<CounterCreationData>
            {
                CriticalCounterData
            };
        }
    }
}

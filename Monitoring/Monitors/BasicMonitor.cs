


namespace Spike.Instrumentation.Monitoring.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Logging;

    public class BasicMonitor : MonitorBase
    {
        public BasicMonitor(string categoryName, string monitorName)
            : base(categoryName, monitorName)
        {
        }

        private CounterCreationData _basicCounterData;

        private readonly Logging _logger = LogFactory.Create("Spike.Monitoring.Monitors.BasicMonitor");
        private CounterCreationData BasicCounterData
        {
            get
            {
                if (_basicCounterData != null)
                {
                    return _basicCounterData;
                }

                return _basicCounterData = new CounterCreationData
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
                Counters.Single(c => c.CounterName == BasicCounterData.CounterName).RawValue = value;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not set counter [{BasicCounterData.CounterName}]. Error [{ex.Message}]");
            }
        }

        public long Value
        {
            get
            {
                try
                {
                    return Counters.Single(c => c.CounterName == BasicCounterData.CounterName).RawValue;
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Could not read counter [{BasicCounterData.CounterName}]. Error [{ex.Message}]");
                }

                return 0;
            }
        }

        public void Increment(long value = 1)
        {
            try
            {
                Counters.Single(c => c.CounterName == BasicCounterData.CounterName).IncrementBy(value);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not Increment counter [{BasicCounterData.CounterName}]. Error [{ex.Message}]");
            }
        }

        public void Decrement(long value = 1)
        {
            try
            {
                var decriment = -1 * value;
                Counters.Single(c => c.CounterName == BasicCounterData.CounterName).IncrementBy(decriment);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not decrement counter [{BasicCounterData.CounterName}]. Error [{ex.Message}]");
            }
        }

        public void Reset()
        {
            try
            {
                var counter = Counters.Single(c => c.CounterName == BasicCounterData.CounterName);
                counter.RawValue = 0;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not reset counter [{BasicCounterData.CounterName}]. Error [{ex.Message}]");
            }
        }

        protected override List<CounterCreationData> CounterDataToRegister()
        {
            return new List<CounterCreationData>
            {
                BasicCounterData
            };
        }
    }
}

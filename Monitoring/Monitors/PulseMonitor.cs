
namespace Spike.Instrumentation.Monitoring.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Logging;

    public class PulseMonitor : MonitorBase
    {
        public const int PulseValue = 10;
        public const int ChangeLockInSeconds = 2;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly Logging _logger = LogFactory.Create("Spike.Monitoring.Monitors.PulseMonitor");

        /// <summary>
        /// Initializes a new instance of the <see cref="PulseMonitor"/> class.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="monitorName">Name of the monitor.</param>
        public PulseMonitor(string categoryName, string monitorName = null) : base(categoryName, monitorName)
        {
            _lastActivity = DateTime.Now.AddSeconds(-1 * (ChangeLockInSeconds + 1));
        }

        private CounterCreationData _pulseCounterData;
        private DateTime _lastActivity;

        private CounterCreationData PulseCounterData
        {
            get
            {
                if (_pulseCounterData != null)
                {
                    return _pulseCounterData;
                }

                return _pulseCounterData = new CounterCreationData
                {
                    CounterName = MonitorName,
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
            }
        }

        private bool IsLockedForChange
        {
            get
            {
                var lockedFromTimstamp = DateTime.Now.AddSeconds(-1 * ChangeLockInSeconds);

                return _lastActivity > lockedFromTimstamp;
            }
        }

        public void Pulse()
        {
            try
            {
                if (IsLockedForChange)
                {
                    return;
                }

                var counter = Counters.Single(c => c.CounterName == PulseCounterData.CounterName);
                var toggel = PulseToggel(counter.RawValue);

                counter.RawValue = toggel;
                _lastActivity = DateTime.Now;

            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not toggel counter [{PulseCounterData.CounterName}]. Error [{ex.Message}]");
            }
        }

        public static long PulseToggel(long currentValue)
        {
            if (currentValue == PulseValue)
            {
                return 0;
            }

            return PulseValue;
        }


        protected override List<CounterCreationData> CounterDataToRegister()
        {
            return new List<CounterCreationData>
            {
                PulseCounterData
            };
        }
    }
}

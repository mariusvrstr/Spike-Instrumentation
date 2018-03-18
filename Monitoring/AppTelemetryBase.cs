
namespace Spike.Instrumentation.Monitoring
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Principal;
    using Monitors;
    using System.Linq;
    using Logging;

    public abstract partial class AppTelemetryBase
    {
        public bool CreateCountersAllowed { get; set; }
        private readonly List<MonitorBase> _registeredMonitors = new List<MonitorBase>();
        private bool _isInitialized;
        public string CategoryDescription { get; }
        public string CategoryName { get;}
        private readonly Logging _logger = LogFactory.Create("Spike.Monitoring.Monitors");

        protected abstract void RegisterMonitors();

        protected AppTelemetryBase(string categoryName, string description, bool registerHeartbeat = true)
        {
            CategoryName = categoryName;
            CategoryDescription = description;

            if (registerHeartbeat)
            {
                AddHeartBeatMonitor();
            }
        }
        
        private T GetMonitorByType<T>(string monitorName)
           where T : MonitorBase
        {
            var monitor = GetMonitor(monitorName);

            if (monitor == null)
            {
                _logger.Warn($"Monitor [{monitorName}] is not a registered monitor in [{CategoryName}]");
                return null;
            }

            if (monitor.GetType() == typeof(T))
            {
                return monitor as T;
            }

            _logger.Warn($"Monitor [{monitorName}] is not a [{typeof(T)}] monitor type");

            return null;
        }
        
        private MonitorBase GetMonitor(string monitorName)
        {
            return _registeredMonitors?.FirstOrDefault(mon => mon.MonitorName == monitorName);
        }

        private bool IsAdministrator 
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        protected TwoStateMonitor AddTwoStateMonitor(string monitorName, IntervalType averageInterval)
        {
            var monitor = new TwoStateMonitor(CategoryName, monitorName, averageInterval);
            _registeredMonitors.Add(monitor);

            return monitor;
        }
        
        protected HeartbeatMonitor AddHeartBeatMonitor()
        {
            var monitor = new HeartbeatMonitor(CategoryName);
            _registeredMonitors.Add(monitor);

            return monitor;
        }

        protected BasicMonitor AddBasicMonitor(string monitorName)
        {
            var monitor = new BasicMonitor(CategoryName, monitorName);
            _registeredMonitors.Add(monitor);

            return monitor;
        }

        protected PulseMonitor AddPulseMonitor(string monitorName)
        {
            var monitor = new PulseMonitor(CategoryName, monitorName);
            _registeredMonitors.Add(monitor);

            return monitor;
        }
        
        protected CriticalErrorMonitor AddCriticalErrorMonitor(string monitorName)
        {
            var monitor = new CriticalErrorMonitor(CategoryName, monitorName);
            _registeredMonitors.Add(monitor);

            return monitor;
        }
        protected void RegisterCounters()
        {
            // Logger.Info($"Checking if counters category [{CategoryName}] should be created = [{CreateCountersAllowed && IsAdministrator}]. Required >> CreateCounters [{CreateCountersAllowed}] IsAdministrator [{IsAdministrator}] Exists [{PerformanceCounterCategory.Exists(CategoryName)}]");

            if (CreateCountersAllowed && IsAdministrator)
            {
                // Logger.Info($"Counters >> Beginning to removing existing category [{CategoryName}]");
                if (PerformanceCounterCategory.Exists(CategoryName))
                {
                   PerformanceCounterCategory.Delete(CategoryName);
                }
            }

            if (!CreateCountersAllowed || PerformanceCounterCategory.Exists(CategoryName)) return;

            // Logger.Info($"Counters >> Beginning creation of a new category [{CategoryName}]");

            var counterData = new List<CounterCreationData>();
            foreach (var monitor in _registeredMonitors)
            {
                counterData.AddRange(monitor.CounterData);
            }

            var dataCollection = new CounterCreationDataCollection(counterData.ToArray());

            PerformanceCounterCategory.Create(CategoryName, CategoryDescription, PerformanceCounterCategoryType.SingleInstance, dataCollection);

            // Logger.Info($"Counters >> Completed creating category [{CategoryName}]");
	}

        public void StartMonitoring()
        {
            if (_isInitialized) return;

            RegisterMonitors();
            RegisterCounters();

            foreach (var monitor in _registeredMonitors)
            {
                monitor.IntializeMonitor();
            }

            _isInitialized = true;
        }
    }
}

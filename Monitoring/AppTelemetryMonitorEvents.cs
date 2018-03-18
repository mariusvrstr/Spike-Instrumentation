
namespace Spike.Instrumentation.Monitoring
{
    using Monitors;

    public abstract partial class AppTelemetryBase
    {
        public void BasicMonitorSet(string monitorName, int value)
        {
            var monitor = GetMonitorByType<BasicMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Set(value);
            }
        }

        public void BasicMonitorInc(string monitorName, int value = 1)
        {
            var monitor = GetMonitorByType<BasicMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Increment(value);
            }
        }

        public void BasicMonitorDec(string monitorName, int value = 1)
        {
            var monitor = GetMonitorByType<BasicMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Decrement(value);
            }
        }

        public void BasicMonitorReset(string monitorName)
        {
            var monitor = GetMonitorByType<BasicMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Reset();
            }
        }

        public long BasicMonitorValue(string monitorName)
        {
            var monitor = GetMonitorByType<BasicMonitor>(monitorName);
            return monitor.Value;
        }

        public void PulseMonitorToggle(string monitorName)
        {
            var monitor = GetMonitorByType<PulseMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Pulse();
            }
        }

        public void CriticalMonitorSet(string monitorName, int value)
        {
            var monitor = GetMonitorByType<CriticalErrorMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Set(value);
            }
        }

        public void CriticalMonitorInc(string monitorName, int value = 1)
        {
            var monitor = GetMonitorByType<CriticalErrorMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Increment(value);
            }
        }

        public void CriticalMonitorDec(string monitorName, int value = 1)
        {
            var monitor = GetMonitorByType<CriticalErrorMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Decrement(value);
            }
        }

        public void CriticalMonitorReset(string monitorName)
        {
            var monitor = GetMonitorByType<CriticalErrorMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Reset();
            }
        }

        public void TwoStateMonitorAttempt(string monitorName)
        {
            var monitor = GetMonitorByType<TwoStateMonitor>(monitorName);
            if (monitor == null) return;
            lock (monitor)
            {
                monitor.Attempt();
            }
        }

        public void TwoStateMonitorSuccess(string monitorName)
        {
            var monitor = GetMonitorByType<TwoStateMonitor>(monitorName);
            if (monitor == null) return;

            lock (monitor)
            {
                monitor.Success();
            }
        }

        public void TwoStateMonitorFailure(string monitorName)
        {
            var monitor = GetMonitorByType<TwoStateMonitor>(monitorName);
            if (monitor == null) return;

            lock (monitor)
            {
                monitor.Failure();
            }
        }
    }
}


namespace Spike.Instrumentation.Monitoring.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Logging;

    public class TwoStateMonitor : MonitorBase
    {
        private const int AverageIntervalInMinutes = 1;

        private const string SuccessCounterName = "Success";
        private const string FailureCounterName = "Failure";
        private const string AttemptCounterName = "Attempt";
        private const string AverageSuccessCounterName = "SuccessAverage";
        private const string AverageFailureCounterName = "FailureAverage";
        private const string AverageAttemptCounterName = "AttemptsAverage";
        private const string LastSuccessCounterName = "LastSuccessPulse";
        private const string ConsecutiveFailuresCounterName = "ConsecutiveFailures";
        private readonly Logging _logger = LogFactory.Create("Spike.Monitoring.Monitors.TwoStateMonitor");

        public TwoStateMonitor(string categoryName, string monitorName, IntervalType averagePeriod)
            : base(categoryName, monitorName)
        {
            _averagePeriod = averagePeriod;
        }

        private CounterCreationData _successCounterData;
        private CounterCreationData _failureCounterData;

        /// <summary>
        /// The attempt counter data.
        /// </summary>
        private CounterCreationData _attemptCounterData;

        /// <summary>
        /// The average successes counter data
        /// </summary>
        private CounterCreationData _averageSuccessesCounterData;
        private CounterCreationData _averageFailureCounterData;
        private CounterCreationData _averageAttemptsCounterData;
        private CounterCreationData _consecutivefailureCounterData;
        private CounterCreationData _lastSuccessCounterData;

        /// <summary>
        /// The number of successes
        /// </summary>
        private int _numberOfSuccesses;
        private int _numberOfFailures;
        private int _numberOfAttempts;

        private readonly IntervalType _averagePeriod;
        private LoopQueue _successQueue;
        private LoopQueue _failureQueue;
        private LoopQueue _attemptsQueue;

        private ThreadTimer _timerHelper;
        private DateTime _lastSuccessActivity;

        private bool IsLockedForChange
        {
            get
            {
                var lockedFromTimstamp = DateTime.Now.AddSeconds(-1 * PulseMonitor.ChangeLockInSeconds);

                return _lastSuccessActivity > lockedFromTimstamp;
            }
        }

        private CounterCreationData SuccessCounterData
        {
            get
            {
                if (_successCounterData != null)
                {
                    return _successCounterData;
                }

                var counterName = $"{MonitorName}.{SuccessCounterName}";

                return _successCounterData = new CounterCreationData
                {
                    CounterName = counterName,
                    CounterType = PerformanceCounterType.NumberOfItems64
                };
            }
        }

        private CounterCreationData FailureCounterData
        {
            get
            {
                if (_failureCounterData != null)
                {
                    return _failureCounterData;
                }

                var counterName = $"{MonitorName}.{FailureCounterName}";

                return _failureCounterData = new CounterCreationData
                {
                    CounterName = counterName,
                    CounterType = PerformanceCounterType.NumberOfItems64
                };
            }
        }

        private CounterCreationData AttemptCounterData
        {
            get
            {
                if (_attemptCounterData != null)
                {
                    return _attemptCounterData;
                }

                var counterName = $"{MonitorName}.{AttemptCounterName}";

                return _attemptCounterData = new CounterCreationData
                {
                    CounterName = counterName,
                    CounterType = PerformanceCounterType.NumberOfItems64
                };
            }
        }
        private CounterCreationData ConsecutiveFailureCounterData
        {
            get
            {
                if (_consecutivefailureCounterData != null)
                {
                    return _consecutivefailureCounterData;
                }

                var counterName = $"{MonitorName}.{ConsecutiveFailuresCounterName}";

                return _consecutivefailureCounterData = new CounterCreationData
                {
                    CounterName = counterName,
                    CounterType = PerformanceCounterType.NumberOfItems64
                };
            }
        }

        private CounterCreationData AverageSuccessesCounterData
        {
            get
            {
                if (_averageSuccessesCounterData != null)
                {
                    return _averageSuccessesCounterData;
                }

                var counterName = $"{MonitorName}.{AverageSuccessCounterName}";

                return _averageSuccessesCounterData = new CounterCreationData
                {
                    CounterName = counterName,
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
            }
        }

        private CounterCreationData AverageFailuresCounterData
        {
            get
            {
                if (_averageFailureCounterData != null)
                {
                    return _averageFailureCounterData;
                }

                var counterName = $"{MonitorName}.{AverageFailureCounterName}";

                return _averageFailureCounterData = new CounterCreationData
                {
                    CounterName = counterName,
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
            }
        }

        private CounterCreationData AverageAttemptsCounterData
        {
            get
            {
                if (_averageAttemptsCounterData != null)
                {
                    return _averageAttemptsCounterData;
                }

                var counterName = $"{MonitorName}.{AverageAttemptCounterName}";

                return _averageAttemptsCounterData = new CounterCreationData
                {
                    CounterName = counterName,
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
            }
        }

        private CounterCreationData LastSuccessCounterData
        {
            get
            {
                if (_lastSuccessCounterData != null)
                {
                    return _lastSuccessCounterData;
                }

                var counterName = $"{MonitorName}.{LastSuccessCounterName}";

                return _lastSuccessCounterData = new CounterCreationData
                {
                    CounterName = counterName,
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
            }
        }

        private void ResetCounter(string counterName)
        {
            try
            {
                Counters.Single(c => c.CounterName == counterName).RawValue = 0;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not reset counter [{counterName}]. Error [{ex.Message}]");
            }
        }

        private void SetCounter(string counterName, long value)
        {
            try
            {
                Counters.Single(c => c.CounterName == counterName).RawValue = value;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not set counter [{counterName}]. Error [{ex.Message}]");
            }
        }

        private void IncrementCounter(string counterName, long value)
        {
            try
            {
                Counters.Single(c => c.CounterName == counterName).IncrementBy(value);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not increment counter [{counterName}]. Error [{ex.Message}]");
            }
        }

        private void SetPulseCounter(string counterName)
        {
            try
            {
                var counter = Counters.Single(c => c.CounterName == counterName);
                var toggel = PulseMonitor.PulseToggel(counter.RawValue);

                counter.RawValue = toggel;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not toggel pulse counter [{counterName}]. Error [{ex.Message}]");
            }
        }

        private void OnAverageTic(object state)
        {
            var instance = (TwoStateMonitor)state;

            var averageSuccess = instance._successQueue.IntervalTic(ref _numberOfSuccesses);
            var averageFailure = instance._failureQueue.IntervalTic(ref _numberOfFailures);
            var averageAttempts = instance._attemptsQueue.IntervalTic(ref _numberOfAttempts);

            instance.SetCounter(AverageSuccessesCounterData.CounterName, averageSuccess);
            instance.SetCounter(AverageFailuresCounterData.CounterName, averageFailure);
            instance.SetCounter(AverageAttemptsCounterData.CounterName, averageAttempts);
        }

        public void StartTimer(int intervalInMinutes)
        {
            _timerHelper.Start(TimeSpan.FromMinutes(intervalInMinutes), true);
        }

        public override void IntializeMonitor()
        {
            _successQueue = new LoopQueue(_averagePeriod, AverageIntervalInMinutes);
            _failureQueue = new LoopQueue(_averagePeriod, AverageIntervalInMinutes);
            _attemptsQueue = new LoopQueue(_averagePeriod, AverageIntervalInMinutes);

            _timerHelper = new ThreadTimer($"{CategoryName}:{MonitorName ?? string.Empty}: OnAverage");
            _timerHelper.TimerEvent += (timer, state) => OnAverageTic(this);

            StartTimer(AverageIntervalInMinutes);
        }

        public void Success(int incrementBy = 1)
        {
            Console.WriteLine("Success {0}", CategoryName);

            _numberOfSuccesses += incrementBy;
            IncrementCounter(SuccessCounterData.CounterName, incrementBy);
            ResetCounter(ConsecutiveFailureCounterData.CounterName);

            if (IsLockedForChange) return;

            SetPulseCounter(LastSuccessCounterData.CounterName);
            _lastSuccessActivity = DateTime.Now;
        }

        public void Failure(int incrementBy = 1)
        {
            Console.WriteLine("Failure {0}", CategoryName);

            _numberOfFailures += incrementBy;
            IncrementCounter(FailureCounterData.CounterName, incrementBy);
            IncrementCounter(ConsecutiveFailureCounterData.CounterName, incrementBy);
        }

        public void Attempt(int incrementBy = 1)
        {
            Console.WriteLine("Attempt {0}", CategoryName);
            _numberOfAttempts += incrementBy;
            IncrementCounter(AttemptCounterData.CounterName, incrementBy);
        }

        protected override List<CounterCreationData> CounterDataToRegister()
        {
            return new List<CounterCreationData>
            {
                SuccessCounterData,
                AverageSuccessesCounterData,
                LastSuccessCounterData,

                FailureCounterData,
                AverageFailuresCounterData,
                ConsecutiveFailureCounterData,

                AttemptCounterData,
                AverageAttemptsCounterData
            };
        }
    }
}

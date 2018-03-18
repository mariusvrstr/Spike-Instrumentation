
namespace Spike.Instrumentation.Logging
{
    using System;
    using NLog;

    [Serializable]
    public class Logging : ILogging
    {
        internal const string DefaultModule = "";
        public string Module { get; internal set; }
        public bool DisableConsoleLogging { get; internal set; }

        public Logging() : this(null) {}

        public Logging(string module = DefaultModule, bool disableConsoleLogging = false)
        {
            Module = string.IsNullOrWhiteSpace(module) ? module : $"{module}::";

            DisableConsoleLogging = disableConsoleLogging;
        }

        private static Logger _logger;

        protected static Logger LocalLogger
        {
            get
            {
                if (_logger != null)
                {
                    return _logger;
                }

                return _logger =  LogManager.GetCurrentClassLogger();
            }
        }

        public virtual void Verbose(string msg)
        {
            if (!DisableConsoleLogging && Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
            }
            WriteLog(LogLevel.Verbose, msg);
        }

        public virtual void Verbose(string msg, params object[] messageArgs)
        {
            Verbose(string.Format(msg, messageArgs));
        }

        public virtual void Debug(string msg)
        {
            if (!DisableConsoleLogging && Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
            }
            WriteLog(LogLevel.Debug, msg);
        }

        public virtual void Debug(string msg, params object[] messageArgs)
        {
            Debug(string.Format(msg, messageArgs));
        }

        public virtual void Info(string msg)
        {
            if (!DisableConsoleLogging && Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
            }
            WriteLog(LogLevel.Info, msg);
        }

        public virtual void Info(string msg, params object[] messageArgs)
        {
            Info(string.Format(msg, messageArgs));
        }

        public virtual void Warn(string msg)
        {
            if (!DisableConsoleLogging && Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
            }
            WriteLog(LogLevel.Warn, msg);
        }

        public virtual void Warn(string msg, params object[] messageArgs)
        {
            Warn(string.Format(msg, messageArgs));
        }

        public virtual void Error(string msg)
        {
            if (!DisableConsoleLogging && Environment.UserInteractive)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
                Console.ResetColor();
            }
            WriteLog(LogLevel.Error, msg);
        }

        public virtual void Error(string msg, params object[] messageArgs)
        {
            Error(string.Format(msg, messageArgs));
        }

        public virtual Guid ErrorWithRef(string msg)
        {
            var errorToken = Guid.NewGuid();
            WriteLog(
                LogLevel.Error,
                $"{msg} - Reference [{errorToken}]");

            return errorToken;
        }

        public virtual Guid ErrorWithRef(string msg, params object[] messageArgs)
        {
            return ErrorWithRef(string.Format(msg, messageArgs));
        }

        public virtual void Critical(string msg)
        {
            WriteLog(LogLevel.Critical, msg);
        }

        public virtual void Critical(string msg, params object[] messageArgs)
        {
            Critical(string.Format(msg, messageArgs));
        }

        public void Dispose() {}

        protected void WriteLog(LogLevel logLevel, string msg)
        {
            var theEvent = new LogEventInfo(Map(logLevel), "", msg);
            theEvent.Properties["Module"] = Module;
            LocalLogger.Log(theEvent);
        }

        public bool IsVerbose => LocalLogger.IsTraceEnabled;

        private static NLog.LogLevel Map(LogLevel original)
        {
            switch(original)
            {
                case LogLevel.Verbose :
                    return NLog.LogLevel.Trace;
                case LogLevel.Info:
                    return NLog.LogLevel.Info;
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Warn:
                    return NLog.LogLevel.Warn;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                case LogLevel.Critical:
                    return NLog.LogLevel.Fatal;
                default :
                    throw new InvalidCastException($"Unknown Log Level [{original}]");
            }
        }
    }
}

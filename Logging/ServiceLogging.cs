
namespace Spike.Instrumentation.Logging
{
    using System;

    public class ServiceLogging : Logging
    {
        public ServiceLogging() : this(null) {}

        public ServiceLogging(string module = DefaultModule)
        {
            Module = string.IsNullOrWhiteSpace(module) ? module : $"{module}::";
        }

        public override void Verbose(string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
            }
            WriteLog(LogLevel.Verbose, msg);
        }

        public override void Debug(string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
            }
            WriteLog(LogLevel.Debug, msg);
        }

        public override void Info(string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
            }
            WriteLog(LogLevel.Info, msg);
        }

        public override void Warn(string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
            }
            WriteLog(LogLevel.Warn, msg);
        }

        public override void Error(string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
                Console.ResetColor();
            }
            WriteLog(LogLevel.Error, msg);
        }

        public override Guid ErrorWithRef(string msg)
        {
            var errorToken = Guid.NewGuid();
            var message = $"{msg} - Reference [{errorToken}]";

            if (Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + message);
            }
            WriteLog(LogLevel.Error, message);

            return errorToken;
        }

        public override void Critical(string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " : " + msg);
                Console.ReadKey();
            }
            WriteLog(LogLevel.Critical, msg);
        }

        public override void Verbose(string msg, params object[] messageArgs)
        {
            Verbose(string.Format(msg, messageArgs));
        }

        public override void Debug(string msg, params object[] messageArgs)
        {
            Debug(string.Format(msg, messageArgs));
        }

        public override void Info(string msg, params object[] messageArgs)
        {
            Info(string.Format(msg, messageArgs));
        }

        public override void Warn(string msg, params object[] messageArgs)
        {
            Warn(string.Format(msg, messageArgs));
        }

        public override void Error(string msg, params object[] messageArgs)
        {
            Error(string.Format(msg, messageArgs));
        }

        public override Guid ErrorWithRef(string msg, params object[] messageArgs)
        {
            return ErrorWithRef(string.Format(msg, messageArgs));
        }

        public override void Critical(string msg, params object[] messageArgs)
        {
            Critical(string.Format(msg, messageArgs));
        }
    }
}

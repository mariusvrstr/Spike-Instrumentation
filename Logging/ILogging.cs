
namespace Spike.Instrumentation.Logging
{
    using System;

    public interface ILogging : IDisposable
    {
        bool IsVerbose { get; }

        void Verbose(string msg);
        void Verbose(string msg, params object[] messageArgs);

        void Debug(string msg);
        void Debug(string msg, params object[] messageArgs);

        void Info(string msg);
        void Info(string msg, params object[] messageArgs);

        void Warn(string msg);
        void Warn(string msg, params object[] messageArgs);

        void Error(string msg);
        void Error(string msg, params object[] messageArgs);

        void Critical(string msg);
        void Critical(string msg, params object[] messageArgs);
    }
}

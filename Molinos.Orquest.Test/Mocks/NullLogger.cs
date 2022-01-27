using System;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Test.Mocks
{
    public class NullLogger : ILogger
    {
        public Type Type
        {
            get { return null; }
        }

        public bool IsDebugEnabled
        {
            get { return false; }
        }

        public bool IsInfoEnabled
        {
            get { return false; }
        }

        public bool IsTraceEnabled
        {
            get { return false; }
        }

        public bool IsWarnEnabled
        {
            get { return false; }
        }

        public bool IsErrorEnabled
        {
            get { return false; }
        }

        public bool IsFatalEnabled
        {
            get { return false; }
        }


        public void Debug(string message)
        {
        }

        public void Debug(string format, params object[] args)
        {
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
        }

        public void Info(string message)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Info(Exception exception, string format, params object[] args)
        {
        }

        public void Trace(string message)
        {
        }

        public void Trace(string format, params object[] args)
        {
        }

        public void Trace(Exception exception, string format, params object[] args)
        {
        }

        public void Warn(string message)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
        }

        public void Error(string message)
        {
        }

        public void Error(string format, params object[] args)
        {
        }

        public void Error(Exception exception, string format, params object[] args)
        {
        }

        public void Fatal(string message)
        {
        }

        public void Fatal(string format, params object[] args)
        {
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
        }
    }
}

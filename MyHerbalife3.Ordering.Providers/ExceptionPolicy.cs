using System;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Providers
{
    internal class ExceptionPolicy
    {
        public static void HandleException(Exception exception, string systemException)
        {
            LoggerHelper.Exception(systemException, exception);
        }
    }

    internal class LoggerTempWireup
    {
        public static void WriteError(string message, string category)
        {
            LoggerHelper.Error(category + " - " + message);
        }
        
        public static void WriteInfo(string message, string category)
        {
            LoggerHelper.Info(category + " - " + message);
        }

        public static void LogServiceExceptionWithContext(Exception e, object context)
        {
            //TODO: figure out if log with context produces deeper stack trace than e.toString()
            LoggerHelper.Error(e.ToString());
        }
    }
}
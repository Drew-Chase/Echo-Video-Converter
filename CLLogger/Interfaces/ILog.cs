using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLLogger.Interfaces
{
    public interface ILog
    {
        bool IsFatalEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsErrorEnabled { get; }

        /// <summary>
        /// A Debug Log with a Single Message
        /// </summary>
        /// <param name="message"></param>
        void Debug(object message);

        /// <summary>
        /// A Debug Log with Multiple Messages
        /// </summary>
        /// <param name="messages"></param>
        void Debug(params object[] messages);
        /// <summary>
        /// A Debug Log with a Single Message with an Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Debug(object message, Exception exception);
        /// <summary>
        /// A Debug Log with Multiple Messagesa Message(s) with a specific format and an IFormatProvider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void DebugFormat(IFormatProvider provider, string format, params object[] args);
        /// <summary>
        /// A Debug Log 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void DebugFormat(string format, params object[] args);
        void DebugFormat(string format, object arg0);
        void DebugFormat(string format, object arg0, object arg1, object arg2);
        void DebugFormat(string format, object arg0, object arg1);
        void Error(object message);
        void Error(params object[] messages);
        void Error(object message, Exception exception);
        void ErrorFormat(string format, object arg0, object arg1, object arg2);
        void ErrorFormat(IFormatProvider provider, string format, params object[] args);
        void ErrorFormat(string format, object arg0, object arg1);
        void ErrorFormat(string format, object arg0);
        void ErrorFormat(string format, params object[] args);
        void Fatal(object message);
        void Fatal(params object[] messages);
        void Fatal(object message, Exception exception);
        void FatalFormat(string format, object arg0, object arg1, object arg2);
        void FatalFormat(string format, object arg0);
        void FatalFormat(string format, params object[] args);
        void FatalFormat(IFormatProvider provider, string format, params object[] args);
        void FatalFormat(string format, object arg0, object arg1);
        void Info(object message, Exception exception);
        void Info(object message);
        void Info(params object[] messages);
        void InfoFormat(string format, object arg0, object arg1, object arg2);
        void InfoFormat(string format, object arg0, object arg1);
        void InfoFormat(string format, object arg0);
        void InfoFormat(string format, params object[] args);
        void InfoFormat(IFormatProvider provider, string format, params object[] args);
        void Warn(object message);
        void Warn(params object[] messages);
        void Warn(object message, Exception exception);
        void WarnFormat(string format, object arg0, object arg1);
        void WarnFormat(string format, object arg0);
        void WarnFormat(string format, params object[] args);
        void WarnFormat(IFormatProvider provider, string format, params object[] args);
        void WarnFormat(string format, object arg0, object arg1, object arg2);

    }
}

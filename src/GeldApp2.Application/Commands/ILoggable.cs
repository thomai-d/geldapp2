using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeldApp2.Application.Commands
{
    public delegate void LogEventDelegate(int eventId, string format, params object[] args);

    /// <summary>
    /// Interface for an object which emits semantic log messages.
    /// </summary>
    public interface ILoggable
    {
        void EmitLog(LogEventDelegate log, bool success);
    }
}

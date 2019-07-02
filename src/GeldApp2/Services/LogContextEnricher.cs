using GeldApp2.Application.Services;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Services
{
    public class LogContextEnricher : ILogContextEnricher
    {
        public IDisposable PushProperty(string property, object value, bool destructure)
        {
            return LogContext.PushProperty(property, value, destructure);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace GeldApp2.Application.Services
{
    /// <summary>
    /// Abstracts Serilog's PushProperty-Method, so this assembly has no reference an external logging framework.
    /// </summary>
    public interface ILogContextEnricher
    {
        IDisposable PushProperty(string property, object value, bool destructure);
    }
}

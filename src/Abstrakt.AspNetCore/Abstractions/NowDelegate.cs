using System;
using System.Collections.Generic;
using System.Text;

namespace Abstrakt.AspNetCore.Abstractions
{
    /// <summary>
    /// Delegate to abstract time for unit tests.
    /// Should return DateTimeOffset.Now in production.
    /// </summary>
    public delegate DateTimeOffset NowDelegate();
}

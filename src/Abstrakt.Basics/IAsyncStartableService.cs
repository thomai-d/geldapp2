using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Abstrakt.Basics
{
    /// <summary>
    /// Interface for a service that needs to be started.
    /// </summary>
    public interface IAsyncStartableService
    {
        Task StartAsync(CancellationToken ct);

        Task StopAsync(CancellationToken ct);
    }
}

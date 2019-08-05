using Abstrakt.Basics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Abstrakt.AspNetCore.Services
{
    /// <summary>
    /// Implementation which starts all <see cref="IAsyncStartableService"/>s.
    /// </summary>
    public class AsyncServiceStarter : IHostedService
    {
        private readonly IEnumerable<IAsyncStartableService> services;
        private readonly ILogger<AsyncServiceStarter> log;

        public AsyncServiceStarter(IEnumerable<IAsyncStartableService> services, ILogger<AsyncServiceStarter> log)
        {
            this.services = services;
            this.log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var service in this.services)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var w = Stopwatch.StartNew();
                await service.StartAsync(cancellationToken);
                w.Stop();
                this.log.LogInformation("Starting {Service} took {ElapsedMilliseconds}ms", service.GetType().Name, w.ElapsedMilliseconds);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var service in this.services)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var w = Stopwatch.StartNew();
                await service.StopAsync(cancellationToken);
                w.Stop();
                this.log.LogInformation("Stopping {Service} took {ElapsedMilliseconds}ms", service.GetType().Name, w.ElapsedMilliseconds);
            }
        }
    }
}

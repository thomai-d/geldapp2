using Abstrakt.Basics;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Abstrakt.AspNetCore.Abstractions
{
    /// <summary>
    /// Wrapper to include an initializable .NET Standard-Service into ASP.NET Core.
    /// </summary>
    public class HostedServiceWrapper<TInterface> : IHostedService
        where TInterface : IAsyncStartableService
    {
        public HostedServiceWrapper(TInterface service)
        {
            this.Service = service;
        }

        public readonly TInterface Service;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return this.Service.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return this.Service.StopAsync(cancellationToken);
        }
    }
}

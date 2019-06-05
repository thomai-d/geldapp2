using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Services
{
    /// <summary>
    /// Logs CPU / RAM load.
    /// </summary>
    public class PerformanceMonitorService : IHostedService
    {
        public readonly TimeSpan SampleInterval = TimeSpan.FromMinutes(5);

        private readonly ILogger<PerformanceMonitorService> log;

        private IDisposable timer;
        private TimeSpan lastProcessorTime = TimeSpan.Zero;

        public PerformanceMonitorService(ILogger<PerformanceMonitorService> log)
        {
            this.log = log;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.timer = Observable.Interval(SampleInterval)
                                   .Subscribe(_ => this.LogValues());

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.timer?.Dispose();
            this.timer = null;

            return Task.CompletedTask;
        }

        private void LogValues()
        {
            using (var process = Process.GetCurrentProcess())
            {
                var processorTime = process.TotalProcessorTime;

                if (this.lastProcessorTime != TimeSpan.Zero)
                {
                    var dProcessorTime = processorTime - this.lastProcessorTime;
                    var cpuPercent = dProcessorTime / SampleInterval * 100;
                    var ramMb = process.WorkingSet64 / 1000000;
                    this.log.LogInformation("Performance: {CpuPercent}% CPU, {MemoryMb}MB RAM", cpuPercent, ramMb);
                }

                this.lastProcessorTime = processorTime;
            }
        }
    }
}

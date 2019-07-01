using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GeldApp2.Extensions;
using GeldApp2.Application.Logging;

namespace GeldApp2.Services
{
    /// <summary>
    /// Logs useful statistics.
    /// </summary>
    public class StatisticsService : IHostedService
    {
        /// <summary>
        /// Interval to generate CPU and RAM statistics.
        /// </summary>
        public readonly TimeSpan PerformanceInterval = TimeSpan.FromMinutes(5);
        
        /// <summary>
        /// Interval to generate usage statistics.
        /// </summary>
        public readonly TimeSpan StatisticsInterval = TimeSpan.FromHours(24);

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        private readonly ILogger<StatisticsService> log;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IUsageStatisticsLogger usageStats;

        private TimeSpan lastProcessorTime = TimeSpan.Zero;

        public StatisticsService(
            ILogger<StatisticsService> log,
            IServiceScopeFactory scopeFactory,
            IUsageStatisticsLogger usageStats)
        {
            this.log = log;
            this.scopeFactory = scopeFactory;
            this.usageStats = usageStats;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Observable.Interval(PerformanceInterval)
                      .Subscribe(_ => this.LogCpuAndRamValues())
                      .DisposeWith(this.disposables);

            Observable.Interval(StatisticsInterval)
                      .Subscribe(_ => this.LogUsageStatistics())
                      .DisposeWith(this.disposables);

            this.LogCpuAndRamValues();
            this.LogUsageStatistics();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.disposables.Clear();

            return Task.CompletedTask;
        }

        private async void LogUsageStatistics()
        {
            using (var scope = this.scopeFactory.CreateScope())
            {
                await this.usageStats.LogUsageStatisticsAsync();
            }
        }

        private void LogCpuAndRamValues()
        {
            using (var process = Process.GetCurrentProcess())
            {
                var processorTime = process.TotalProcessorTime;

                if (this.lastProcessorTime != TimeSpan.Zero)
                {
                    var dProcessorTime = processorTime - this.lastProcessorTime;
                    var cpuPercent = dProcessorTime / PerformanceInterval * 100;
                    var ramMb = process.WorkingSet64 / 1000000;
                    this.log.LogInformation(Events.PerformanceStatistics, "Performance: {CpuPercent}% CPU, {MemoryMb}MB RAM", cpuPercent, ramMb);
                }

                this.lastProcessorTime = processorTime;
            }
        }
    }
}

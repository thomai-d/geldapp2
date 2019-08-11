using Abstrakt.Basics;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Services
{
    public interface IScheduler : IAsyncStartableService
    {
        void ScheduleEveryNight(Func<Task> task);
    }

    public class Scheduler : IScheduler
    {
        public Task StartAsync(CancellationToken ct)
        {
            JobManager.Initialize();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct)
        {
            JobManager.Stop();

            return Task.CompletedTask;
        }

        public void ScheduleEveryNight(Func<Task> task)
        {
            JobManager.AddJob(() => task().Wait(), s => s.NonReentrant().ToRunEvery(1).Days().At(03, 00));
        }
    }
}

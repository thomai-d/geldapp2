using Abstrakt.AspNetCore;
using GeldApp2.Application.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace GeldApp2.Services
{
    public class IpBlockerSettings
    {
        public int Events { get; set; }

        public TimeSpan SurveyTime { get; set; }

        public TimeSpan BanTime { get; set; }
    }

    public interface IIpBlockerService : IDisposable
    {
        void CountViolation(string ip);
        bool IsBlocked(string ip);
    }

    public class IpBlockerService : IIpBlockerService
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> blocked
            = new ConcurrentDictionary<string, DateTimeOffset>();

        private readonly EventCounter events;
        private readonly IpBlockerSettings settings;
        private readonly ILogger<IpBlockerService> log;

        private readonly IDisposable cleanupTask;

        public IpBlockerService(IOptions<IpBlockerSettings> settings, ILogger<IpBlockerService> log)
        {
            this.settings = settings.Value;
            this.events = new EventCounter(() => DateTimeOffset.Now, this.settings.SurveyTime);
            this.log = log;

            this.cleanupTask = Observable.Interval(TimeSpan.FromMinutes(5))
                .Subscribe(_ => this.events.Cleanup());
        }

        public void CountViolation(string ip)
        {
            this.events.Count(ip);

            if (this.events.GetCount(ip) > this.settings.Events)
            {
                var bannedUntil = DateTimeOffset.Now + this.settings.BanTime;
                this.blocked[ip] = bannedUntil;
                this.log.LogWarning(Events.IpBlocked, "{Ip} is blocked until {BannedUntil}", ip, bannedUntil);
            }
        }

        public bool IsBlocked(string ip)
        {
            return this.blocked.TryGetValue(ip, out var date) && DateTimeOffset.Now < date;
        }

        public void Dispose()
        {
            this.cleanupTask.Dispose();
        }
    }
}

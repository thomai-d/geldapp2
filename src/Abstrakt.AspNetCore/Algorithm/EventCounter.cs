using Abstrakt.AspNetCore.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Abstrakt.AspNetCore
{
    public class EventCounter
    {
        private readonly NowDelegate now;
        private readonly TimeSpan surveyTime;
        private ConcurrentDictionary<string, ConcurrentQueue<DateTimeOffset>>
            events = new ConcurrentDictionary<string, ConcurrentQueue<DateTimeOffset>>();

        public EventCounter(NowDelegate now, TimeSpan surveyTime)
        {
            this.now = now;
            this.surveyTime = surveyTime;
        }

        public void Clear(string key)
        {
            this.events.TryRemove(key, out var _);
        }

        public void Count(string key)
        {
            this.events.AddOrUpdate(key,
                _ => { var q = new ConcurrentQueue<DateTimeOffset>(); q.Enqueue(this.now()); return q; },
                (_, queue) => { queue.Enqueue(this.now()); return queue; });
        }

        public long GetCount(string key)
        {
            if (this.events.TryGetValue(key, out var queue))
            {
                var threshold = this.now() - this.surveyTime;
                while (queue.TryPeek(out var t) && t < threshold)
                    queue.TryDequeue(out var _);

                return queue.Count;
            }

            return 0;
        }

        public void Cleanup()
        {
            var threshold = this.now() - this.surveyTime;
            foreach (var kvp in this.events)
            {
                var queue = kvp.Value;
                while (queue.TryPeek(out var t) && t < threshold)
                    queue.TryDequeue(out var _);

                if (queue.Count == 0)
                    this.events.TryRemove(kvp.Key, out var _);
            }
        }

        public KeyValuePair<string, int>[] GetSnapshot()
        {
            return this.events
                .Select(i => new KeyValuePair<string, int>(i.Key, i.Value.Count))
                .ToArray();
        }
    }
}

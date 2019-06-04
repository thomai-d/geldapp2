using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Abstrakt.AspNetCore.UnitTests
{
    public class EventCounterTest
    {
        [Fact]
        public void SimpleCountTest()
        {
            var target = new EventCounter(() => DateTimeOffset.Now, TimeSpan.FromHours(1));

            target.Count("a");
            target.Count("a");
            target.Count("b");

            target.GetCount("a").Should().Be(2);
            target.GetCount("b").Should().Be(1);
            target.GetCount("c").Should().Be(0);
        }

        [Fact]
        public void ClearTest()
        {
            var target = new EventCounter(() => DateTimeOffset.Now, TimeSpan.FromHours(1));

            target.Count("a");
            target.Count("a");
            target.Clear("a");

            target.GetCount("a").Should().Be(0);
        }

        [Fact]
        public void CleanupTest()
        {
            var time = T(TimeSpan.Zero);
            var target = new EventCounter(() => time, TimeSpan.FromMinutes(1));

            target.Count("a");
            target.Count("a");
            target.Count("a");
            time = T(TimeSpan.FromMinutes(5));

            target.GetSnapshot()
                .Should().Contain(i => i.Key == "a");
            target.Cleanup();
            target.GetSnapshot()
                .Should().NotContain(i => i.Key == "a");
        }

        [Fact]
        public void SurveyTimeTest()
        {
            var time = T(TimeSpan.Zero);
            var target = new EventCounter(() => time, TimeSpan.FromMinutes(1));

            target.Count("a");
            target.Count("a");
            target.Count("a");
            time = T(TimeSpan.FromMinutes(1));
            target.Count("a");
            time = T(TimeSpan.FromMinutes(2));
            target.Count("a");

            target.GetCount("a").Should().Be(2);
        }

        [Fact]
        public async Task ConcurrencyTest()
        {
            for (int r = 0; r < 30; r++)
            {
                var target = new EventCounter(() => DateTimeOffset.Now, TimeSpan.FromHours(1));
                var random = new Random();

                var tasks = Enumerable.Range(1, 15)
                    .Select(_ => Task.Run(() =>
                    {
                        for (var n = 0; n < 1000; n++)
                            target.Count(random.Next(30).ToString());
                    })).ToArray();

                await Task.WhenAll(tasks);

                target.GetSnapshot()
                    .Sum(i => i.Value)
                    .Should().Be(15000);
            }
        }

        private static DateTimeOffset T(TimeSpan offset)
        {
            return new DateTimeOffset(2000, 1, 1, 10, 0, 0, TimeSpan.Zero) + offset;
        }
    }
}

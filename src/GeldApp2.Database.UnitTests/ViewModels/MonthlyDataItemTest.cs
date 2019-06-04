using FluentAssertions;
using GeldApp2.Database.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace GeldApp2.Database.ViewModels.UnitTests
{
    public class MonthlyDataItemTest
    {
        [Fact]
        public void WithoutGaps_Empty_Test()
        {
            var items = new MonthlyDataItem[0];
            items.WithoutGaps().Should().HaveCount(0);
        }

        [Fact]
        public void WithoutGaps_NoGaps_Test()
        {
            var items = new[]
            {
                new MonthlyDataItem { Month = 1, Year = 2000, Amount = 2001 },
                new MonthlyDataItem { Month = 2, Year = 2000, Amount = 2002 },
                new MonthlyDataItem { Month = 3, Year = 2000, Amount = 2003 }
            };

            var result = items.WithoutGaps().ToArray();
            result[0].Month.Should().Be(1);
            result[1].Month.Should().Be(2);
            result[2].Month.Should().Be(3);
            result[0].Year.Should().Be(2000);
            result[1].Year.Should().Be(2000);
            result[2].Year.Should().Be(2000);
            result[0].Amount.Should().Be(2001);
            result[1].Amount.Should().Be(2002);
            result[2].Amount.Should().Be(2003);
        }

        [Fact]
        public void WithoutGaps_SingleGap_Test()
        {
            var items = new[]
            {
                new MonthlyDataItem { Month = 1, Year = 2000, Amount = 2001 },
                new MonthlyDataItem { Month = 3, Year = 2000, Amount = 2003 }
            };

            var result = items.WithoutGaps().ToArray();
            result.Should().HaveCount(3);
            result[0].Month.Should().Be(1);
            result[1].Month.Should().Be(2);
            result[2].Month.Should().Be(3);
            result[0].Year.Should().Be(2000);
            result[1].Year.Should().Be(2000);
            result[2].Year.Should().Be(2000);
            result[0].Amount.Should().Be(2001);
            result[1].Amount.Should().Be(0);
            result[2].Amount.Should().Be(2003);
        }

        [Fact]
        public void WithoutGaps_MultiGap_Test()
        {
            var items = new[]
            {
                new MonthlyDataItem { Month = 11, Year = 2000, Amount = 2011 },
                new MonthlyDataItem { Month = 2, Year = 2001, Amount = 2002 },
                new MonthlyDataItem { Month = 4, Year = 2001, Amount = 2003 }
            };

            var result = items.WithoutGaps().ToArray();
            result.Should().HaveCount(6);
            result[0].Month.Should().Be(11);
            result[1].Month.Should().Be(12);
            result[2].Month.Should().Be(1);
            result[3].Month.Should().Be(2);
            result[4].Month.Should().Be(3);
            result[5].Month.Should().Be(4);
            result[0].Year.Should().Be(2000);
            result[1].Year.Should().Be(2000);
            result[2].Year.Should().Be(2001);
            result[3].Year.Should().Be(2001);
            result[4].Year.Should().Be(2001);
            result[5].Year.Should().Be(2001);
            result[0].Amount.Should().Be(2011);
            result[1].Amount.Should().Be(0);
            result[2].Amount.Should().Be(0);
            result[3].Amount.Should().Be(2002);
            result[4].Amount.Should().Be(0);
            result[5].Amount.Should().Be(2003);
        }

        [Fact]
        public void WithoutGaps_AbortIfGapTooLarge_Test()
        {
            var items = new[]
            {
                new MonthlyDataItem { Month = 11, Year = 1000, Amount = 2011 },
                new MonthlyDataItem { Month = 2, Year = 2001, Amount = 2002 },
            };

            items.WithoutGaps()
                 .Invoking(i => i.ToArray())
                 .Should().Throw<InvalidOperationException>();
        }
    }
}

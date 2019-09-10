using FluentAssertions;
using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Queries.Expense.Filter;
using GeldApp2.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Xunit;

namespace GeldApp2.Application.UnitTests.Queries.Expense.Filter
{
    public class ExpenseFilterStringTest
    {
        [Theory]
        [InlineData("", null, null, null, null)]
        [InlineData("month:10", 10, null, null, null)]
        [InlineData("month:10    ", 10, null, null, null)]
        [InlineData("month:10 and year:2000", 10, 2000, null, null)]
        [InlineData("month:10   and   year:2000", 10, 2000, null, null)]
        [InlineData("year:2000 and month:10", 10, 2000, null, null)]
        [InlineData("amount>10", null, null, null, null, null, AmountCompareType.GreaterThan, 10.0)]
        [InlineData("amount<0", null, null, null, null, null, AmountCompareType.LowerThan, 0)]
        [InlineData("amount:0,01", null, null, null, null, null, AmountCompareType.Equals, 0.01)]
        [InlineData("amount:-0,01", null, null, null, null, null, AmountCompareType.Equals, -0.01)]
        [InlineData("amount:1", null, null, null, null, null, AmountCompareType.Equals, 1)]
        [InlineData("year:2000 and month:10 and category:'test'", 10, 2000, "test", null)]
        [InlineData("year:2000 and month:10 and category:test", 10, 2000, "test", null)]
        [InlineData("year:2000 and month:10 and category:'test with space'", 10, 2000, "test with space", null)]
        [InlineData("year:2000 and month:10 and category:'test with space' and subcategory:'ok'", 10, 2000, "test with space", "ok")]
        [InlineData("year:2000 and month:10 and category:'test with space' and subcategory:'ok yes'", 10, 2000, "test with space", "ok yes")]
        [InlineData("year:2000 and month:10 and category:'test with space' and subcategory:ok", 10, 2000, "test with space", "ok")]
        [InlineData("year:2000 and month:10 and category:'test with space' and subcategory:ok and type:expense", 10, 2000, "test with space", "ok", ExpenseType.Expense)]
        [InlineData("year:2000 and month:10 and category:'test with space' and subcategory:ok and type:revenue", 10, 2000, "test with space", "ok", ExpenseType.Revenue)]
        [InlineData("year:2000 and month:10 and subcategory:ok", 10, 2000, null, "ok")]
        public void ValidFiltersTest(string filterString, int? expMonth, int? expYear, string expCategory, string expSubcat, ExpenseType? expectedType = null, AmountCompareType? amountComp = null, decimal amount = 0)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("DE");
            var result = ExpenseFilterString.Parse(filterString);
            result.Month.Should().Be(expMonth);
            result.Year.Should().Be(expYear);
            result.Category.Should().Be(expCategory);
            result.Subcategory.Should().Be(expSubcat);
            result.Type.Should().Be(expectedType);

            if (amountComp.HasValue)
            {
                result.AmountCompareType.Should().Be(amountComp.Value);
                result.Amount.Should().Be(amount);
            }
        }

        [Theory]
        [InlineData("*")]
        [InlineData("_")]
        [InlineData("yourmom")]
        [InlineData("month=10")]
        [InlineData("month:1000000000000000")]
        [InlineData("month:10 ok")]
        [InlineData("month:ok")]
        [InlineData("month:10 and month:11")]
        [InlineData("year:10 and year:11")]
        [InlineData("year:10 and year:11 and amount")]
        [InlineData("year:10 and year:11 and amount=")]
        [InlineData("year:10 and year:11 and amount=ok")]
        [InlineData("year:10 and year:11 and category:'")]
        [InlineData("year:10 and year:11 and category:")]
        [InlineData("year:10 and year:11 and category: and ' subcategory:")]
        [InlineData("type:")]
        [InlineData("type:exp")]
        public void InvalidFiltersTest(string filterString)
        {
            this.Invoking(i => ExpenseFilterString.Parse(filterString))
                .Should().Throw<FilterParseException>();
        }
    }
}

using FluentAssertions;
using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Queries.Expense.Filter;
using System;
using System.Collections.Generic;
using System.Text;
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
        [InlineData("year:2000 and month:10 and category:'test'", 10, 2000, "test", null)]
        [InlineData("year:2000 and month:10 and category:test", 10, 2000, "test", null)]
        [InlineData("year:2000 and month:10 and category:'test with space'", 10, 2000, "test with space", null)]
        [InlineData("year:2000 and month:10 and category:'test with space' and subcategory:'ok'", 10, 2000, "test with space", "ok")]
        [InlineData("year:2000 and month:10 and category:'test with space' and subcategory:'ok yes'", 10, 2000, "test with space", "ok yes")]
        [InlineData("year:2000 and month:10 and category:'test with space' and subcategory:ok", 10, 2000, "test with space", "ok")]
        [InlineData("year:2000 and month:10 and subcategory:ok", 10, 2000, null, "ok")]
        public void ValidFiltersTest(string filterString, int? expMonth, int? expYear, string expCategory, string expSubcat)
        {
            var result = ExpenseFilterString.Parse(filterString);
            result.Month.Should().Be(expMonth);
            result.Year.Should().Be(expYear);
            result.Category.Should().Be(expCategory);
            result.Subcategory.Should().Be(expSubcat);
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
        [InlineData("year:10 and year:11 and category:'")]
        [InlineData("year:10 and year:11 and category:")]
        [InlineData("year:10 and year:11 and category: and ' subcategory:")]
        public void InvalidFiltersTest(string filterString)
        {
            this.Invoking(i => ExpenseFilterString.Parse(filterString))
                .Should().Throw<FilterParseException>();
        }
    }
}

using Abstrakt.UnitTesting;
using FluentAssertions;
using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GeldApp2.UnitTests.Services
{
    public class DkbCsvParserTest
    {
        [Fact]
        public void RejectInvalidFilesTest()
        {
            using (var test = new UnitTest<DkbCsvParser>())
            {
                test.Target.Invoking(i => i.Parse("HALLO Test").First())
                           .Should().Throw<UserException>();
            }
        }

        [Fact]
        public void ParseCsvFileTest()
        {
            var csv = File.ReadAllText("Services/dkb-import-test.csv", Encoding.UTF7);
            using (var test = new UnitTest<DkbCsvParser>())
            {
                var result = test.Target.Parse(csv).ToArray();
                var firstLine = result.First();
                firstLine.BookingDay.Date.Should().Be(DateTime.Parse("2019-10-25"));
                firstLine.Valuta.Date.Should().Be(DateTime.Parse("2019-10-25"));
                firstLine.Type.Should().Be("FOLGELASTSCHRIFT");
                firstLine.Partner.Should().Be("LastschriftVon");
                firstLine.Detail.Should().Be("DetailInfo1 äöü");
                firstLine.AccountNumber.Should().Be("DE11111111111111111111");
                firstLine.BankingCode.Should().Be("TUBDDEDD");
                firstLine.Amount.Should().Be(-12.34M);
                firstLine.DebteeId.Should().Be("DE22222222222222222222");
                firstLine.Reference1.Should().Be("mref1");
                firstLine.Reference2.Should().Be("kref1");
            }
        }
    }
}

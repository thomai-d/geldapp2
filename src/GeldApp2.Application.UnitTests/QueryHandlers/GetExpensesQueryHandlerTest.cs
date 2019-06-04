using FluentAssertions;
using GeldApp2.Application.Queries;
using GeldApp2.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeldApp2.Application.UnitTests.Queries.Expense
{
    public class GetExpensesQueryHandlerTest
    {
        private GetExpensesQueryHandler target;

        public GetExpensesQueryHandlerTest()
        {
            this.target = new GetExpensesQueryHandler(this.GetTestDb());
        }

        [Theory]
        [InlineData("", new long[] { 1, 2, 3, 4, 5, 6, 7 }, "Find all with future")]
        [InlineData("", new long[] { 1, 2, 3, 4, 5, 6 }, "Find all without future", false)]
        [InlineData("Schokolade", new long[0], "Zero results")]
        [InlineData("Essen", new long[] { 1, 2 }, "Find Category")]
        [InlineData("Pidser", new long[] { 1 }, "Find Subcategory")]
        [InlineData("Calzone", new long[] { 1 }, "Find Details")]
        [InlineData("1000,12", new long[] { 6 }, "Find amount")]
        [InlineData("!month:2", new long[] { 4, 5 }, "Month query")]
        [InlineData("!month:1 and year:2", new long[] { 6 }, "Month and year query")]
        [InlineData("!category:Essen", new long[] { 1, 2 }, "Kategorie")]
        [InlineData("!category:'Essen'", new long[] { 1, 2 }, "Kategorie")]
        [InlineData("!subcategory:'Pidser'", new long[] { 1 }, "Unterkategorie")]
        public async Task SearchTest(string searchText, long[] resultIds, string reason, bool includeFuture = true)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("DE");
            var cmd = new GetExpensesQuery("xxx")
            {
                SearchText = searchText,
                Account = new Account(1, "xxx"),
                IncludeFuture = includeFuture,
                Limit = 100,
                Offset = 0
            };

            var result = await this.target.Handle(cmd, CancellationToken.None);
            result.Select(i => i.Id).OrderBy(i => i).Should().BeEquivalentTo(resultIds, reason);
        }

        private GeldAppContext GetTestDb()
        {
            DateTime D(int y, int m, int d) => new DateTime(y, m, d);
            var builder = new DbContextOptionsBuilder<GeldAppContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString(), i => { });
            var db = new GeldAppContext(builder.Options);

            var account1 = new Account(1, "Ich");
            var account2 = new Account(2, "Helmut Kohl");
            db.Expenses.Add(new Database.Expense { Id = 1, Date = D(1, 1, 1), Account = account1, Amount = -1, Category = "Essen", Subcategory = "Pidser", Details = "Calzone" });
            db.Expenses.Add(new Database.Expense { Id = 2, Date = D(1, 1, 1), Account = account1, Amount = -2, Category = "Essen", Subcategory = "Coler", Details = "Coca Coler" });
            db.Expenses.Add(new Database.Expense { Id = 3, Date = D(1, 1, 2), Account = account1, Amount = -3, Category = "Auto", Subcategory = "Öl", Details = "Racing" });
            db.Expenses.Add(new Database.Expense { Id = 4, Date = D(2, 2, 1), Account = account1, Amount = -4, Category = "Auto", Subcategory = "Benzin", Details = "E10" });
            db.Expenses.Add(new Database.Expense { Id = 5, Date = D(2, 2, 1), Account = account1, Amount = 100, Category = "Gehalt", Subcategory = "Arbyte", Details = "", Type = ExpenseType.Revenue });
            db.Expenses.Add(new Database.Expense { Id = 6, Date = D(2, 1, 1), Account = account1, Amount = 1000.12M, Category = "Gehalt", Subcategory = "Schwarzgeld", Details = "", Type = ExpenseType.Revenue });
            db.Expenses.Add(new Database.Expense { Id = 7, Date = D(3000, 1, 1), Account = account1, Amount = 1000000, Category = "Zocken", Subcategory = "Lotto", Details = "", Type = ExpenseType.Revenue });
            db.Expenses.Add(new Database.Expense { Id = 8, Date = D(1, 1, 1), Account = account2, Amount = -1, Category = "Essen", Subcategory = "Pidser", Details = "Calzone" });
            db.SaveChanges();

            return db;
        }
    }
}

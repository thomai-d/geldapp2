using FluentAssertions;
using GeldApp2.Database;
using GeldApp2.Database.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace GeldApp2.IntegrationTests
{
    /// <summary>
    /// Tests all requests for import subsystem and adds some access tests.
    /// </summary>
    public class ImportedExpensesTests
    {
        [Fact]
        public async Task GetExpensesAccessTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");

                await fixture.ExpectGetAsync("/api/account/Hans/imports/unhandled", HttpStatusCode.OK);

                fixture.Logout();

                await fixture.ExpectGetAsync("/api/account/Hans/imports/unhandled", HttpStatusCode.Unauthorized);

                await fixture.Login("Petra");
                await fixture.ExpectGetAsync("/api/account/Hans/imports/unhandled", HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task FullTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");

                // Verify db is empty.
                var imported = await fixture.GetAsync<ImportedExpense[]>("/api/account/Hans/imports/unhandled");
                imported.Length.Should().Be(0);

                // Import.
                var csv = File.ReadAllBytes("Import/dkb-import-test.csv");
                await fixture.PostFileAsync("/api/account/Hans/import/csv", "csvFile", "file.csv", csv);

                // Check imported stuff.
                imported = await fixture.GetAsync<ImportedExpense[]>("/api/account/Hans/imports/unhandled");
                var importedExpense = imported.Single();
                importedExpense.Amount.Should().Be(-12.34M);
                importedExpense.BookingDay.Should().Be(DateTime.Parse("2019-10-25"));

                // Add some expenses.
                await fixture.AddExpenseAsync("Hans", -12.34M, "Wrong", "Subcategory", ex => ex.Date = importedExpense.BookingDay.AddDays(-100).Date);
                await fixture.AddExpenseAsync("Hans", -12.34M, "Wrong", "Subcategory", ex => ex.Date = importedExpense.BookingDay.AddDays(+100).Date);
                await fixture.AddExpenseAsync("Hans", -12, "Wrong", "Subcategory", ex => ex.Date = importedExpense.BookingDay.AddDays(-2).Date);
                await fixture.AddExpenseAsync("Hans", -12.34M, "Correct", "Subcategory", ex => ex.Date = importedExpense.BookingDay.AddDays(-2).Date);
                (await fixture.GetExpensesAsync("Hans")).Should().HaveCount(4);

                // Get related expenses.
                var relatedExpenses = await fixture.GetAsync<ExpenseViewModel[]>("/api/account/Hans/expenses?relatedToImportedExpense=" + importedExpense.Id);
                relatedExpenses.Should().HaveCount(1);
                var relatedExpense = relatedExpenses.Single();
                relatedExpense.CategoryName.Should().Be("Correct");

                // Handle expense by linking it.
                await fixture.ExpectPostAsync($"/api/account/Hans/import/link?importedExpenseId={importedExpense.Id}&relatedExpenseId={relatedExpense.Id}", HttpStatusCode.OK);
                (await fixture.GetExpensesAsync("Hans")).Should().HaveCount(4);
                imported = await fixture.GetAsync<ImportedExpense[]>("/api/account/Hans/imports/unhandled");
                imported.Should().HaveCount(0);

                // Check link.
                var modifiedImportedExpense = fixture.QueryDb(db => db.ImportedExpenses.Include(ex => ex.Expense).Where(ex => ex.Expense != null).Single());
                modifiedImportedExpense.Expense.Id.Should().Be(relatedExpense.Id);
            }
        }

        [Fact]
        public async Task HandleTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");

                // Import.
                var csv = File.ReadAllBytes("Import/dkb-import-test.csv");
                await fixture.PostFileAsync("/api/account/Hans/import/csv", "csvFile", "file.csv", csv);

                // Check imported stuff.
                var imported = await fixture.GetAsync<ImportedExpense[]>("/api/account/Hans/imports/unhandled");
                var importedExpense = imported.Single();

                // Handle expense by linking it.
                await fixture.ExpectPostAsync($"/api/account/Hans/imports/{importedExpense.Id}/handle", HttpStatusCode.OK);
                
                // No more unhandled imported expenses expected.
                imported = await fixture.GetAsync<ImportedExpense[]>("/api/account/Hans/imports/unhandled");
                imported.Should().HaveCount(0);
            }
        }
        
        [Fact]
        public async Task HandleByCreatingExpenseTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");

                // Import.
                var csv = File.ReadAllBytes("Import/dkb-import-test.csv");
                await fixture.PostFileAsync("/api/account/Hans/import/csv", "csvFile", "file.csv", csv);

                // Check imported stuff.
                var imported = await fixture.GetAsync<ImportedExpense[]>("/api/account/Hans/imports/unhandled");
                var importedExpense = imported.Single();

                // No expenses yet.
                (await fixture.GetExpensesAsync("Hans")).Should().HaveCount(0);

                // Handle expense by creating new expense.
                await fixture.AddExpenseAsync("Hans", 10, "Cat", "Sub", modCmd: cmd => cmd.HandlesImportedExpenseId = importedExpense.Id);
                (await fixture.GetExpensesAsync("Hans")).Should().HaveCount(1);

                // No more unhandled imported expenses expected.
                imported = await fixture.GetAsync<ImportedExpense[]>("/api/account/Hans/imports/unhandled");
                imported.Should().HaveCount(0);
            }
        }

        [Fact]
        public async Task EnsureThatExpensesFromOtherAccountsDoNotLeak()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");

                // Import.
                var csv = File.ReadAllBytes("Import/dkb-import-test.csv");
                await fixture.PostFileAsync("/api/account/Hans/import/csv", "csvFile", "file.csv", csv);
                var hansImportedExpense = (await fixture.GetAsync<ImportedExpense[]>("/api/account/Hans/imports/unhandled")).Single();

                // Add some expenses.
                await fixture.AddExpenseAsync("Hans", -12.34M, "Correct", "Subcategory", ex => ex.Date = hansImportedExpense.BookingDay.AddDays(-2).Date);
                
                // Get related expenses.
                var relatedExpenses = await fixture.GetAsync<ExpenseViewModel[]>("/api/account/Hans/expenses?relatedToImportedExpense=" + hansImportedExpense.Id);
                relatedExpenses.Should().HaveCount(1);
                relatedExpenses.Single().CategoryName.Should().Be("Correct");

                // Login as different user.
                fixture.Logout();
                await fixture.Login("Petra");

                // Simple leak.
                await fixture.ExpectGetAsync("/api/account/Hans/expenses?relatedToImportedExpense=" + hansImportedExpense.Id, HttpStatusCode.Unauthorized);
                
                // Leak via related expenses.
                await fixture.PostFileAsync("/api/account/Petra/import/csv", "csvFile", "file.csv", csv);
                var petraImportedExpense = (await fixture.GetAsync<ImportedExpense[]>("/api/account/Petra/imports/unhandled")).Single();
                var petraRelatedExpenses = await fixture.GetAsync<ExpenseViewModel[]>("/api/account/Petra/expenses?relatedToImportedExpense=" + petraImportedExpense.Id);
                petraRelatedExpenses.Should().BeEmpty();

                // Leak via linking.
                await fixture.ExpectPostAsync($"/api/account/Hans/import/link?importedExpenseId={hansImportedExpense.Id}&relatedExpenseId={relatedExpenses.Single().Id}", HttpStatusCode.Unauthorized);
                await fixture.ExpectPostAsync($"/api/account/Petra/import/link?importedExpenseId={hansImportedExpense.Id}&relatedExpenseId={relatedExpenses.Single().Id}", HttpStatusCode.NotFound);

                // Leak via creating.
                await fixture.ExpectAddExpenseAsync("Hans", 10, "Cat", "Sub", modCmd: cmd => cmd.HandlesImportedExpenseId = hansImportedExpense.Id, expectedStatus: HttpStatusCode.Unauthorized);
                await fixture.ExpectAddExpenseAsync("Petra", 10, "Cat", "Sub", modCmd: cmd => cmd.HandlesImportedExpenseId = hansImportedExpense.Id, expectedStatus: HttpStatusCode.NotFound);
            }
        }
    }
}

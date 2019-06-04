using FluentAssertions;
using GeldApp2.Database;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace GeldApp2.IntegrationTests
{
    /// <summary>
    /// Tests all requests for expense subsystem and adds some access tests.
    /// </summary>
    public class ExpenseTests
    {
        [Fact]
        public async Task GetExpensesAccessTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");

                await fixture.ExpectGetAsync("/api/account/Hans/expenses", HttpStatusCode.OK);
                await fixture.ExpectGetAsync("/api/account/Shared/expenses", HttpStatusCode.OK);
                await fixture.ExpectGetAsync("/api/account/Petra/expenses", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/NonExistend/expenses", HttpStatusCode.Unauthorized);

                fixture.Logout();

                await fixture.ExpectGetAsync("/api/account/Hans/expenses", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Shared/expenses", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Petra/expenses", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/NonExistend/expenses", HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task CreateUpdateDeleteExpenseTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");
                (await fixture.GetExpensesAsync("Hans")).Should().HaveCount(0);

                // Create.
                var cmd = new Expense(-100, "Ausgabe", "Essen", "Pizza").AsCommand("Hans");
                (await fixture.Client.PostAsync("/api/account/Hans/expenses", cmd.AsContent())).ShouldBeOk();
                var exp = (await fixture.GetExpensesAsync("Hans")).Single();
                exp.Amount.Should().Be(-100);

                // Edit.
                exp.Amount = -200;
                (await fixture.Client.PutAsync($"/api/account/Hans/expense/{exp.Id}", exp.AsContent())).ShouldBeOk();
                exp = (await fixture.GetExpensesAsync("Hans")).Single();
                exp.Amount.Should().Be(-200);
                exp.Details.Should().Be("pidser", "PipelineBehaviors should be enabled :)");

                // Get Single.
                exp = await fixture.GetExpenseAsync("Hans", exp.Id);
                exp.Amount.Should().Be(-200);

                // Delete.
                (await fixture.Client.DeleteAsync($"/api/account/Hans/expense/{exp.Id}")).ShouldBeOk();
                (await fixture.GetExpensesAsync("Hans")).Should().HaveCount(0);
            }
        }

        [Fact]
        public async Task UsersShouldNotHaveAccessToForeignAccountsTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");
                (await fixture.GetExpensesAsync("Hans")).Should().HaveCount(0);

                // Create.
                var cmd = new Expense(-100, "Ausgabe", "Essen", "Pizza").AsCommand("Hans");
                (await fixture.Client.PostAsync("/api/account/Hans/expenses", cmd.AsContent())).ShouldBeOk();
                var exp = (await fixture.GetExpensesAsync("Hans")).Single();

                // Switch user.
                fixture.Logout();
                await fixture.Login("Petra");

                // Check foreign user actions.
                (await fixture.GetExpensesAsync("Petra")).Should().HaveCount(0);
                (await fixture.Client.GetAsync("/api/account/Hans/expenses")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
                (await fixture.Client.GetAsync($"/api/account/Hans/expense/{exp.Id}")).IsUnauthorized();
                (await fixture.Client.PostAsync($"/api/account/Hans/expenses", exp.AsContent())).IsUnauthorized();
                (await fixture.Client.PutAsync($"/api/account/Hans/expense/{exp.Id}", exp.AsContent())).IsUnauthorized();
                (await fixture.Client.DeleteAsync($"/api/account/Hans/expense/{exp.Id}")).IsUnauthorized();
            }
        }
    }
}

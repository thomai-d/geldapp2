using FluentAssertions;
using GeldApp2.Application.Commands.Category;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace GeldApp2.IntegrationTests
{
    /// <summary>
    /// Tests basic functionality and also adds some more access tests. Just to be safe.
    /// </summary>
    public class CategoryTests
    {
        [Fact]
        public async Task CreateDeleteCategoryTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");
                var categories = await fixture.GetCategoriesAsync("Hans");
                categories.Single().Name.Should().Be("Ausgaben");

                // Create
                var cmd = new CreateCategoryCommand("Hans", "Einnahmen");
                (await fixture.Client.PostAsync("/api/account/Hans/categories", cmd.AsContent())).ShouldBeOk();
                categories = await fixture.GetCategoriesAsync("Hans");
                categories.Skip(1).Single().Name.Should().Be("Einnahmen");

                // Create sub
                (await fixture.Client.PutAsync("/api/account/Hans/category/Einnahmen/Aktien", null)).ShouldBeOk();
                categories = await fixture.GetCategoriesAsync("Hans");
                categories.Skip(1).Single().Subcategories.Single().Should().Be("Aktien");

                // Delete sub
                (await fixture.Client.DeleteAsync("/api/account/Hans/category/Einnahmen/Aktien")).ShouldBeOk();
                categories = await fixture.GetCategoriesAsync("Hans");
                categories.Skip(1).Single().Subcategories.Should().HaveCount(0);

                // Delete main
                (await fixture.Client.DeleteAsync("/api/account/Hans/category/Einnahmen")).ShouldBeOk();
                categories = await fixture.GetCategoriesAsync("Hans");
                categories.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task EnsureAccessIsVerifiedTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                var testCmd = new CreateCategoryCommand("Hans", "Einnahmen");

                // Test authenticated.
                await fixture.Login("Petra");
                await fixture.ExpectGetAsync("/api/account/Hans/categories", HttpStatusCode.Unauthorized);
                (await fixture.Client.PostAsync("/api/account/Hans/categories", testCmd.AsContent())).IsUnauthorized();
                (await fixture.Client.PostAsync("/api/account/Teal'C/categories", testCmd.AsContent())).IsUnauthorized();
                (await fixture.Client.PutAsync("/api/account/Hans/category/Einnahmen/Aktien", null)).IsUnauthorized();
                (await fixture.Client.PutAsync("/api/account/SamCarter/category/Einnahmen/Aktien", null)).IsUnauthorized();
                (await fixture.Client.DeleteAsync("/api/account/Hans/category/Einnahmen/Aktien")).IsUnauthorized();
                (await fixture.Client.DeleteAsync("/api/account/Hans/category/Einnahmen")).IsUnauthorized();
                (await fixture.Client.DeleteAsync("/api/account/Hans/category/Schwarzgeld")).IsUnauthorized();
                (await fixture.Client.DeleteAsync("/api/account/JackONeil/category/Schwarzgeld")).IsUnauthorized();

                // Test unauthenticated.
                fixture.Logout();
                await fixture.ExpectGetAsync("/api/account/Hans/categories", HttpStatusCode.Unauthorized);
                (await fixture.Client.PostAsync("/api/account/Hans/categories", testCmd.AsContent())).IsUnauthorized();
                (await fixture.Client.PostAsync("/api/account/Teal'C/categories", testCmd.AsContent())).IsUnauthorized();
                (await fixture.Client.PutAsync("/api/account/Hans/category/Einnahmen/Aktien", null)).IsUnauthorized();
                (await fixture.Client.PutAsync("/api/account/SamCarter/category/Einnahmen/Aktien", null)).IsUnauthorized();
                (await fixture.Client.DeleteAsync("/api/account/Hans/category/Einnahmen/Aktien")).IsUnauthorized();
                (await fixture.Client.DeleteAsync("/api/account/Hans/category/Einnahmen")).IsUnauthorized();
                (await fixture.Client.DeleteAsync("/api/account/Hans/category/Schwarzgeld")).IsUnauthorized();
                (await fixture.Client.DeleteAsync("/api/account/JackONeil/category/Schwarzgeld")).IsUnauthorized();
            }
        }
    }
}

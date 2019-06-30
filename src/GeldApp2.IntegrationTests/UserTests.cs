using FluentAssertions;
using GeldApp2.Application.Commands.Category;
using GeldApp2.Database.ViewModels;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace GeldApp2.IntegrationTests
{
    public class UserTests
    {
        [Fact]
        public async Task AuthorizationTests()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Admin");
                (await fixture.Client.GetAsync("/api/users")).ShouldBeOk();
                fixture.Logout();

                await fixture.Login("Hans");
                (await fixture.Client.GetAsync("/api/users")).ShouldBeForbidden();
                fixture.Logout();
            }
        }

        [Fact]
        public async Task GetSummaryTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Admin");
                var users = await fixture.GetAsync<UserSummary[]>("/api/users");
                users.Should().HaveCount(3);
                users[0].Name.Should().Be("Hans");
                users[1].Name.Should().Be("Petra");
                users[2].Name.Should().Be("Admin");
            }
        }
    }
}

using FluentAssertions;
using GeldApp2.Application.Commands.Category;
using GeldApp2.Application.Commands.Users;
using GeldApp2.Application.Commands.Users;
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

        [Theory]
        [InlineData("Hans", "x", false, false, false)]
        [InlineData("Admin", "Admin", true, false, false)]
        [InlineData("Admin", "", true, false, false)]
        [InlineData("Admin", "Neu", true, true, false)]
        [InlineData("Admin", "Neu", true, true, true)]
        public async Task PostUserText(string user, string newUserName, bool shouldBeAllowed, bool shouldSucceed, bool createDefaultAccount)
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login(user);
                var result = await fixture.Client.PostAsync("/api/users", new CreateUserCommand(newUserName, "password", createDefaultAccount).AsContent());
                if (!shouldBeAllowed)
                {
                    result.ShouldBeForbidden();
                    return;
                }

                if (!shouldSucceed)
                {
                    result.ShouldClientFail();
                    return;
                }

                var users = await fixture.GetAsync<UserSummary[]>("/api/users");
                users.Should().HaveCount(4);
                users[0].Name.Should().Be("Hans");
                users[1].Name.Should().Be("Petra");
                users[2].Name.Should().Be("Admin");
                users[3].Name.Should().Be("Neu");

                if (createDefaultAccount)
                    users[3].Accounts.Single().Should().Be("Neu");
                else
                    users[3].Accounts.Should().BeEmpty();
            }
        }
    }
}

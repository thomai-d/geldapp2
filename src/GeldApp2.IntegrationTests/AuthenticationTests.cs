﻿using FluentAssertions;
using GeldApp2.Application.Commands.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace GeldApp2.IntegrationTests
{
    /// <summary>
    /// This integration test ensures that every single API endpoint is protected by the authentication mechanism.
    /// If you add endpoints, also ensure that a test is added here.
    /// </summary>
    public class AuthenticationTests : IClassFixture<GeldAppFixture>
    {
        private readonly GeldAppFixture fixture;

        public AuthenticationTests(GeldAppFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData("Admin", "abc123", true, true)]
        [InlineData("Hans", "abc123", true, false)]
        [InlineData("hans", "abc123", false, false)]
        [InlineData("Hans", "Abc123", false, false)]
        [InlineData("Hans", "abc123!", false, false)]
        [InlineData("Hans' AND 1 = 1;--", "abc123", false, false)]
        [InlineData("abc123", "abc123", false, false)]
        [InlineData("", "", false, false)]
        [InlineData("§(%$=)/$&!", "=(%=)§%()§/&=", false, false)]
        public async Task LoginTest(string user, string pass, bool authorize, bool isAdmin)
        {
            var auth = new LoginCommand { Username = user, Password = pass };
            var resp = await this.fixture.Client.PostAsync("/api/auth/login", auth.AsContent());
            resp.StatusCode.Should().Be(authorize ? HttpStatusCode.OK : HttpStatusCode.Unauthorized);

            if (authorize)
            {
                var token = await resp.GetJwtTokenAsync();

                if (isAdmin)
                    token.Claims.Should().Contain(t => t.Type == ClaimTypes.Role && t.Value == "admin");
                else
                    token.Claims.Should().NotContain(t => t.Type == ClaimTypes.Role && t.Value == "admin");
            }
        }

        [Fact]
        public async Task IpBlockerTest()
        {
            using (var fixture = new GeldAppSecurityFixture())
            {
                for (var n = 0; n < 15; n++)
                {
                    var auth = new LoginCommand { Username = "hans", Password = "wrong" };
                    var resp = await fixture.Client.PostAsync("/api/auth/login", auth.AsContent());
                    resp.StatusCode.Should().Be(n <= 10 ? HttpStatusCode.Unauthorized : HttpStatusCode.TooManyRequests);
                }
            }
        }

        [Theory]
        [InlineData("/api/accounts/summary/month")]
        [InlineData("/api/account/Hans/categories")]
        [InlineData("/api/account/Hans/categories/predict")]
        [InlineData("/api/account/Hans/charts/month-by-category")]
        [InlineData("/api/account/Hans/charts/expense-history")]
        [InlineData("/api/account/Hans/charts/revenue-history")]
        [InlineData("/api/account/Hans/expenses")]
        [InlineData("/api/account/Hans/expense/1")]
        [InlineData("/api/account/Hans/export/tsv")]
        [InlineData("/api/auth/refresh")]
        [InlineData("/api/users")]
        public async Task GetControllersAreNotAccessibleWithoutAuthentication(string url)
        {
            var result = await this.fixture.Client.GetAsync(url);
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("/api/auth/changePassword")]
        [InlineData("/api/account/Hans/categories")]
        [InlineData("/api/account/Hans/expenses")]
        [InlineData("/api/account/Hans/charts/compare-category")]
        [InlineData("/api/account/Hans/import/csv")]
        [InlineData("/api/users")]
        public async Task PostControllersAreNotAccessibleWithoutAuthentication(string url)
        {
            var result = await this.fixture.Client.PostAsync(url, new StringContent(""));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("/api/users")]
        public async Task PostControllersAreNotAccessibleWithoutAdminRights(string url)
        {
            await this.fixture.Login("Hans");
            var result = await this.fixture.Client.PostAsync(url, new StringContent(""));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("/api/account/Hans/category/Cat/Sub")]
        [InlineData("/api/account/Hans/expense/1")]
        public async Task PutControllersAreNotAccessibleWithoutAuthentication(string url)
        {
            var result = await this.fixture.Client.PutAsync(url, new StringContent(""));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("/api/account/Hans/category/Test")]
        [InlineData("/api/account/Hans/category/Test/Subcategory")]
        [InlineData("/api/account/Hans/expense/1")]
        public async Task DeleteControllersAreNotAccessibleWithoutAuthentication(string url)
        {
            var result = await this.fixture.Client.DeleteAsync(url);
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RefreshTokenTests()
        {
            using (var fixture = new GeldAppFixture())
            {
                // Not logged in.
                var result = await fixture.Client.GetAsync("/api/auth/refresh?token=refreshMe");
                result.IsUnauthorized();

                // Invalid token.
                await fixture.Login("Hans");
                result = await fixture.Client.GetAsync("/api/auth/refresh?token=invalid");
                result.IsUnauthorized();

                // Valid token.
                result = await fixture.Client.GetAsync("/api/auth/refresh?token=refreshMe");
                result.ShouldBeOk();
                var content = await result.Content.ReadAsStringAsync();
                content.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task ChangePasswordTests()
        {
            var wrongOldPwd = new ChangePasswordCommand { OldPassword = "111", NewPassword = "12345" };
            var passTooShort = new ChangePasswordCommand { OldPassword = "abc123", NewPassword = "123" };
            var validRequest = new ChangePasswordCommand { OldPassword = "abc123", NewPassword = "12345" };

            using (var fixture = new GeldAppFixture())
            {
                // Not logged in.
                var result = await fixture.Client.PostAsync("/api/auth/changePassword", validRequest.AsContent());
                result.IsUnauthorized();

                await fixture.Login("Hans");
                
                // Invalid old password.
                result = await fixture.Client.PostAsync("/api/auth/changePassword", wrongOldPwd.AsContent());
                result.ShouldBeForbidden();
                
                // New password too short.
                result = await fixture.Client.PostAsync("/api/auth/changePassword", passTooShort.AsContent());
                result.ShouldClientFail();
                
                // Change password.
                result = await fixture.Client.PostAsync("/api/auth/changePassword", validRequest.AsContent());
                result.ShouldBeOk();

                fixture.Logout();

                // Login with old creds should fail.
                fixture.Invoking(f => f.Login("Hans").Wait()).Should().Throw<AuthenticationException>();

                // Login with new password should succeed.
                await fixture.Login("Hans", "12345");
            }
        }
    }
}

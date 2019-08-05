using FluentAssertions;
using GeldApp2.Application.Commands.Users;
using GeldApp2.Database;
using GeldApp2.Database.Abstractions;
using GeldApp2.Database.ViewModels;
using GeldApp2.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace GeldApp2.IntegrationTests
{
    /// <summary>
    /// Test fixture for the geldapp API.
    /// Contains basic helpers for easy unit testing.
    /// </summary>
    public class GeldAppFixture : IDisposable
    {

        public readonly HttpClient Client;
        public readonly WebApplicationFactory<Startup> Sut;

        public GeldAppFixture()
        {
            Environment.SetEnvironmentVariable("INTEGRATION_TEST", "1");
            Environment.SetEnvironmentVariable("AuthenticationSettings__JwtTokenSecret", "INTEGRATION_TEST");
            this.Sut = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureLogging((ILoggingBuilder l) => l.ClearProviders());
                    builder.ConfigureTestServices(services =>
                    {
                        if (this.disableSecurity)
                            services.AddSingleton(s => new Mock<IIpBlockerService>().Object);

                        services.AddScoped(s => new Mock<IDatabaseMigrator>().Object);
                        services.AddScoped(s => new Mock<ISqlQuery>().Object);
                    });
                    builder.ConfigureServices(services =>
                    {
                        var serviceProvider = new ServiceCollection()
                                                .AddEntityFrameworkInMemoryDatabase()
                                                .BuildServiceProvider();

                        services.AddDbContext<GeldAppContext>(opt =>
                        {
                            opt.UseInMemoryDatabase("IntegrationTests");
                            opt.UseInternalServiceProvider(serviceProvider);
                        });

                        using (var scope = services.BuildServiceProvider().CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<GeldAppContext>();
                            this.SeedDatabase(db);
                        }
                    });
                });

            this.Client = Sut.CreateDefaultClient();
        }

        protected virtual bool disableSecurity { get; } = true;

        // Test helpers.

        public async Task<T> GetAsync<T>(string url)
        {
            var resp = await this.Client.GetAsync(url);
            return await resp.AsAsync<T>();
        }

        public async Task ExpectGetAsync(string url, HttpStatusCode status)
        {
            var resp = await this.Client.GetAsync(url);
            resp.StatusCode.Should().Be(status);
        }

        public async Task ExpectPostAsync(string url, HttpContent content, HttpStatusCode status)
        {
            var resp = await this.Client.PostAsync(url, content);
            resp.StatusCode.Should().Be(status);
        }

        public T GetService<T>()
        {
            using (var scope = this.Sut.Server.Host.Services.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<T>();
            }
        }

        // High level api calls.

        public async Task Login(string user, string password = "abc123")
        {
            var auth = new LoginCommand { Username = user, Password = password };
            var resp = await this.Client.PostAsync("/api/auth/login", auth.AsContent());
            if (resp.StatusCode != HttpStatusCode.OK)
                throw new AuthenticationException();
            var content = await resp.Content.ReadAsStringAsync();
            var bearerToken = JObject.Parse(content)["token"].Value<string>();
            this.Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
        }

        public void Logout()
        {
            this.Client.DefaultRequestHeaders.Remove("Authorization");
        }

        public async Task<ExpenseViewModel[]> GetExpensesAsync(string accountName)
        {
            return await this.GetAsync<ExpenseViewModel[]>($"/api/account/{accountName}/expenses");
        }

        public async Task<CategoryViewModel[]> GetCategoriesAsync(string accountName)
        {
            return await this.GetAsync<CategoryViewModel[]>($"/api/account/{accountName}/categories");
        }

        public async Task<ExpenseViewModel> GetExpenseAsync(string accountName, long id)
        {
            return await this.GetAsync<ExpenseViewModel>($"/api/account/{accountName}/expense/{id}");
        }

        public async Task AddExpenseAsync(string accountName, decimal amount, string category, string subcategory)
        {
            var cmd = new Expense(amount, category, subcategory).AsCommand(accountName);
            (await this.Client.PostAsync($"/api/account/{accountName}/expenses", cmd.AsContent())).ShouldBeOk();
        }

        // Internal stuff.

        public void Dispose()
        {
            this.Sut.Dispose();
            this.Client.Dispose();
        }

        private void SeedDatabase(GeldAppContext db)
        {
            var cat1 = new Category("Ausgaben");
            var cat2 = new Category("Ausgaben");
            var cat3 = new Category("Ausgaben");
            cat1.Subcategories.Add(new Subcategory("Essen"));
            cat2.Subcategories.Add(new Subcategory("Trinken"));
            cat3.Subcategories.Add(new Subcategory("Naschen"));

            var sharedAccount = new Account("Shared", cat2);

            var hans = new User("Hans", "abc123") { RefreshToken = "refreshMe" };
            hans.AddAccount(new Account("Hans", cat1));
            hans.AddAccount(sharedAccount);
            db.Users.Add(hans);

            var petra = new User("Petra", "abc123");
            petra.AddAccount(new Account("Petra", cat3));
            petra.AddAccount(sharedAccount);
            db.Users.Add(petra);

            var admin = User.CreateAdmin("Admin", "abc123");
            db.Users.Add(admin);

            db.SaveChanges();
        }
    }

    public class GeldAppSecurityFixture : GeldAppFixture
    {
        protected override bool disableSecurity => false;
    }
}

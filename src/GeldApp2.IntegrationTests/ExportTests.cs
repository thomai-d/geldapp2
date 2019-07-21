using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GeldApp2.IntegrationTests
{
    public class ExportTests
    {
        [Fact]
        public async Task GetExpensesAccessTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");

                await fixture.ExpectGetAsync("/api/account/Hans/export/tsv", HttpStatusCode.OK);
                await fixture.ExpectGetAsync("/api/account/Shared/export/tsv", HttpStatusCode.OK);
                await fixture.ExpectGetAsync("/api/account/Petra/expenses", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Unknown/expenses", HttpStatusCode.Unauthorized);

                fixture.Logout();

                await fixture.ExpectGetAsync("/api/account/Hans/export/tsv", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Shared/export/tsv", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Petra/expenses", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Unknown/expenses", HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task CsvExportTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.Login("Hans");
                var resp = await fixture.Client.GetAsync("/api/account/Hans/export/tsv");
                resp.Content.Headers.ContentType.ToString().Should().Be("text/tab-separated-values");
                resp.ShouldBeOk();
                var content = await resp.Content.ReadAsStringAsync();
                content.Should().StartWith("Id\tAccountId\tCategory");
            }
        }
    }
}

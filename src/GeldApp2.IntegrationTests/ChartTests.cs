using GeldApp2.Application.Queries.Chart;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace GeldApp2.IntegrationTests
{
    /// <summary>
    /// Chart tests are boring.
    /// </summary>
    public class ChartTests
    {
        [Fact]
        public async Task EnsureAccessIsVerifiedTest()
        {
            using (var fixture = new GeldAppFixture())
            {
                await fixture.ExpectGetAsync("/api/account/Hans/charts/month-by-category", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Hans/charts/expense-history", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Hans/charts/revenue-history", HttpStatusCode.Unauthorized);
                await fixture.ExpectPostAsync("/api/account/Hans/charts/compare-category", new GetCompareCategoryChartQuery().AsContent(), HttpStatusCode.Unauthorized);

                await fixture.Login("Hans");
                await fixture.ExpectGetAsync("/api/account/Hans/charts/month-by-category", HttpStatusCode.OK);
                await fixture.ExpectGetAsync("/api/account/Hans/charts/expense-history", HttpStatusCode.OK);
                await fixture.ExpectGetAsync("/api/account/Hans/charts/revenue-history", HttpStatusCode.OK);
                await fixture.ExpectPostAsync("/api/account/Hans/charts/compare-category", new GetCompareCategoryChartQuery().AsContent(), HttpStatusCode.OK);
                fixture.Logout();

                await fixture.Login("Petra");
                await fixture.ExpectGetAsync("/api/account/Hans/charts/month-by-category", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Hans/charts/expense-history", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/Hans/charts/revenue-history", HttpStatusCode.Unauthorized);
                await fixture.ExpectGetAsync("/api/account/ColonelHogan/charts/revenue-history", HttpStatusCode.Unauthorized);
                await fixture.ExpectPostAsync("/api/account/Hans/charts/compare-category", new GetCompareCategoryChartQuery().AsContent(), HttpStatusCode.Unauthorized);
            }
        }
    }
}

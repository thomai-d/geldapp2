using System;
using System.Threading.Tasks;
using GeldApp2.Application.Queries.Chart;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private readonly IMediator mediator;

        public ChartsController(
            IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet, Route("/api/account/{accountName}/charts/month-by-category")]
        public async Task<LabelledChartItem[]> GetMonthChartByCategory(string accountName, [FromQuery]int month = 0, [FromQuery]int year = 0)
        {
            if (year == 0 || month == 0)
            {
                var now = DateTime.Now;
                year = now.Year;
                month = now.Month;
            }

            accountName = Uri.UnescapeDataString(accountName);
            return await this.mediator.Send(new GetAccountCategorySummaryQuery(accountName, month, year));
        }

        [HttpGet, Route("/api/account/{accountName}/charts/expense-history")]
        public async Task<DateChartItem[]> GetExpenseHistory(string accountName)
        {
            accountName = Uri.UnescapeDataString(accountName);
            return await this.mediator.Send(new GetExpenseHistoryByMonthQuery(accountName));
        }

        [HttpGet, Route("/api/account/{accountName}/charts/revenue-history")]
        public async Task<DateChartItem[]> GetRevenueHistory(string accountName)
        {
            accountName = Uri.UnescapeDataString(accountName);
            return await this.mediator.Send(new GetRevenueHistoryByMonthQuery(accountName));
        }

        [HttpPost, Route("/api/account/{accountName}/charts/compare-category")]
        public async Task<ExpenseRevenueLineChartsDto> GetCompareCategoryChart(string accountName, [FromBody]GetCompareCategoryChartQuery query)
        {
            query.AccountName = Uri.UnescapeDataString(accountName);
            return await this.mediator.Send(query);
        }
    }
}

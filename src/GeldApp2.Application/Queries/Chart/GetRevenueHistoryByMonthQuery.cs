using GeldApp2.Application.Commands;
using GeldApp2.Database.Abstractions;
using GeldApp2.Database.ViewModels;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries.Chart
{
    public class GetRevenueHistoryByMonthQuery : AccountRelatedRequest<DateChartItem[]>
    {
        public GetRevenueHistoryByMonthQuery(string accountName)
            : base(accountName)
        {
        }
    }

    public class GetRevenueHistoryByMonthQueryHandler
        : IRequestHandler<GetRevenueHistoryByMonthQuery, DateChartItem[]>
    {
        private readonly ISqlQuery sql;

        public GetRevenueHistoryByMonthQueryHandler(ISqlQuery sql)
        {
            this.sql = sql;
        }

        public async Task<DateChartItem[]> Handle(GetRevenueHistoryByMonthQuery request, CancellationToken cancellationToken)
        {
            var items = await this.sql.Query<MonthlyDataItem>(
                        $@"SELECT MONTH(Date) as Month, YEAR(Date) as Year, SUM(Amount) as Amount
                           FROM Expenses
                           WHERE AccountId = {request.Account.Id}
                             AND Amount > 0
                           GROUP BY MONTH(Date), YEAR(Date)
                           ORDER BY YEAR(Date), MONTH(Date)");

            return items
                .WithoutGaps()
                .Select(DateChartItem.From)
                .ToArray();
        }
    }
}

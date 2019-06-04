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
    public class GetExpenseHistoryByMonthQuery : AccountRelatedRequest<DateChartItem[]>
    {
        public GetExpenseHistoryByMonthQuery(string accountName)
            : base(accountName)
        {
        }

    }

    public class GetExpenseHistoryByMonthQueryHandler
        : IRequestHandler<GetExpenseHistoryByMonthQuery, DateChartItem[]>
    {
        private readonly ISqlQuery sql;

        public GetExpenseHistoryByMonthQueryHandler(ISqlQuery sql)
        {
            this.sql = sql;
        }

        public async Task<DateChartItem[]> Handle(GetExpenseHistoryByMonthQuery request, CancellationToken cancellationToken)
        {
            var items = await this.sql.Query<MonthlyDataItem>(
                        $@"SELECT MONTH(Date) as Month, YEAR(Date) as Year, SUM(Amount) as Amount
                           FROM Expenses
                           WHERE AccountId = {request.Account.Id}
                             AND Amount < 0
                           GROUP BY MONTH(Date), YEAR(Date)
                           ORDER BY YEAR(Date), MONTH(Date)");

            return items
                .WithoutGaps()
                .Select(DateChartItem.From)
                .ToArray();
        }
    }
}

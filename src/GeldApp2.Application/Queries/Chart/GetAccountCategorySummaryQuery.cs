using GeldApp2.Application.Commands;
using GeldApp2.Database.Abstractions;
using GeldApp2.Database.ViewModels;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries.Chart
{
    public class GetAccountCategorySummaryQuery : AccountRelatedRequest<LabelledChartItem[]>
    {
        public GetAccountCategorySummaryQuery(string accountName, int month, int year)
            : base(accountName)
        {
            this.Month = month;
            this.Year = year;
        }

        public int Month { get; }

        public int Year { get; }
    }

    public class GetAccountCategorySummaryQueryHandler
        : IRequestHandler<GetAccountCategorySummaryQuery, LabelledChartItem[]>
    {
        private readonly ISqlQuery sql;

        public GetAccountCategorySummaryQueryHandler(ISqlQuery sql)
        {
            this.sql = sql;
        }

        public Task<LabelledChartItem[]> Handle(GetAccountCategorySummaryQuery request, CancellationToken cancellationToken)
        {
            return this.sql.Query<LabelledChartItem>
                ($@"SELECT e.Category as Label, SUM(e.Amount) as Y
                    FROM Expenses e
                    WHERE e.AccountId = {request.Account.Id} 
                    AND YEAR(e.Date) = {request.Year} AND MONTH(e.Date) = {request.Month}
                    AND e.Amount < 0
                    GROUP BY e.Category");
        }
    }
}

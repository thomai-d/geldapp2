﻿using GeldApp2.Database;
using GeldApp2.Database.Abstractions;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries.Account
{
    public class GetAccountSummariesQuery : IRequest<AccountSummary[]>
    {
        public GetAccountSummariesQuery(User user, int month, int year)
        {
            this.User = user;
            this.Month = month;
            this.Year = year;
        }

        public User User { get; }

        public int Month { get; }

        public int Year { get; }
    }

    public class GetAccountSummariesQueryHandler : IRequestHandler<GetAccountSummariesQuery, AccountSummary[]>
    {
        private readonly ISqlQuery sql;

        public GetAccountSummariesQueryHandler(ISqlQuery sql)
        {
            this.sql = sql;
        }

        public async Task<AccountSummary[]> Handle(GetAccountSummariesQuery request, CancellationToken cancellationToken)
        {
            return await this.sql.Query<AccountSummary>($@"SELECT a.Name as AccountName,
                                                                    (SELECT IFNULL(SUM(e.Amount), 0)
                                                                     FROM Expenses e
                                                                     WHERE e.AccountId = a.Id
                                                                       AND MONTH(e.Date) = {request.Month}
                                                                       AND YEAR(e.Date) = {request.Year}
                                                                       AND e.Amount < 0) AS TotalExpenses
                                                               FROM UserAccount ua
                                                               JOIN Accounts a ON a.Id = ua.AccountId
                                                               WHERE ua.UserId = {request.User.Id}");
        }
    }
}

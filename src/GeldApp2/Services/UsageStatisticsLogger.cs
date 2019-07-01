using GeldApp2.Application.Logging;
using GeldApp2.Database;
using GeldApp2.Database.Abstractions;
using GeldApp2.Database.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Services
{
    public interface IUsageStatisticsLogger
    {
        Task LogUsageStatisticsAsync();
    }

    public class UsageStatisticsLogger : IUsageStatisticsLogger
    {
        private readonly ISqlQuery sql;
        private readonly ILogger<UsageStatisticsLogger> log;

        public UsageStatisticsLogger(
            ISqlQuery sql,
            ILogger<UsageStatisticsLogger> log)
        {
            this.sql = sql;
            this.log = log;
        }

        public async Task LogUsageStatisticsAsync()
        {
            var expensesPerAccount = await this.sql.Query<KeyValueItem>($@"SELECT Accounts.Name AS ""Key"", COUNT(*) ""Value"" FROM Accounts
                                            JOIN Expenses ON Expenses.AccountId = Accounts.Id
                                            GROUP BY Accounts.Name");

            foreach (var epa in expensesPerAccount)
            {
                this.log.LogInformation(
                    Events.UsageStatistics,
                    "There are currently {ExpenseCount} expenses for {Username}",
                    epa.Value,
                    epa.Key);
            }
        }
    }
}

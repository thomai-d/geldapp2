using Abstrakt.Basics;
using GeldApp2.Application.Logging;
using GeldApp2.Application.ML;
using GeldApp2.Database;
using GeldApp2.Database.Abstractions;
using GeldApp2.Database.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Services
{
    public interface ICategoryPredictionService : IAsyncStartableService
    {
        Task LearnCategoriesAsync();

        CategoryPredictionResult Predict(string accountName, float amount, DateTime created, DateTime expenseDate);
    }

    public class CategoryPredictionService : ICategoryPredictionService
    {
        public const int MinimumNumberOfExpensesForPrediction = 30;

        private readonly ConcurrentDictionary<string, CategoryPredictor> accountToPredictor = new ConcurrentDictionary<string, CategoryPredictor>();
        private readonly GeldAppContext db;
        private readonly IScheduler scheduler;
        private readonly ILogger<CategoryPredictionService> log;

        public CategoryPredictionService(
            GeldAppContext db,
            IScheduler scheduler,
            ILogger<CategoryPredictionService> log)
        {
            this.db = db;
            this.log = log;
            this.scheduler = scheduler;
        }

        public async Task LearnCategoriesAsync()
        {
            var accounts = await this.db.Accounts.ToArrayAsync();

            foreach (var account in accounts)
            {
                var w = Stopwatch.StartNew();
                var expenseCount = this.db.Expenses.Count(ex => ex.AccountId == account.Id);
                if (expenseCount < MinimumNumberOfExpensesForPrediction)
                    continue;

                var expenses = await this.db.Expenses.Where(ex => ex.AccountId == account.Id).ToArrayAsync();
                var predictor = new CategoryPredictor();
                predictor.Learn(expenses);
                this.accountToPredictor.AddOrUpdate(account.Name, predictor, (k, v) => predictor);
                w.Stop();

                this.log.LogInformation(Events.LearnCategoriesForAccount,
                                        "Learning categories for {AccountName} took {ElapsedMilliseconds}ms ({ExpenseCount} samples)",
                                        account.Name, w.ElapsedMilliseconds, expenseCount);
            }
        }

        public CategoryPredictionResult Predict(string accountName, float amount, DateTime created, DateTime expenseDate)
        {
            if (!this.accountToPredictor.TryGetValue(accountName, out var predictor))
                return CategoryPredictionResult.Empty;

            return predictor.Predict(amount, created, expenseDate);
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await this.LearnCategoriesAsync();
            this.scheduler.ScheduleEveryNight(() => this.LearnCategoriesAsync());
        }

        public Task StopAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}

using GeldApp2.Application.Commands;
using GeldApp2.Database;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries.Expense
{
    public class GetExpensesRelatedToImportedExpenseQuery : AccountRelatedRequest<ExpenseViewModel[]>
    {
        public GetExpensesRelatedToImportedExpenseQuery(string accountName, long importedExpenseId)
            : base(accountName)
        {
            this.ImportedExpenseId = importedExpenseId;
        }

        public long ImportedExpenseId { get; private set; }

        public TimeSpan DateTolerance { get; } = TimeSpan.FromDays(7);
    }

    public class GetExpensesRelatedToImportedExpenseQueryHandler : IRequestHandler<GetExpensesRelatedToImportedExpenseQuery, ExpenseViewModel[]>
    {
        private readonly GeldAppContext db;

        public GetExpensesRelatedToImportedExpenseQueryHandler(
            GeldAppContext db)
        {
            this.db = db;
        }


        public async Task<ExpenseViewModel[]> Handle(GetExpensesRelatedToImportedExpenseQuery request, CancellationToken cancellationToken)
        {
            var importedExpense = await this.db.ImportedExpenses.SingleAsync(i => i.Id == request.ImportedExpenseId);
            var startDate = importedExpense.BookingDay.Subtract(request.DateTolerance);
            var endDate = importedExpense.BookingDay.Add(request.DateTolerance);
            var expenses = await this.db.Expenses
                                    .Include(ex => ex.Account)
                                    .Where(ex => ex.AccountId == importedExpense.AccountId
                                              && ex.Amount == importedExpense.Amount
                                              && ex.Date >= startDate
                                              && ex.Date <= endDate)
                                    .ToArrayAsync();

            return expenses
                .Select(ExpenseViewModel.FromDb)
                .ToArray();
        }
    }
}

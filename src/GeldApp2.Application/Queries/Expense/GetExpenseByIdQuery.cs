using GeldApp2.Application.Commands;
using GeldApp2.Application.Exceptions;
using GeldApp2.Database;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries
{
    public class GetExpenseByIdQuery : AccountRelatedRequest<ExpenseViewModel>
    {
        public GetExpenseByIdQuery(string accountName, long expenseId)
            : base(accountName)
        {
            this.ExpenseId = expenseId;
        }

        public long ExpenseId { get; }
    }

    public class GetExpenseByIdQueryHandler
        : IRequestHandler<GetExpenseByIdQuery, ExpenseViewModel>
    {
        private readonly GeldAppContext db;

        public GetExpenseByIdQueryHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<ExpenseViewModel> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
        {
            var expense = await this.db.Expenses
                .Include(ex => ex.Account)
                .SingleOrDefaultAsync(ex => ex.Id == request.ExpenseId
                                         && ex.AccountId == request.Account.Id);

            if (expense == null)
                throw new NotFoundException($"Expense {request.ExpenseId}");

            return ExpenseViewModel.FromDb(expense);
        }
    }
}

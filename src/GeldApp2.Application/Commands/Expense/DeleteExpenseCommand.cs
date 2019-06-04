﻿using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands
{
    public class DeleteExpenseCommand : AccountRelatedRequest<bool>
    {
        public long Id { get; set; }
    }

    public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, bool>
    {
        private readonly GeldAppContext db;

        public DeleteExpenseCommandHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<bool> Handle(DeleteExpenseCommand cmd, CancellationToken cancellationToken)
        {
            var expense = await this.db.Expenses
                .SingleOrDefaultAsync(xp => xp.Id == cmd.Id && xp.AccountId == cmd.Account.Id);

            if (expense == null)
            {
                throw new ArgumentException($"Expense {cmd.Id} not found for account '{cmd.AccountName}'");
            }

            this.db.Expenses.Remove(expense);
            await this.db.SaveChangesAsync();

            return true;
        }
    }
}

using FluentValidation;
using GeldApp2.Application.Logging;
using GeldApp2.Database;
using GeldApp2.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Expense
{
    public class UpdateExpenseCommand : CreateExpenseCommand, ILoggable, ICommand
    {
        public long Id { get; set; }

        public DateTime LastModified { get; set; }

        public override void EmitLog(LogEventDelegate log, bool success)
        {
            if (success)
                log(Events.CategoryCommands, "{Account} updated expense {ExpenseId}", this.AccountName, this.Id);
            else
                log(Events.CategoryCommands, "Updating expense {ExpenseId} for {Account} failed", this.Id, this.AccountName);
        }
    }

    public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand, bool>
    {
        private readonly GeldAppContext db;
        private readonly IValidator<Database.Expense> expenseValidator;

        public UpdateExpenseCommandHandler(
            GeldAppContext db,
            IValidator<Database.Expense> expenseValidator)
        {
            this.db = db;
            this.expenseValidator = expenseValidator;
        }

        public async Task<bool> Handle(UpdateExpenseCommand cmd, CancellationToken cancellationToken)
        {
            var expense = await this.LoadExpense(cmd.Id);

            this.UpdateProperties(cmd, expense);

            this.expenseValidator.Validate(expense)
                                 .ThrowOnError("Invalid expense");

            await this.db.SaveChangesAsync();

            return true;
        }

        private async Task<Database.Expense> LoadExpense(long expenseId)
        {
            if (expenseId <= 0)
                throw new ArgumentException("Invalid Id");

            var expense = await this.db.Expenses
                .Include(ex => ex.Account)
                .SingleOrDefaultAsync(xp => xp.Id == expenseId);

            if (expense == null)
                throw new ArgumentException($"Id not found: {expenseId}");
            return expense;
        }

        private void UpdateProperties(UpdateExpenseCommand cmd, Database.Expense expense)
        {
            expense.AccountId = cmd.Account.Id;
            expense.Amount = cmd.Amount;
            expense.Category = cmd.CategoryName;
            expense.Subcategory = cmd.SubcategoryName;
            expense.Date = cmd.Date;
            expense.Details = cmd.Details;
            expense.Type = cmd.Type;

            var modified = (DateTimeOffset)cmd.LastModified;
            expense.LastModified = modified.LocalDateTime;
            expense.LastModifiedBy = cmd.UserName;
        }
    }
}

using FluentValidation;
using GeldApp2.Application.Logging;
using GeldApp2.Database;
using GeldApp2.Extensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Expense
{
    public class CreateExpenseCommand : AccountRelatedRequest<bool>, ILoggable, ICommand
    {
        public decimal Amount { get; set; }

        public string CategoryName { get; set; }

        public string SubcategoryName { get; set; }
        
        public string Details { get; set; }

        public DateTime Date { get; set; }

        public DateTime Created { get; set; }

        public ExpenseType Type { get; set; }

        public string UserName { get; set; }

        public virtual void EmitLog(LogEventDelegate log, bool success)
        {
            if (success)
                log(Events.ExpenseCommands, "{Account} created a new {ExpenseType}", this.AccountName, this.Type.ToString());
            else
                log(Events.ExpenseCommands, "Creating a new {ExpenseType} for {Account} failed", this.Type.ToString(), this.AccountName);
        }
    }

    public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, bool>
    {
        private readonly GeldAppContext db;
        private readonly IValidator<Database.Expense> expenseValidator;

        public CreateExpenseCommandHandler(
            GeldAppContext db,
            IValidator<Database.Expense> expenseValidator)
        {
            this.db = db;
            this.expenseValidator = expenseValidator;
        }

        public async Task<bool> Handle(CreateExpenseCommand cmd, CancellationToken cancellationToken)
        {
            var createdUtc = (DateTimeOffset)cmd.Created;

            var expense = new Database.Expense
            {
                Id = 0,
                AccountId = cmd.Account.Id,
                Category = cmd.CategoryName,
                Subcategory = cmd.SubcategoryName,
                Amount = cmd.Amount,
                Date = cmd.Date,
                Details = cmd.Details,
                Type = cmd.Type,
                Created = createdUtc.LocalDateTime,
                CreatedBy = cmd.UserName,
                LastModified = createdUtc.LocalDateTime,
                LastModifiedBy = cmd.UserName
            };

            this.expenseValidator.Validate(expense)
                                 .ThrowOnError("Invalid expense");

            this.db.Expenses.Add(expense);
            await this.db.SaveChangesAsync();

            return true;
        }
    }
}

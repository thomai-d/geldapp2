using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Logging;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Import
{
    public class LinkImportedExpenseCommand : AccountRelatedRequest<bool>, ICommand, ILoggable
    {
        public LinkImportedExpenseCommand(string accountName, long importedExpenseId, long relatedExpenseId) : base(accountName)
        {
            this.ImportedExpenseId = importedExpenseId;
            this.RelatedExpenseId = relatedExpenseId;
        }

        public long ImportedExpenseId { get; private set; }

        public long RelatedExpenseId { get; private set; }
        
        public void EmitLog(LogEventDelegate log, bool success)
        {
            if (success)
                log(Events.HandleImportedExpenseCommand, "{Account} linked an imported expense", this.AccountName);
            else
                log(Events.HandleImportedExpenseCommand, "Linking an imported expense for {Account} failed", this.AccountName);
        }
    }

    public class LinkImportedExpenseCommandHandler : IRequestHandler<LinkImportedExpenseCommand, bool>
    {
        private readonly GeldAppContext db;

        public LinkImportedExpenseCommandHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<bool> Handle(LinkImportedExpenseCommand request, CancellationToken cancellationToken)
        {
            var importedExpense = await this.db.ImportedExpenses
                                         .SingleOrDefaultAsync(ie => ie.AccountId == request.Account.Id
                                                             && ie.Id == request.ImportedExpenseId);
            if (importedExpense == null)
                throw new NotFoundException($"ImportedExpense {request.ImportedExpenseId}");

            var linkedExpense = await this.db.Expenses
                                       .SingleOrDefaultAsync(ex => ex.AccountId == request.Account.Id
                                                              && ex.Id == request.RelatedExpenseId);
            if (linkedExpense == null)
                throw new NotFoundException($"Expense {request.RelatedExpenseId}");

            importedExpense.ExpenseId = linkedExpense.Id;
            importedExpense.IsHandled = true;
            this.db.ImportedExpenses.Update(importedExpense);

            await this.db.SaveChangesAsync();
            return true;
        }
    }
}

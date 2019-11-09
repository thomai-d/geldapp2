using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Logging;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Import
{
    public class HandleImportedExpenseCommand : AccountRelatedRequest<bool>, ICommand, ILoggable
    {
        public HandleImportedExpenseCommand(string accountName, long importedExpenseId)
            : base(accountName)
        {
            this.ImportedExpenseId = importedExpenseId;
        }
        
        public long ImportedExpenseId { get; private set; }
        
        public void EmitLog(LogEventDelegate log, bool success)
        {
            if (success)
                log(Events.HandleImportedExpenseCommand, "{Account} handled an imported expense", this.AccountName);
            else
                log(Events.HandleImportedExpenseCommand, "Handling an imported expense for {Account} failed", this.AccountName);
        }
    }

    public class HandleImportedExpenseCommandHandler : IRequestHandler<HandleImportedExpenseCommand, bool>
    {
        private readonly GeldAppContext db;

        public HandleImportedExpenseCommandHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<bool> Handle(HandleImportedExpenseCommand request, CancellationToken cancellationToken)
        {
            var importedExpense = await this.db.ImportedExpenses
                                         .SingleOrDefaultAsync(ie => ie.AccountId == request.Account.Id
                                                             && ie.Id == request.ImportedExpenseId);
            if (importedExpense == null)
                throw new NotFoundException($"ImportedExpense {request.ImportedExpenseId}");

            importedExpense.IsHandled = true;
            await this.db.SaveChangesAsync();
            return true;
        }
    }
}

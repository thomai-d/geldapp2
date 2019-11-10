using GeldApp2.Application.Commands;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries.Import
{
    public class GetUnhandledImportedExpensesQuery : AccountRelatedRequest<ImportedExpense[]>
    {
        public GetUnhandledImportedExpensesQuery(string accountName)
            : base(accountName)
        {
        }
    }

    public class GetUnhandledImportedExpensesQueryHandler : IRequestHandler<GetUnhandledImportedExpensesQuery, ImportedExpense[]>
    {
        private readonly GeldAppContext db;

        public GetUnhandledImportedExpensesQueryHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<ImportedExpense[]> Handle(GetUnhandledImportedExpensesQuery request, CancellationToken cancellationToken)
        {
            return await this.db.ImportedExpenses
                .Where(ex => ex.AccountId == request.Account.Id
                          && !ex.IsHandled)
                .OrderByDescending(ex => ex.BookingDay)
                .ToArrayAsync();
        }
    }
}

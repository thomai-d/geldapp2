using GeldApp2.Application.Commands;
using GeldApp2.Application.Queries.Expense.Filter;
using GeldApp2.Database;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries
{
    public class GetExpensesQuery : AccountRelatedRequest<ExpenseViewModel[]>
    {
        public GetExpensesQuery(string accountName)
            : base(accountName)
        {
        }

        public string SearchText { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }

        public bool IncludeFuture { get; set; }
    }

    public class GetExpensesQueryHandler
        : IRequestHandler<GetExpensesQuery, ExpenseViewModel[]>
    {
        public readonly CultureInfo SearchCulture = new CultureInfo("de");

        private readonly GeldAppContext db;

        public GetExpensesQueryHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<ExpenseViewModel[]> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
        {
            var account = await this.db.Accounts
                    .SingleOrDefaultAsync(acct => acct.Id == request.Account.Id);

            if (account == null)
                return new ExpenseViewModel[0];

            var query = this.db.Expenses
                .Include(ex => ex.Account)
                .OrderByDescending(ex => ex.Date)
                    .ThenByDescending(ex => ex.LastModified)
                .Where(ex => ex.AccountId == account.Id);

            if (!request.IncludeFuture)
            {
                query = query.Where(e => e.Date.Date <= DateTime.Now.Date);
            }

            if (!string.IsNullOrEmpty(request.SearchText))
            {
                if (request.SearchText.StartsWith("!"))
                {
                    var filter = ExpenseFilterString.Parse(request.SearchText.Substring(1));
                    if (filter.Month.HasValue)
                        query = query.Where(ex => ex.Date.Month == filter.Month.Value);
                    if (filter.Year.HasValue)
                        query = query.Where(ex => ex.Date.Year == filter.Year.Value);
                    if (!string.IsNullOrEmpty(filter.Category))
                        query = query.Where(ex => ex.Category.Equals(filter.Category));
                    if (!string.IsNullOrEmpty(filter.Subcategory))
                        query = query.Where(ex => ex.Subcategory.Equals(filter.Subcategory));
                }
                else
                {
                    var amountSearchText = request.SearchText;
                    if (double.TryParse(amountSearchText, NumberStyles.AllowDecimalPoint, SearchCulture, out var amount))
                    {
                        // Translate the amount's search culture into local culture (used for differing decimal points).
                        amountSearchText = amount.ToString("0.00");
                    }

                    query = query.Where(ex => ex.Category.Contains(request.SearchText)
                                            || ex.Subcategory.Contains(request.SearchText)
                                            || ex.Details.Contains(request.SearchText)
                                            || ex.Amount.ToString().Contains(amountSearchText));
                }
            }

            var expenses = await query
                .Skip(request.Offset)
                .Take(request.Limit)
                .ToListAsync();

            return expenses
                .Select(ExpenseViewModel.FromDb)
                .ToArray();
        }
    }
}

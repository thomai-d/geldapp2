using GeldApp2.Application.Commands;
using GeldApp2.Database;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries.Category
{
    public class GetCategoriesForAccountQuery : AccountRelatedRequest<CategoryViewModel[]>
    {
        public GetCategoriesForAccountQuery(string accountName)
            : base(accountName) { }
    }

    public class GetCategoriesForAccountQueryHandler : IRequestHandler<GetCategoriesForAccountQuery, CategoryViewModel[]>
    {
        private readonly GeldAppContext db;

        public GetCategoriesForAccountQueryHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<CategoryViewModel[]> Handle(GetCategoriesForAccountQuery request, CancellationToken cancellationToken)
        {
            var categories = await this.db.Categories
                    .AsNoTracking()
                    .Include(i => i.Subcategories)
                    .Where(acct => acct.AccountId == request.Account.Id)
                    .OrderBy(i => i.Name)
                    .ToArrayAsync();

            foreach (var cat in categories)
                cat.SortSubcategories();

            return categories
                .Select(CategoryViewModel.FromDb)
                .ToArray();
        }
    }
}

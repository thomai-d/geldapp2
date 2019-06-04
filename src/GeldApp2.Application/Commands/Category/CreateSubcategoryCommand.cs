using GeldApp2.Application.Exceptions;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Category
{
    public class CreateSubcategoryCommand : AccountRelatedRequest<bool>
    {
        public CreateSubcategoryCommand()
        {
        }

        public CreateSubcategoryCommand(string accountName, string categoryName, string subcategoryName)
        {
            this.AccountName = accountName;
            this.CategoryName = categoryName;
            this.SubcategoryName = subcategoryName;
        }

        public string CategoryName { get; set; }

        public string SubcategoryName { get; set; }
    }

    public class CreateSubcategoryCommandHandler : IRequestHandler<CreateSubcategoryCommand, bool>
    {
        private readonly GeldAppContext db;

        public CreateSubcategoryCommandHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<bool> Handle(CreateSubcategoryCommand cmd, CancellationToken cancellationToken)
        {
            var category = await this.db.Categories
                .Include(cat => cat.Subcategories)
                .SingleOrDefaultAsync(cat => cat.Name == cmd.CategoryName && cat.AccountId == cmd.Account.Id)
                ?? throw new UserException("Invalid category name");

            if (category.Subcategories.Any(subcategory => string.Compare(subcategory.Name, cmd.SubcategoryName, ignoreCase: true) == 0))
                throw new UserException($"Subcategory {cmd.SubcategoryName} already exists");

            category.Subcategories.Add(new Subcategory(cmd.SubcategoryName));
            await this.db.SaveChangesAsync();

            return true;
        }
    }
}

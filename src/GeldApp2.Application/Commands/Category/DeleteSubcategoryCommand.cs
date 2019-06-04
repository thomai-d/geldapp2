using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands
{
    public class DeleteSubcategoryCommand : AccountRelatedRequest<bool>
    {
        public DeleteSubcategoryCommand()
        {
        }

        public DeleteSubcategoryCommand(string accountName, string categoryName, string subcategoryName)
        {
            this.AccountName = accountName;
            this.CategoryName = categoryName;
            this.SubcategoryName = subcategoryName;
        }

        public string CategoryName { get; set; }

        public string SubcategoryName { get; set; }
    }

    public class DeleteSubcategoryCommandHandler : IRequestHandler<DeleteSubcategoryCommand, bool>
    {
        private readonly GeldAppContext db;

        public DeleteSubcategoryCommandHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<bool> Handle(DeleteSubcategoryCommand cmd, CancellationToken cancellationToken)
        {
            var category = await this.db.Categories.SingleOrDefaultAsync(c => c.AccountId == cmd.Account.Id
                                                                        && c.Name.ToLower() == cmd.CategoryName.ToLower());
            if (category == null)
                throw new ArgumentException($"Category {cmd.CategoryName} does not exist.");

            var subcategory = await this.db.Subcategories.SingleOrDefaultAsync(c => c.CategoryId == category.Id
                                                                        && c.Name.ToLower() == cmd.SubcategoryName.ToLower());
            if (subcategory == null)
                throw new ArgumentException($"Subcategory {cmd.SubcategoryName} does not exist.");

            this.db.Remove(subcategory);
            await this.db.SaveChangesAsync();

            return true;
        }
    }
}

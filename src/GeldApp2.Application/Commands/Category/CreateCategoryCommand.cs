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
    public class CreateCategoryCommand : AccountRelatedRequest<bool>
    {
        public CreateCategoryCommand()
        {
        }

        public CreateCategoryCommand(string accountName, string categoryName)
        {
            this.AccountName = accountName;
            this.CategoryName = categoryName;
        }

        public string CategoryName { get; set; }
    }

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, bool>
    {
        private readonly GeldAppContext db;

        public CreateCategoryCommandHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<bool> Handle(CreateCategoryCommand cmd, CancellationToken cancellationToken)
        {
            var account = await this.db.Accounts
                                        .Include(a => a.Categories)
                                        .SingleAsync(a => a.Id == cmd.Account.Id);

            if (account.Categories.Any(cat => cat.Name.ToLower() == cmd.CategoryName.ToLower()))
                throw new UserException($"Category {cmd.CategoryName} already exists");

            account.Categories.Add(new Database.Category(cmd.CategoryName));
            await this.db.SaveChangesAsync();

            return true;
        }
    }
}

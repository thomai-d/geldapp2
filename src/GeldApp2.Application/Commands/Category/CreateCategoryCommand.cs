using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Logging;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Category
{
    public class CreateCategoryCommand : AccountRelatedRequest<bool>, ILoggable
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

        public void EmitLog(LogEventDelegate log, bool success)
        {
            if (success)
                log(Events.CategoryCommands, "{Account} created a new category {Category}", this.AccountName, this.CategoryName);
            else
                log(Events.CategoryCommands, "Creating a new category {Category} for {Account} failed", this.CategoryName, this.AccountName);
        }
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

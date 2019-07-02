using GeldApp2.Application.Commands;
using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Logging;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Category
{
    public class DeleteCategoryCommand : AccountRelatedRequest<bool>, ILoggable
    {
        public string CategoryName { get; set; }

        public void EmitLog(LogEventDelegate log, bool success)
        {
            if (success)
                log(Events.CategoryCommands, "{Account} deleted the category {Category}", this.AccountName, this.CategoryName);
            else
                log(Events.CategoryCommands, "Deleting the category {Category} for {Account} failed", this.CategoryName, this.AccountName);
        }
    }

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly GeldAppContext db;

        public DeleteCategoryCommandHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<bool> Handle(DeleteCategoryCommand cmd, CancellationToken cancellationToken)
        {
            var category = await this.db.Categories.SingleOrDefaultAsync(c => c.AccountId == cmd.Account.Id
                                                                        && c.Name.ToLower() == cmd.CategoryName.ToLower());

            if (category == null)
                throw new UserException($"Category {cmd.CategoryName} does not exist.");

            this.db.Remove(category);
            await this.db.SaveChangesAsync();

            return true;
        }
    }
}

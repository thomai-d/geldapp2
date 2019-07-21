using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GeldApp2.Application.Commands;
using GeldApp2.Application.Commands.Expense;
using GeldApp2.Application.Queries;
using GeldApp2.Database;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly User currentUser;

        public ExpenseController(IMediator mediator, User currentUser)
        {
            this.mediator = mediator;
            this.currentUser = currentUser;
        }

        [HttpPost("/api/account/{accountName}/expenses")]
        public async Task Create(string accountName, [FromBody]CreateExpenseCommand cmd)
        {
            accountName = Uri.UnescapeDataString(accountName);
            cmd.AccountName = accountName;
            cmd.UserName = this.currentUser.Name;
            await this.mediator.Send(cmd);
        }

        [HttpPut("/api/account/{accountName}/expense/{id}")]
        public async Task Update(string accountName, long id, [FromBody]UpdateExpenseCommand cmd)
        {
            accountName = Uri.UnescapeDataString(accountName);
            cmd.AccountName = accountName;
            cmd.UserName = this.currentUser.Name;
            cmd.Id = id;
            await this.mediator.Send(cmd);
        }

        [HttpDelete("/api/account/{accountName}/expense/{id}")]
        public async Task DeleteExpense(string accountName, long id)
        {
            accountName = Uri.UnescapeDataString(accountName);
            var cmd = new DeleteExpenseCommand
            {
                AccountName = accountName,
                Id = id
            };

            await this.mediator.Send(cmd);
        }

        [HttpGet("/api/account/{accountName}/expenses")]
        public async Task<ActionResult<ExpenseViewModel[]>> GetExpenses(
            string accountName,
            [FromQuery]string q = "",
            [FromQuery]int limit = 20,
            [FromQuery]int offset = 0,
            [FromQuery]bool includeFuture = false)
        {
            accountName = Uri.UnescapeDataString(accountName);

            var cmd = new GetExpensesQuery(accountName)
            {
                Limit = limit,
                Offset = offset,
                IncludeFuture = includeFuture,
                SearchText = q
            };

            return await this.mediator.Send(cmd);
        }

        [HttpGet("/api/account/{accountName}/expense/{id}")]
        public async Task<ExpenseViewModel> GetExpense(string accountName, long id)
        { 
            accountName = Uri.UnescapeDataString(accountName);
            return await this.mediator.Send(new GetExpenseByIdQuery(accountName, id));
        }
    }
}

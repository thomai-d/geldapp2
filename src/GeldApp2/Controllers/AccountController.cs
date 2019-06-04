using System;
using System.Threading.Tasks;
using GeldApp2.Application.Queries.Account;
using GeldApp2.Database;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly User currentUser;
        private readonly IMediator mediator;

        public AccountController(
            User currentUser,
            IMediator mediator)
        {
            this.currentUser = currentUser;
            this.mediator = mediator;
        }

        /// <summary>
        /// Returns a summary for every account.
        /// </summary>
        [HttpGet, Route("/api/accounts/summary/month")]
        public async Task<AccountSummary[]> GetAccountSummaryMonth()
        {
            var now = DateTime.Now;
            return await this.mediator.Send(new GetAccountSummariesQuery(this.currentUser, now.Month, now.Year));
        }
    }
}

using GeldApp2.Application.Queries.Users;
using GeldApp2.Database;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class UserController
    {
        private readonly User currentUser;
        private readonly IMediator mediator;

        public UserController(
            User currentUser,
            IMediator mediator)
        {
            this.currentUser = currentUser;
            this.mediator = mediator;
        }

        [Authorize(Roles = "admin")]
        [HttpGet, Route("/api/users")]
        public async Task<UserSummary[]> GetAccountSummaryMonth()
        {
            return await this.mediator.Send(new GetUserSummaryQuery());
        }
    }
}

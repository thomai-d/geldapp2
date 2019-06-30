using GeldApp2.Application.Commands.Users;
using GeldApp2.Database;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly User currentUser;

        public AuthenticationController(IMediator mediator, User currentUser)
        {
            this.mediator = mediator;
            this.currentUser = currentUser;
        }

        [HttpPost("/api/auth/changePassword")]
        public async Task ChangePasswordAsync([FromBody]ChangePasswordCommand cmd)
        {
            cmd.User = this.currentUser;
            await this.mediator.Send(cmd);
        }

        [HttpGet("/api/auth/refresh")]
        public async Task<IActionResult> GetRefreshToken([FromQuery]string token)
        {
            var cmd = new RefreshTokenCommand(this.currentUser, token);
            var result = await this.mediator.Send(cmd);
            return this.Ok(new { token = result });
        }
    }
}

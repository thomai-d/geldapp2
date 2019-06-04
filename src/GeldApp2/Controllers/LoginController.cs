using GeldApp2.Application.Commands.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IMediator mediator;

        public LoginController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("/api/auth/login")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginCommand cmd)
        {
            var result = await this.mediator.Send(cmd);
            return this.Ok(result);
        }
    }
}

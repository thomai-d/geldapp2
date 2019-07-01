using GeldApp2.Application.Logging;
using GeldApp2.Application.Services;
using GeldApp2.Database;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Users
{
    /// <summary>
    /// Command that checks the login and returns a JWT token.
    /// </summary>
    public class LoginCommand : IRequest<LoginResult>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResult
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly GeldAppContext db;
        private readonly IJwtTokenService jwtTokenService;
        private readonly ILogger<LoginCommandHandler> log;

        public LoginCommandHandler(GeldAppContext db, IJwtTokenService jwtTokenService, ILogger<LoginCommandHandler> log)
        {
            this.db = db;
            this.jwtTokenService = jwtTokenService;
            this.log = log;
        }
        public async Task<LoginResult> Handle(LoginCommand cmd, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(cmd.Username) || string.IsNullOrEmpty(cmd.Password))
                throw new AuthenticationException("Empty user or password");

            var tempUser = new User(cmd.Username, cmd.Password);
            var user = this.db.Users.SingleOrDefault(u => u.Name == tempUser.Name
                                                       && u.Password == tempUser.Password);
            if (user == null)
                throw new AuthenticationException("User not found or passord wrong.");

            user.LastLogin = DateTimeOffset.Now;
            await this.db.SaveChangesAsync();

            var token = await this.jwtTokenService.GetTokenStringAsync(user);
            var tokenPart = $"{token.Substring(0, 6)}...{token.Substring(token.Length - 6)}";
            this.log.LogInformation(Events.Login, "{user} logged in. Token = {token}", user.Name, tokenPart);
            return new LoginResult
            {
                Token = token,
                RefreshToken = user.RefreshToken
            };
        }
    }
}

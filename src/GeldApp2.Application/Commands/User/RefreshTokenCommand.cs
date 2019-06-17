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

namespace GeldApp2.Application.Commands.User
{
    /// <summary>
    /// Command returns a fresh jwt token.
    /// </summary>
    public class RefreshTokenCommand : IRequest<string>
    {
        public RefreshTokenCommand(Database.User user, string refreshToken)
        {
            this.User = user;
            this.RefreshToken = refreshToken;
        }

        public Database.User User { get; }

        public string RefreshToken { get; set; }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, string>
    {
        private readonly GeldAppContext db;
        private readonly IJwtTokenService jwtTokenService;
        private readonly ILogger<RefreshTokenCommandHandler> log;

        public RefreshTokenCommandHandler(GeldAppContext db, IJwtTokenService jwtTokenService, ILogger<RefreshTokenCommandHandler> log)
        {
            this.db = db;
            this.log = log;
            this.jwtTokenService = jwtTokenService;
        }
        public async Task<string> Handle(RefreshTokenCommand cmd, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(cmd.RefreshToken) || cmd.User.Id <= 0)
                throw new AuthenticationException("Empty token or invalid user id");

            var user = this.db.Users.SingleOrDefault(u => u.Id == cmd.User.Id
                                                       && u.RefreshToken == cmd.RefreshToken);
            if (user == null)
                throw new AuthenticationException("User not found or invalid token.");

            user.LastLogin = DateTimeOffset.Now;
            await this.db.SaveChangesAsync();

            var token = await this.jwtTokenService.GetTokenStringAsync(user);
            var tokenPart = $"{token.Substring(0, 6)}...{token.Substring(token.Length - 6)}";
            this.log.LogInformation(Events.RefreshToken, "{user} refreshed token. Token = {token}", user.Name, tokenPart);
            return token;
        }
    }
}

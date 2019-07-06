using GeldApp2.Application.Commands;
using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Logging;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Users
{
    public class ChangePasswordCommand : IRequest<bool>, ILoggable, ICommand
    {
        public Database.User User { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public void EmitLog(LogEventDelegate log, bool success)
        {
            if (success)
                log(Events.UserCommands, "{Username} changed the password", this.User.Name);
            else
                log(Events.UserCommands, "Changing the password for {Username} failed", this.User.Name);
        }
    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        public const int MinPasswordLength = 5;

        private readonly GeldAppContext db;

        public ChangePasswordCommandHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (!request.User.HasPassword(request.OldPassword))
                throw new UnauthorizedException("Wrong old password");

            if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < MinPasswordLength)
                throw new UserException("Password does not satisfy server requirements");

            var user = await this.db.Users.SingleAsync(u => u.Id == request.User.Id);
            user.SetPassword(request.NewPassword);
            user.GenerateRefreshToken();
            await this.db.SaveChangesAsync();

            return true;
        }

    }
}

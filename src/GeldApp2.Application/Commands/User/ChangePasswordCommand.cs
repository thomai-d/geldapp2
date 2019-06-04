using GeldApp2.Application.Commands;
using GeldApp2.Application.Exceptions;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.User
{
    public class ChangePasswordCommand : IRequest<bool>
    {
        public Database.User User { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
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
                throw new AuthenticationException("Wrong old password");

            if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < MinPasswordLength)
                throw new UserException("Password does not satisfy server requirements");

            var user = await this.db.Users.SingleAsync(u => u.Id == request.User.Id);
            user.SetPassword(request.NewPassword);
            user.RefreshToken = this.GenerateRefreshToken();
            await this.db.SaveChangesAsync();

            return true;
        }

        private string GenerateRefreshToken()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[16];
                provider.GetBytes(bytes);
                return new Guid(bytes).ToString();
            }
        }
    }
}

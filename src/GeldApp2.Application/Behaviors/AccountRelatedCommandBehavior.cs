using GeldApp2.Application.Commands;
using GeldApp2.Database;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Behaviors
{
    /// <summary>
    /// Fills the Account propierty of an <see cref="AccountRelatedRequest"/> - Command.
    /// </summary>
    public class AccountRelatedCommandBehavior<TReq, TResp> : IPipelineBehavior<TReq, TResp>
        where TReq: AccountRelatedRequest<TResp>
    {
        private readonly User currentUser;

        public AccountRelatedCommandBehavior(User currentUser)
        {
            this.currentUser = currentUser;
        }

        public async Task<TResp> Handle(TReq request, CancellationToken cancellationToken, RequestHandlerDelegate<TResp> next)
        {
            if (string.IsNullOrEmpty(request.AccountName))
                throw new ArgumentException("Account name not set", nameof(request));

            request.Account = this.currentUser.GetAccount(request.AccountName);

            return await next();
        }
    }
}

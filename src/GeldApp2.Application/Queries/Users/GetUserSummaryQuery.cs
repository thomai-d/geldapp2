using GeldApp2.Database;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries.Users
{
    public class GetUserSummaryQuery : IRequest<UserSummary[]>
    {
    }

    public class GetUserSummaryQueryHandler : IRequestHandler<GetUserSummaryQuery, UserSummary[]>
    {
        private readonly GeldAppContext db;

        public GetUserSummaryQueryHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<UserSummary[]> Handle(GetUserSummaryQuery request, CancellationToken cancellationToken)
        {
            var users = await this.db.Users
                .AsNoTracking()
                .Include(u => u.UserAccounts)
                    .ThenInclude(ua => ua.Account)
                .ToArrayAsync();

            return users.Select(UserSummary.FromUser)
                        .ToArray();
        }
    }
}

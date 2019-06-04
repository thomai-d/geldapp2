using GeldApp2.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace GeldApp2.Middleware
{
    public class UserPreloadMiddleware
    {
        private readonly RequestDelegate next;

        public UserPreloadMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, GeldAppContext db)
        {
            await this.PreloadUserAsync(context, db);

            await this.next(context);
        }

        private async Task PreloadUserAsync(HttpContext context, GeldAppContext db)
        {
            var userIdClaim = context.User.FindFirst("userid")?.Value;
            if (userIdClaim == null)
                return;

            if (!long.TryParse(userIdClaim, out var userid) || userid <= 0)
                throw new AuthenticationException($"Invalid claim: '{userIdClaim}'");

            var user = await db.Users
                        .AsNoTracking()
                        .Include(i => i.UserAccounts)
                            .ThenInclude(ua => ua.Account)
                        .SingleOrDefaultAsync(u => u.Id == userid);

            if (user == null)
                throw new AuthenticationException($"Unknown userid: {userid}");

            context.Items.Add("currentUser", user);
        }
    }
}

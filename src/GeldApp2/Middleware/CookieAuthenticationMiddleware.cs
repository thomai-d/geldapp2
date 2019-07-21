using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Middleware
{
    /// <summary>
    /// Forwards the 'X-Authentication-Bearer' header to the context's header.
    /// This is a workaround for file downloads where the client app temporarily
    /// sets the bearer token as a cookie while requesting the download.
    /// (Downloads with JWT Token are not possible)
    /// </summary>
    public class CookieAuthenticationMiddleware
    {
        private readonly RequestDelegate next;

        public CookieAuthenticationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cookies = context.Request.Cookies;
            if (cookies.TryGetValue("X-Authorization-Bearer", out var authToken))
            {
                if (context.Request.Headers.ContainsKey("Authorization"))
                    throw new UnauthorizedAccessException("Authorization-Header and X-Authorization-Bearer Cookie set simultaneously");

                context.Request.Headers.Add("Authorization", $"Bearer {authToken}");
            }

            await this.next(context);
        }
    }
}

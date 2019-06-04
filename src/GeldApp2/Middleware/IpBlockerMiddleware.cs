using Abstrakt.AspNetCore.Extensions;
using FluentValidation;
using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Logging;
using GeldApp2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace GeldApp2.Middleware
{
    public class IpBlockerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IIpBlockerService ipBlockerService;

        public IpBlockerMiddleware(RequestDelegate next, IIpBlockerService ipBlockerService)
        {
            this.next = next;
            this.ipBlockerService = ipBlockerService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (this.ipBlockerService.IsBlocked(context.GetRemoteIp()))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return;
            }

            await this.next(context);
        }
    }
}

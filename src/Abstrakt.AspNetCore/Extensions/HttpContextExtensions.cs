﻿using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Abstrakt.AspNetCore.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetHttpHeader(this HttpContext ctx, string key)
        {
            if (ctx.Request.Headers.TryGetValue(key, out var value))
                return value;
            else
                return string.Empty;
        }

        public static string GetRemoteIp(this HttpContext ctx)
        {
            if (ctx.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
                return realIp.First().Trim(new[] { ':', ' ' });

            if (Runtime.IsIntegrationTesting)
                return "127.0.0.1";

            return ctx.Connection.RemoteIpAddress.ToString();
        }
    }
}

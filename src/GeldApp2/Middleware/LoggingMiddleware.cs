using Abstrakt.AspNetCore.Extensions;
using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Logging;
using GeldApp2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace GeldApp2.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<LoggingMiddleware> log;
        private readonly IIpBlockerService ipBlockerService;

        public LoggingMiddleware(
            RequestDelegate next,
            ILogger<LoggingMiddleware> log,
            IIpBlockerService ipBlockerService)
        {
            this.next = next;
            this.log = log;
            this.ipBlockerService = ipBlockerService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (LogContext.Push(new IpEnricher(context)))
            {
                var watch = Stopwatch.StartNew();

                try
                {
                    await this.next(context);
                    watch.Stop();
                    this.log.LogInformation(
                        Events.HandleRequestSuccess,
                        "{Method} {RequestPath} took {ElapsedMilliseconds}ms",
                        context.Request.Method,
                        context.Request.Path,
                        watch.ElapsedMilliseconds);
                }
                catch (ValidationException ex)
                {
                    this.LogError(context, watch, ex);
                    context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(ex.Message));
                }
                catch (NotFoundException ex)
                {
                    this.LogNotFound(context, watch, ex);
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(ex.Message));
                }
                catch (UserException ex)
                {
                    this.LogUserWarn(context, watch, ex);
                    context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(ex.Message));
                }
                catch (AuthenticationException ex)
                {
                    this.LogError(context, watch, ex);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    this.ipBlockerService.CountViolation(context.GetRemoteIp());
                }
                catch (Exception ex)
                {
                    this.LogError(context, watch, ex);
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
            }
        }

        private void LogNotFound(HttpContext context, Stopwatch watch, NotFoundException ex)
        {
            watch.Stop();
            this.log.LogWarning(Events.HandleRequestUserFailed,
                "{Method} {RequestPath} failed after {ElapsedMilliseconds}ms with 404: '{Entity}' could not be found.",
                context.Request.Method, context.Request.Path,
                watch.ElapsedMilliseconds, ex.Entity);
        }

        private void LogUserWarn(HttpContext context, Stopwatch watch, Exception ex)
        {
            watch.Stop();
            this.log.LogWarning(Events.HandleRequestUserFailed,
                "{Method} {RequestPath} failed after {ElapsedMilliseconds}ms with user error: {UserError} ({ExceptionType})",
                context.Request.Method, context.Request.Path,
                watch.ElapsedMilliseconds, ex.Message, ex.GetType().Name);
        }

        private void LogError(HttpContext context, Stopwatch watch, Exception ex)
        {
            watch.Stop();
            this.log.LogError(Events.HandleRequestFailed, ex,
                "{Method} {RequestPath} failed after {ElapsedMilliseconds}ms with {ExceptionType}",
                context.Request.Method, context.Request.Path,
                watch.ElapsedMilliseconds, ex.GetType().Name);
        }
    }

    public class IpEnricher : ILogEventEnricher
    {
        private readonly HttpContext ctx;

        public IpEnricher(HttpContext ctx)
        {
            this.ctx = ctx;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("X-Real-IP", this.ctx.GetRemoteIp()));
        }
    }
}

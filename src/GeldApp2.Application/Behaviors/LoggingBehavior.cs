using GeldApp2.Application.Commands;
using GeldApp2.Application.Logging;
using GeldApp2.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Behaviors
{
    public class LoggingBehavior<TReq, TResp> : IPipelineBehavior<TReq, TResp>
    {
        private readonly ILogger<LoggingBehavior<TReq, TResp>> log;
        private readonly ILogContextEnricher logEnricher;

        public LoggingBehavior(
            ILogger<LoggingBehavior<TReq, TResp>> log,
            ILogContextEnricher logEnricher)
        {
            this.log = log;
            this.logEnricher = logEnricher;
        }


        public async Task<TResp> Handle(TReq request, CancellationToken cancellationToken, RequestHandlerDelegate<TResp> next)
        {
            var success = false;

            using (this.logEnricher.PushProperty("Request", typeof(TReq).Name, false))
            {
                try
                {
                    var result = await next();
                    success = true;
                    return result;
                }
                finally
                {
                    if (request is ILoggable loggable)
                    {
                        if (success)
                            loggable.EmitLog((id, format, args) => this.log.LogInformation(id, format, args), true);
                        else
                            loggable.EmitLog((id, format, args) => this.log.LogError(id, format, args), true);
                    }
                }
            }
        }
    }
}

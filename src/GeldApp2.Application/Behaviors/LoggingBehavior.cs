using GeldApp2.Application.Logging;
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

        public LoggingBehavior(ILogger<LoggingBehavior<TReq, TResp>> log)
        {
            this.log = log;
        }


        public async Task<TResp> Handle(TReq request, CancellationToken cancellationToken, RequestHandlerDelegate<TResp> next)
        {
            this.log.LogInformation(Events.HandleRequest, "Handling {RequestType}", request.GetType().Name);
            return await next();
        }
    }
}

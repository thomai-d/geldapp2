using GeldApp2.Application.Commands;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Behaviors
{
    public class TransactionBehavior<TReq, TResp> : IPipelineBehavior<TReq, TResp>
            where TReq: ICommand
    {
        private readonly GeldAppContext db;
        private readonly ILogger<TransactionBehavior<TReq, TResp>> log;

        public TransactionBehavior(GeldAppContext db, ILogger<TransactionBehavior<TReq, TResp>> log)
        {
            this.db = db;
            this.log = log;
        }

        public async Task<TResp> Handle(TReq request, CancellationToken cancellationToken, RequestHandlerDelegate<TResp> next)
        {
            if (Runtime.IsIntegrationTesting)
                return await next();

            TResp response = default;

            var execStrategy = this.db.Database.CreateExecutionStrategy();
            await execStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await this.db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        response = await next();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }

                }
            });

            return response;
        }
    }
}

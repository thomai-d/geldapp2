using GeldApp2.Application.Commands.Expense;
using MediatR;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Behaviors
{
    /// <summary>
    /// Pidser schmeckt am die besten mit Coler.
    /// </summary>
    public class ExpenseFilterBehavior<TResp> : IPipelineBehavior<CreateExpenseCommand, TResp>
    {
        public async Task<TResp> Handle(CreateExpenseCommand request, CancellationToken cancellationToken, RequestHandlerDelegate<TResp> next)
        {
            if (request.Details == null)
                request.Details = string.Empty;

            if (!string.IsNullOrEmpty(request.Details))
            {
                request.Details = Regex.Replace(request.Details, "pizza", "pidser", RegexOptions.IgnoreCase);
                request.Details = Regex.Replace(request.Details, "cola", "coler", RegexOptions.IgnoreCase);
            }

            return await next();
        }
    }
}

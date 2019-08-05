using GeldApp2.Application.ML;
using GeldApp2.Application.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Category
{
    public class PredictCategoryCommand : AccountRelatedRequest<CategoryPredictionResult>, ICommand
    {
        public PredictCategoryCommand(string accountName, float amount, DateTime created, DateTime expenseDate)
            : base(accountName)
        {
            this.Amount = amount;
            this.Created = created;
            this.ExpenseDate = expenseDate;
        }

        public float Amount { get; set; }

        public DateTime Created { get; set; }

        public DateTime ExpenseDate { get; set; }
    }

    public class PredictCategoryCommandHandler : IRequestHandler<PredictCategoryCommand, CategoryPredictionResult>
    {
        private readonly ICategoryPredictionService expensePredictionService;

        public PredictCategoryCommandHandler(ICategoryPredictionService expensePredictionService)
        {
            this.expensePredictionService = expensePredictionService;
        }

        public Task<CategoryPredictionResult> Handle(PredictCategoryCommand cmd, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.expensePredictionService.Predict(cmd.AccountName, cmd.Amount, cmd.Created, cmd.ExpenseDate));
        }
    }
}

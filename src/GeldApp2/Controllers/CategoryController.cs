using System;
using System.Threading.Tasks;
using GeldApp2.Application.Commands;
using GeldApp2.Application.Commands.Category;
using GeldApp2.Application.ML;
using GeldApp2.Application.Queries.Category;
using GeldApp2.Database.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator mediator;

        public CategoryController(IMediator mediator)
        {
            this.mediator = mediator;
        }
        
        [HttpGet, Route("/api/account/{accountName}/categories")]
        public async Task<CategoryViewModel[]> GetCategories(string accountName)
        {
            accountName = Uri.UnescapeDataString(accountName);
            return await this.mediator.Send(new GetCategoriesForAccountQuery(accountName));
        }

        [HttpGet, Route("/api/account/{accountName}/categories/predict")]
        public async Task<ActionResult<CategoryPredictionResult>> PredictCategory(string accountName, float amount, DateTime created, DateTime expenseDate)
        {
            accountName = Uri.UnescapeDataString(accountName);
            var result = await this.mediator.Send(new PredictCategoryCommand(accountName, amount, created, expenseDate));
            if (result == CategoryPredictionResult.Empty)
                return this.NoContent();

            return result;
        }
        
        [HttpPost("/api/account/{accountName}/categories")]
        public async Task Create(string accountName, [FromBody]CreateCategoryCommand cmd)
        {
            accountName = Uri.UnescapeDataString(accountName);
            cmd.AccountName = accountName;
            await this.mediator.Send(cmd);
        }

        [HttpDelete("/api/account/{accountName}/category/{categoryName}")]
        public async Task DeleteCategory(string accountName, string categoryName)
        {
            accountName = Uri.UnescapeDataString(accountName);
            var cmd = new DeleteCategoryCommand
            {
                AccountName = accountName,
                CategoryName = categoryName
            };

            await this.mediator.Send(cmd);
        }

        [HttpPut("/api/account/{accountName}/category/{categoryName}/{subcategoryName}")]
        public async Task CreateSubcategory(string accountName, string categoryName, string subcategoryName)
        {
            var cmd = new CreateSubcategoryCommand();
            accountName = Uri.UnescapeDataString(accountName);
            cmd.CategoryName = Uri.UnescapeDataString(categoryName);
            cmd.SubcategoryName = Uri.UnescapeDataString(subcategoryName);
            cmd.AccountName = accountName;
            await this.mediator.Send(cmd);
        }

        [HttpDelete("/api/account/{accountName}/category/{categoryName}/{subcategoryName}")]
        public async Task DeleteSubcategory(string accountName, string categoryName, string subcategoryName)
        {
            accountName = Uri.UnescapeDataString(accountName);
            var cmd = new DeleteSubcategoryCommand
            {
                AccountName = accountName,
                CategoryName = categoryName,
                SubcategoryName = subcategoryName
            };

            await this.mediator.Send(cmd);
        }
    }
}
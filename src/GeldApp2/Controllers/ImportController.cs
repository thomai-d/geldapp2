using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeldApp2.Application.Commands.Import;
using GeldApp2.Application.Queries.Export;
using GeldApp2.Application.Queries.Import;
using GeldApp2.Database;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private static int MaxImportLength = 1024 * 1024 * 1024;

        private readonly IMediator mediator;

        public ImportController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("/api/account/{accountName}/imports/unhandled")]
        public async Task<ActionResult<ImportedExpense[]>> GetUnhandledImportedExpenses(string accountName)
        {
            accountName = Uri.UnescapeDataString(accountName);
            var query = new GetUnhandledImportedExpensesQuery(accountName);
            return await this.mediator.Send(query);
        }

        [HttpPost("/api/account/{accountName}/import/link")]
        public async Task<IActionResult> LinkImportedExpense(string accountName, [FromQuery]long importedExpenseId, [FromQuery]long relatedExpenseId)
        {
            accountName = Uri.UnescapeDataString(accountName);
            var query = new LinkImportedExpenseCommand(accountName, importedExpenseId, relatedExpenseId);
            await this.mediator.Send(query);
            return Ok();
        }
        
        [HttpPost("/api/account/{accountName}/imports/{id}/handle")]
        public async Task<IActionResult> HandleImportedExpense(string accountName, long id)
        {
            accountName = Uri.UnescapeDataString(accountName);
            var query = new HandleImportedExpenseCommand(accountName, id);
            await this.mediator.Send(query);
            return Ok();
        }

        [HttpPost("/api/account/{accountName}/import/csv")]
        public async Task<IActionResult> ImportCsv(string accountName, IFormFile csvFile)
        {
            accountName = Uri.UnescapeDataString(accountName);
            if (csvFile.Length > MaxImportLength)
            {
                return this.StatusCode(StatusCodes.Status413PayloadTooLarge, new { error = "File is too large " });
            }

            using (var str = csvFile.OpenReadStream())
            {
                using (var reader = new StreamReader(str, Encoding.UTF7))
                {
                    var content = await reader.ReadToEndAsync();
                    var cmd = new ImportCsvCommand(accountName, content);
                    await this.mediator.Send(cmd);
                    return this.Ok();
                }
            }
        }
    }
}
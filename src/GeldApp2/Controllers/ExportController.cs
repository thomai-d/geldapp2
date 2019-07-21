using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeldApp2.Application.Queries.Export;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly IMediator mediator;

        public ExportController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("/api/account/{accountName}/export/tsv")]
        public async Task<FileStreamResult> GetTsvExport(string accountName)
        {
            accountName = Uri.UnescapeDataString(accountName);

            var stream = await this.mediator.Send(new GetTsvExportStreamQuery(accountName));
            return new FileStreamResult(stream, "text/tab-separated-values")
            {
                FileDownloadName = $"{accountName}.tsv"
            };
        }
    }
}
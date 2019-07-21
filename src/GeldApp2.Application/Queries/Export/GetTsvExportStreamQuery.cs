using GeldApp2.Application.Commands;
using GeldApp2.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries.Export
{
    public class GetTsvExportStreamQuery : AccountRelatedRequest<Stream>
    {
        public GetTsvExportStreamQuery(string accountName)
            : base(accountName)
        {
        }
    }

    public class GetTsvExportStreamQueryHandler : IRequestHandler<GetTsvExportStreamQuery, Stream>
    {
        private readonly GeldAppContext db;

        public GetTsvExportStreamQueryHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<Stream> Handle(GetTsvExportStreamQuery request, CancellationToken cancellationToken)
        {
            var expenses = await this.db.Expenses
                                  .Where(e => e.AccountId == request.Account.Id)
                                  .ToArrayAsync();

            var mem = new MemoryStream();
            var stream = new StreamWriter(mem);
            stream.Write("Id\t");
            stream.Write("AccountId\t");
            stream.Write("Category\t");
            stream.Write("Subcategory\t");
            stream.Write("Details\t");
            stream.Write("Amount\t");
            stream.Write("Date\t");
            stream.Write("Type\t");
            stream.Write("Created\t");
            stream.Write("CreatedBy\t");
            stream.Write("LastModified\t");
            stream.WriteLine();

            foreach (var expense in expenses)
            {
                stream.Write($"{expense.Id}\t");
                stream.Write($"{expense.AccountId}\t");
                stream.Write($"{expense.Category}\t");
                stream.Write($"{expense.Subcategory}\t");
                stream.Write($"{expense.Details}\t");
                stream.Write($"{expense.Amount.ToString("0.00", CultureInfo.InvariantCulture)}\t");
                stream.Write($"{expense.Date:yyyy-MM-dd}\t");
                stream.Write($"{expense.Type}\t");
                stream.Write($"{expense.Created:o}\t");
                stream.Write($"{expense.CreatedBy}\t");
                stream.Write($"{expense.LastModifiedBy}\t");
                stream.WriteLine();
            }

            stream.Flush();
            mem.Seek(0, SeekOrigin.Begin);
            return mem;
        }
    }
}

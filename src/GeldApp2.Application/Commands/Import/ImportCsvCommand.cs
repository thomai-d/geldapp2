using FluentValidation;
using GeldApp2.Application.Logging;
using GeldApp2.Application.Services;
using GeldApp2.Database;
using GeldApp2.Extensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Import
{
    public class ImportCsvCommand : AccountRelatedRequest<bool>, ILoggable, ICommand
    {
        public ImportCsvCommand(string accountName, string content)
            : base(accountName)
        {
            this.Content = content;
        }

        public string Content { get; set; }

        public virtual void EmitLog(LogEventDelegate log, bool success)
        {
            if (success)
                log(Events.ImportCsvFile, "{Account} imported a csv file", this.AccountName);
            else
                log(Events.ImportCsvFile, "Import a csv file for {Account} failed", this.AccountName);
        }
    }

    public class ImportCsvCommandHandler : IRequestHandler<ImportCsvCommand, bool>
    {
        private readonly GeldAppContext db;
        private readonly IDkbCsvParser dkbCsvParser;

        public ImportCsvCommandHandler(
            GeldAppContext db,
            IDkbCsvParser dkbCsvParser,
            IValidator<Database.Expense> expenseValidator)
        {
            this.db = db;
            this.dkbCsvParser = dkbCsvParser;
        }

        public async Task<bool> Handle(ImportCsvCommand cmd, CancellationToken cancellationToken)
        {
            var importedExpenses = this.dkbCsvParser.Parse(cmd.Content).ToArray();

            foreach (var item in importedExpenses)
                item.AccountId = cmd.Account.Id;

            var thingsToAdd = importedExpenses
                                .Where(this.NotAlreadyPresentInDatabase)
                                .ToArray();

            if (!thingsToAdd.Any())
                return true;

            this.db.ImportedExpenses.AddRange(thingsToAdd);
            await this.db.SaveChangesAsync();

            return true;
        }

        private bool NotAlreadyPresentInDatabase(ImportedExpense import)
        {
            return !this.db.ImportedExpenses.Any(dbex => dbex.AccountId == import.AccountId
                                                      && dbex.BookingDay == import.BookingDay
                                                      && dbex.AccountNumber == import.AccountNumber
                                                      && dbex.Amount == import.Amount
                                                      && dbex.Detail == import.Detail);
        }
    }
}

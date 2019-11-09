using GeldApp2.Application.Exceptions;
using GeldApp2.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GeldApp2.Application.Services
{
    public interface IDkbCsvParser
    {
        IEnumerable<ImportedExpense> Parse(string csv);
    }

    public class DkbCsvParser : IDkbCsvParser
    {
        private static Regex MagicStringRx = new Regex("^\"Kontonummer:\";\"DE\\d{20}\\s.*?\";$");

        public IEnumerable<ImportedExpense> Parse(string csv)
        {
            var lines = csv.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 6
             || !MagicStringRx.IsMatch(lines[0])
             || !lines[4].StartsWith("\"Buchungstag\""))
                throw new UserException("Keine gültige DKB-CSV-Datei!");

            var germanCulture = new CultureInfo("DE");
            foreach (var line in lines.Skip(5))
            {
                var parts = line.Split(new char[] { ';' })
                                .Select(p => p.Trim(new[] { '"' }))
                                .ToArray();

                if (parts.Length != 12)
                    throw new UserException("Keine gültige DKB-CSV-Datei!");

                var amount = decimal.Parse(parts[7], germanCulture);
                if (amount == 0)
                    continue;

                yield return new ImportedExpense
                {
                    AccountNumber = parts[5],
                    Amount = amount,
                    BankingCode = parts[6],
                    BookingDay = DateTime.Parse(parts[0], germanCulture),
                    DebteeId = parts[8],
                    Detail = parts[4],
                    Partner = parts[3],
                    Imported = DateTimeOffset.Now,
                    Reference1 = parts[9],
                    Reference2 = parts[10],
                    Type = parts[2],
                    Valuta = DateTime.Parse(parts[1], germanCulture)
                };
            }
        }
    }
}

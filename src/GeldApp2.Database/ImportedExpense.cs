using System;
using System.Collections.Generic;
using System.Text;

namespace GeldApp2.Database
{
    /// <summary>
    /// Expense that has been imported from an external system (ie. banking account).
    /// </summary>
    public class ImportedExpense
    {
        public long Id { get; set; }

        public DateTime BookingDay { get; set; }
        
        public DateTime Valuta { get; set; }
        
        public DateTimeOffset Imported { get; set; }
        
        /// <summary>
        /// Type, eg. "Lastschrift" or "Gutschrift".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// "Auftraggeber / Begünstigter"
        /// </summary>
        public string Partner { get; set; }
        
        public string AccountNumber { get; set; }

        public string BankingCode { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// "Gläubiger-ID"
        /// </summary>
        public string DebteeId { get; set; }
        
        /// <summary>
        /// "Verwendungszweck"
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// "Mandatsreferenz"
        /// </summary>
        public string Reference1 { get; set; }
        
        /// <summary>
        /// "Kundenreferenz"
        /// </summary>
        public string Reference2 { get; set; }

        /// <summary>
        /// True if this expense has already been handled.
        /// </summary>
        public bool IsHandled { get; set; }

        /// <summary>
        /// Expense which has been generated from this import (optional).
        /// </summary>
        public long? ExpenseId { get; set; }
        public Expense Expense { get; set; }

        public long AccountId { get; set; }
        public Account Account { get; set; }
    }
}

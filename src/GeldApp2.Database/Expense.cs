using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database
{
    public class Expense
    {
        public Expense()
        {
        }

        public Expense(decimal amount, string category, string subcategory)
            : this(amount, category, subcategory, string.Empty)
        {
        }

        public Expense(decimal amount, string category, string subcategory, string details)
        {
            this.Amount = amount;
            this.Category = category;
            this.Subcategory = subcategory;
            this.Details = details;
            this.Type = amount < 0 ? ExpenseType.Expense : ExpenseType.Revenue;
            this.Created = DateTimeOffset.Now;
            this.Date = this.Created.Date;
            this.LastModified = this.Created;
        }

        public long Id { get; set; }

        public long AccountId { get; set; }

        public string Category { get; set; }

        public string Subcategory { get; set; }

        public string Details { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public ExpenseType Type { get; set; }

        public DateTimeOffset Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset LastModified { get; set; }

        public string LastModifiedBy { get; set; }

        /* Navigation properties */

        public Account Account { get; set; }
    }
}

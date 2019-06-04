using GeldApp2.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database.ViewModels
{
    public class ExpenseViewModel
    {
        public static ExpenseViewModel FromDb(Expense xp)
        {
            return new ExpenseViewModel
            {
                Id = xp.Id,
                AccountName = xp.Account.Name,
                Amount = xp.Amount,
                CategoryName = xp.Category,
                Date = xp.Date,
                Details = xp.Details,
                SubcategoryName = xp.Subcategory,
                Type = FirstCharToLower(xp.Type.ToString()),
                Created = xp.Created,
                CreatedBy = xp.CreatedBy,
                LastModified = xp.LastModified,
                LastModifiedBy = xp.LastModifiedBy
            };
        }

        public long Id { get; set; }

        public string AccountName { get; set; }

        public decimal Amount { get; set; }

        public string CategoryName { get; set; }

        public string SubcategoryName { get; set; }

        public string Details { get; set; }

        public DateTime Date { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset LastModified { get; set; }

        public string Type { get; set; }

        public string CreatedBy { get; set; }

        public string LastModifiedBy { get; set; }

        // TODO Abstrakt
        public static string FirstCharToLower(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
            {
                return str;
            }

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}

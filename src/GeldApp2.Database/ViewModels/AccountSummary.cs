using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database.ViewModels
{
    public class AccountSummary
    {
        public string AccountName { get; private set; }

        public decimal TotalExpenses { get; private set; }
    }
}

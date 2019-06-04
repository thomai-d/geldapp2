using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database.ViewModels
{
    public class ExpenseRevenueLineChartsDto
    {
        public List<DateLineChartDto> Expense { get; set; } = new List<DateLineChartDto>();

        public List<DateLineChartDto> Revenue { get; set; } = new List<DateLineChartDto>();
    }
}

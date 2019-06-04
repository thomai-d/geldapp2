using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database.ViewModels
{
    /// <summary>
    /// ViewModel for items grouped by month / year.
    /// </summary>
    public class MonthlyDataItem
    {
        public int Month { get; set; }

        public int Year { get; set; }

        public decimal Amount { get; set; }
    }

    public static class MonthlyDataItemsExtension
    {
        public static IEnumerable<MonthlyDataItem> WithoutGaps(this IEnumerable<MonthlyDataItem> items)
        {
            var lastMonth = 0;
            var additions = 0;

            foreach (var item in items)
            {
                var nextMonth = (item.Month - 1) + item.Year * 12;
                var thisMonth = lastMonth + 1;
                while (lastMonth != 0 && thisMonth < nextMonth)
                {
                    var m = (thisMonth % 12) + 1;
                    var y = thisMonth / 12;
                    thisMonth++;

                    additions++;
                    if (additions > 1000)
                        throw new InvalidOperationException("Aborting after 1000 additions.");

                    yield return new MonthlyDataItem { Month = m, Year = y, Amount = 0 };
                }

                lastMonth = nextMonth;
                yield return item;
            }
        }
    }
}

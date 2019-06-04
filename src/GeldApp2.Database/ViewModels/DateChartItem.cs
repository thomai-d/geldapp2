using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database.ViewModels
{
    public class DateChartItem
    {
        public static DateChartItem From(MonthlyDataItem item)
        {
            return new DateChartItem { X = new DateTime(item.Year, item.Month, 1), Y = (double)item.Amount };
        }

        public DateTime X { get; set; }

        public double Y { get; set; }
    }
}

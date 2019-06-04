using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database.ViewModels
{
    public class DateLineChartDto
    {
        public DateLineChartDto(string name, IEnumerable<DateChartItem> items)
        {
            this.Name = name;
            this.Items = items.ToArray();
        }

        public string Name { get; }

        public DateChartItem[] Items { get; }

        /// <summary>
        /// Optional content.
        /// </summary>
        public object Tag { get; set; }
    }
}

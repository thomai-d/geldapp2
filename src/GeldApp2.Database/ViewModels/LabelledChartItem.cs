using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database.ViewModels
{
    /// <summary>
    /// ViewModel for a ChartJS labelled chart item.
    /// </summary>
    public class LabelledChartItem
    {
        public decimal Y { get; set; }

        public string Label { get; set; }
    }
}
